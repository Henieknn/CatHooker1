using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShield : NetworkBehaviour
{
    public GameObject shieldVisual; // Ссылка на визуальную сферу внутри префаба
    public float duration = 0.5f;
    public float cooldown = 18f;

    public float nextShieldTime;
    [SyncVar(hook = nameof(OnShieldStatusChanged))]
    private bool isShieldActive = false;

    void Start()
    {
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Keyboard.current.wKey.wasPressedThisFrame && Time.time >= nextShieldTime)
        {
            nextShieldTime = Time.time + cooldown;
            CmdActivateShield();
        }
    }

    [Command]
    void CmdActivateShield()
    {
        isShieldActive = true;
        StartCoroutine(ShieldTimer());
    }

    IEnumerator ShieldTimer()
    {
        yield return new WaitForSeconds(duration);
        isShieldActive = false;
    }

    // Синхронизация видимости щита для всех игроков
    void OnShieldStatusChanged(bool oldStatus, bool newStatus)
    {
        if (shieldVisual != null) shieldVisual.SetActive(newStatus);
    }

    // Логика блокировки хука на сервере
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!isShieldActive) return;

        if (other.CompareTag("Hook"))
        {
            Debug.Log("Щит заблокировал хук!");
            NetworkServer.Destroy(other.gameObject);
        }
    }
}