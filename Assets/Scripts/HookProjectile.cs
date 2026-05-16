using UnityEngine;
using Mirror;

public class HookProjectile : NetworkBehaviour
{
    [Header("Settings")]
    // Добавляем SyncVar, чтобы сервер рассылал эти значения клиентам
    [SyncVar(hook = nameof(OnWidthChanged))] public float width;
    [SyncVar] public float speed;
    [SyncVar] public float range;
    
    [HideInInspector] public GameObject owner;

    private Vector3 startPosition;

    void OnWidthChanged(float oldWidth, float newWidth)
    {
        transform.localScale = new Vector3(newWidth, newWidth, newWidth);
    }

    void Start()
    {
        // Запоминаем точку вылета для расчета пройденной дистанции
        startPosition = transform.position;

        // Визуально настраиваем ширину (если будем использовать апгрейд ширины)
        transform.localScale = new Vector3(width, width, width);
    }

    void Update()
    {
        // Движение хука вперед
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Проверка дистанции (только на сервере)
        if (isServer)
        {
            if (Vector3.Distance(startPosition, transform.position) >= range)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats targetStats = other.GetComponent<PlayerStats>();
            PlayerStats ownerStats = owner.GetComponent<PlayerStats>();

            // Если команды разные — засчитываем попадание
            if (targetStats != null && ownerStats != null && targetStats.team != ownerStats.team)
            {
                ApplyWinLogic(other.gameObject);
            }
        }
    }

    [Server]
    private void ApplyWinLogic(GameObject target)
    {
        Debug.Log($"[SERVER] Попадание! {owner.name} убил {target.name}");

        // Начисляем награду убийце
        if (owner.TryGetComponent(out PlayerStats killerStats))
        {
            killerStats.kills++;
            killerStats.gold += 150;
        }

        // Вместо удаления объекта вызываем метод смерти
        if (target.TryGetComponent(out ClickToMove targetMovement))
        {
            targetMovement.Die();
        }

        // А сам хук (снаряд) удаляем, как и раньше
        NetworkServer.Destroy(gameObject);
    }
}