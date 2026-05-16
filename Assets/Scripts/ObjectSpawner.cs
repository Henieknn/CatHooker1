using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
public class ObjectSpawner : NetworkBehaviour
{
    public GameObject collectiblePrefab;

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            CmdSpawnObject();
        }
    }
    [Command]
    void CmdSpawnObject()
    {
        if (collectiblePrefab == null) {
        Debug.LogError("Префаб не назначен в ObjectSpawner!");
        return;
    }

    // Создаем объект на сервере
    GameObject obj = Instantiate(collectiblePrefab, transform.position + Vector3.up, Quaternion.identity);

    // Логируем попытку спауна
    Debug.Log($"Сервер создал объект. Попытка сетевого спауна...");

    // Это магическая строчка, которая "включает" видимость для клиентов
    NetworkServer.Spawn(obj);
    }
}