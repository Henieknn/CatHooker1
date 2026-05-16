using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerDash : NetworkBehaviour
{
    [Header("Settings")]
    public float dashDistance = 2f;    // Дистанция рывка
    public float dashDuration = 0.2f;  // Время самого рывка (чем меньше, тем быстрее)
    public float cooldown = 15f;       // КД из ТЗ

    public float nextDashTime;
    private bool isDashing = false;
    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Ввод на клавишу Space или E (выбери подходящую по ТЗ)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time >= nextDashTime && !isDashing)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        nextDashTime = Time.time + cooldown;
        
        // Направление рывка — куда смотрит персонаж в данный момент
        Vector3 dashDirection = transform.forward;
        
        // Запускаем процесс рывка
        StartCoroutine(DashCoroutine(dashDirection));
        
        // Сообщаем серверу, чтобы он передвинул нас для всех остальных
        CmdRequestDash(dashDirection);
    }

    // Плавное перемещение на клиенте для мгновенного отклика
    IEnumerator DashCoroutine(Vector3 direction)
    {
        isDashing = true;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            // Двигаем игрока
            transform.position += direction * (dashDistance / dashDuration) * Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    [Command]
    void CmdRequestDash(Vector3 direction)
    {
        // Сервер подтверждает рывок и транслирует позицию другим через NetworkTransform
        // В идеале здесь должна быть проверка КД на сервере для защиты от читов
        RpcPlayDashEffect();
    }

    [ClientRpc]
    void RpcPlayDashEffect()
    {
        // Здесь можно включить Particle System (пыль под ногами или след)
        Debug.Log("Dash Effect!");
    }
}