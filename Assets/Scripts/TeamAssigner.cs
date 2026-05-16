using UnityEngine;
using Mirror;

public class TeamAssigner : NetworkBehaviour
{
    // SyncVar гарантирует, что все клиенты знают текущее кол-во игроков
    [SyncVar] private int playersJoined = 0;

    // Этот метод вызывается Игроком, но выполняется на Сервере
    [Command(requiresAuthority = false)]
    public void CmdAssignTeam(GameObject playerObject)
    {
        PlayerStats stats = playerObject.GetComponent<PlayerStats>();
        if (stats != null)
        {
            // Назначаем команду (0 или 1) и увеличиваем счетчик
            stats.team = playersJoined % 2;
            playersJoined++;
            
            Debug.Log($"[SERVER] Игроку {playerObject.name} назначена команда {stats.team}");
            
            // После назначения команды — отправляем игрока на его базу
            TargetMoveToSpawn(playerObject.GetComponent<NetworkIdentity>().connectionToClient, playerObject, stats.team);
        }
    }

    [TargetRpc]
    private void TargetMoveToSpawn(NetworkConnection target, GameObject player, int team)
    {
        // Ищем точки спавна на сцене (те, что мы назвали Spawn_Team0 и Spawn_Team1)
        NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        foreach (var point in spawnPoints)
        {
            if (point.name.Contains("Team" + team))
            {
                // Если у тебя NavMeshAgent — используем Warp
                if (player.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
                {
                    agent.Warp(point.transform.position);
                }
                else
                {
                    player.transform.position = point.transform.position;
                }
                break;
            }
        }
    }
}