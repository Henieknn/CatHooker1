using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Collections;
using UnityEngine.InputSystem;

public class ClickToMove : NetworkBehaviour 
{
    private NavMeshAgent agent;
    private bool isDead = false; // Блокировка действий при смерти
    public bool GetIsDead() => isDead;
    [SyncVar] public float timeUntilRespawn = 0; // Переменная для UI
    public float respawnDuration = 3f;
    private Animator anim;
    private bool canMove = true; // По умолчанию ходить можно

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        if (anim == null) Debug.LogError("Аниматор не найден на коте!");
    }

    void Update() {
        if (!isLocalPlayer || isDead) return;
        // 1. Обработка состояния "Обездвижен" (Root)
        if (GetComponent<PlayerStats>().isRooted) {
            // Если мы только что попали в ловушку, сбрасываем путь агента
            if (agent.hasPath) agent.ResetPath();
            
            // Обнуляем анимацию скорости, чтобы кот не бежал на месте
            if (anim != null) anim.SetFloat("Speed", 0f);
            
            // Прекращаем выполнение Update, чтобы нельзя было кликать
            return; 
        }

        // 2. Логика движения (только если не в руте и не мертв)
        if (canMove && Mouse.current.rightButton.wasPressedThisFrame) {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                agent.SetDestination(hit.point); 
            }
        }

        // 3. Логика анимации (обновляется, только если мы живы и можем ходить)
        if (anim != null) {
            // Используем magnitude для анимации бега
            float moveSpeed = agent.velocity.magnitude;
            anim.SetFloat("Speed", moveSpeed);
        }
    }
    public void SetMovementLock(bool lockState)
    {
        canMove = !lockState;
        
        // Если мы заблокировали движение, сразу останавливаем агента
        if (lockState && agent != null)
        {
            agent.ResetPath(); 
            anim.SetFloat("Speed", 0f); // Сразу переключаем в Idle
        }
    }

    // --- ЛОГИКА СМЕРТИ И РЕСПАУНА ---

    public void Die() {
        if (!isServer) return;
        
        GetComponent<PlayerStats>().deaths++;
        timeUntilRespawn = respawnDuration; // Устанавливаем таймер
        
        RpcOnDeath();
        StartCoroutine(RespawnTimer());
    }

    [ClientRpc]
    private void RpcOnDeath() {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence() {
        isDead = true;
        
        // 1. Останавливаем навигацию
        if (agent != null) {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 2. Скрываем игрока (визуально)
        // Предполагаем, что модель кота лежит в дочернем объекте
        foreach (var renderer in GetComponentsInChildren<Renderer>()) {
            renderer.enabled = false;
        }

        Debug.Log("Вы погибли! Ждите респауна...");
        yield return null;
    }

    private IEnumerator RespawnTimer() {
        // Пока таймер больше нуля, уменьшаем его
        while (timeUntilRespawn > 0) {
            yield return new WaitForSeconds(0.1f);
            timeUntilRespawn -= 0.1f;
        }
        PlayerStats stats = GetComponent<PlayerStats>();
        NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        foreach (var point in spawnPoints)
        {
            if (point.name.Contains("Team" + stats.team))
            {
                spawnPos = point.transform.position;
            }
        }

        RpcRespawn(spawnPos);
    }

    [ClientRpc]
    private void RpcRespawn(Vector3 spawnPos) {
        // 1. Перемещаем (важно делать это через NavMeshAgent, если он активен)
        if (agent != null) {
            agent.Warp(spawnPos);
            agent.isStopped = false;
        } else {
            transform.position = spawnPos;
        }

        // 2. Показываем модель обратно
        foreach (var renderer in GetComponentsInChildren<Renderer>()) {
            renderer.enabled = true;
        }

        isDead = false;
        Debug.Log("Вы возродились!");
    }
}