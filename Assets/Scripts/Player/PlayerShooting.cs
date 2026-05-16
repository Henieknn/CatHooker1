using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShooting : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Префаб клубка шерсти с NetworkIdentity и HookProjectile")]
    public GameObject hookPrefab;
    
    private PlayerStats stats;
    private LineRenderer aimLine;
    private Camera mainCamera;
    public float cooldown;

    [Header("Settings")]
    [Tooltip("Слой пола для Raycast (чтобы целиться точно по земле)")]
    public LayerMask groundLayer;

    // Состояние прицеливания
    private bool isAiming = false;
    
    // Время, когда хук будет снова готов (локальное для клиента)
    public float nextReadyTime;

    // Свойство для проверки готовности (можно использовать в UI)
    public bool IsReady => Time.time >= nextReadyTime;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        aimLine = GetComponent<LineRenderer>();
        mainCamera = Camera.main;

        // Изначально линия скрыта
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }

    void Update()
    {
        // Управляем только своим персонажем
        if (!isLocalPlayer) return;

        HandleInput();

        if (isAiming)
        {
            DrawAimPreview();
        }
    }

    private void HandleInput()
    {
        cooldown = 10f - (stats.speedLvl * 0.3f);
        // 1. Вход в режим прицеливания по Q
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (!isAiming)
            {
                // Пытаемся прицелиться только если КД прошел
                if (IsReady)
                {
                    isAiming = true;
                    aimLine.enabled = true;
                }
                else
                {
                    Debug.Log($"Хук перезаряжается. Осталось: {nextReadyTime - Time.time:F1} сек.");
                }
            }
            else
            {
                // Если уже целились — выключаем
                CancelAiming();
            }
        }

        // 2. Выстрел по ЛКМ (только в режиме прицеливания)
        if (isAiming && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Финальная проверка КД перед выстрелом
            if (IsReady)
            {
                ExecuteShoot();
            }
        }

        // 3. Отмена по ПКМ
        if (isAiming && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelAiming();
        }
    }

    private void DrawAimPreview()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPoint = hit.point;
            targetPoint.y = transform.position.y; // Выравниваем высоту

            Vector3 direction = (targetPoint - transform.position).normalized;

            // Берем актуальные статы из PlayerStats
            float currentRange = 15f + (stats.rangeLvl * 5f);
            float currentWidth = 0.5f + (stats.widthLvl * 0.15f);

            // Настраиваем линию под размер будущего хука
            aimLine.startWidth = currentWidth;
            aimLine.endWidth = currentWidth;

            aimLine.positionCount = 2;
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, transform.position + direction * currentRange);

            // Кот всегда смотрит туда, куда целится
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void ExecuteShoot()
    {
        GetComponent<NetworkAnimator>().SetTrigger("Throwing");
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPoint = hit.point;
            targetPoint.y = transform.position.y;
            Vector3 shootDirection = (targetPoint - transform.position).normalized;

            // Сбрасываем режим прицеливания
            CancelAiming();

            // Рассчитываем и ставим КД (база 10с, -0.3с за уровень)
            nextReadyTime = Time.time + cooldown;

            // Поворачиваем перед выстрелом для синхронизации
            transform.rotation = Quaternion.LookRotation(shootDirection);

            // Отправляем команду серверу
            CmdFireHook(shootDirection);
        }
    }

    private void CancelAiming()
    {
        isAiming = false;
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }

    [Command]
    private void CmdFireHook(Vector3 direction)
    {
        // Расчет характеристик на сервере (защита от читов)
        float currentSpeed = 25f + (stats.speedLvl * 3f);
        float currentRange = 15f + (stats.rangeLvl * 3f);
        float currentWidth = 0.5f + (stats.widthLvl * 0.15f);

        // Спаун префаба (чуть впереди кота)
        Vector3 spawnPos = transform.position + direction * 1.2f;
        GameObject hook = Instantiate(hookPrefab, spawnPos, Quaternion.LookRotation(direction));

        // Инициализация снаряда
        HookProjectile projectile = hook.GetComponent<HookProjectile>();
        if (projectile != null)
        {
            projectile.speed = currentSpeed;
            projectile.range = currentRange;
            projectile.width = currentWidth;
            projectile.owner = gameObject;
        }

        // Сетевой спаун
        NetworkServer.Spawn(hook);
        RpcApplyShootLock();
        
    }

    [ClientRpc]
        void RpcApplyShootLock()
        {
            if (isLocalPlayer)
            {
                StartCoroutine(ShootLockCoroutine());
            }
        }
        
        IEnumerator ShootLockCoroutine()
        {
            ClickToMove moveScript = GetComponent<ClickToMove>();
            
            if (moveScript != null)
            {
                moveScript.SetMovementLock(true); // Заблокировали
                
                // Длительность паузы должна совпадать с анимацией броска
                // Например, если замах и бросок длятся 0.8 секунды:
                yield return new WaitForSeconds(0.8f); 
                
                moveScript.SetMovementLock(false); // Разблокировали
            }
        }
}