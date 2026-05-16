using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class PlayerTrapSetter : NetworkBehaviour
{
    public GameObject trapProjectilePrefab; // Префаб ЛЕТЯЩЕЙ ловушки
    public LayerMask groundLayer;
    public float cooldown = 20f;
    public float maxRange = 15f;

    private LineRenderer aimLine;
    private bool isAiming = false;
    public float nextTrapTime;

    void Start() {
        aimLine = GetComponent<LineRenderer>(); // Используем тот же или второй LineRenderer
        aimLine.enabled = false;
    }

    void Update() {
        if (!isLocalPlayer) return;

        // Вход в прицеливание на E
        if (Keyboard.current.eKey.wasPressedThisFrame) {
            if (Time.time >= nextTrapTime) {
                isAiming = !isAiming;
                aimLine.enabled = isAiming;
            }
        }

        if (isAiming) {
            DrawArc();
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                ExecuteThrow();
            }
            if (Mouse.current.rightButton.wasPressedThisFrame) {
                isAiming = false;
                aimLine.enabled = false;
            }
        }
    }

    void DrawArc() {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) {
            Vector3 target = hit.point;
            // Ограничиваем дистанцию броска
            if (Vector3.Distance(transform.position, target) > maxRange) {
                target = transform.position + (target - transform.position).normalized * maxRange;
            }

            // Рисуем простую дугу из 10 точек
            int points = 15;
            aimLine.positionCount = points;
            for (int i = 0; i < points; i++) {
                float t = i / (float)(points - 1);
                aimLine.SetPosition(i, CalculateArcPoint(t, transform.position, target));
            }
        }
    }

    Vector3 CalculateArcPoint(float t, Vector3 start, Vector3 end) {
        Vector3 pos = Vector3.Lerp(start, end, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * 5f; // 5f - высота дуги
        return pos;
    }

    void ExecuteThrow() {
        GetComponent<NetworkAnimator>().SetTrigger("Throwing");
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) {
            Vector3 target = hit.point;
            if (Vector3.Distance(transform.position, target) > maxRange) {
                target = transform.position + (target - transform.position).normalized * maxRange;
            }

            CmdThrowTrap(target);
            isAiming = false;
            aimLine.enabled = false;
            nextTrapTime = Time.time + cooldown;
        }
    }

    [Command]
    void CmdThrowTrap(Vector3 target) {
        GameObject projectile = Instantiate(trapProjectilePrefab, transform.position + Vector3.up, Quaternion.identity);
        projectile.GetComponent<TrapProjectile>().targetPos = target;
        NetworkServer.Spawn(projectile);
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