using UnityEngine;
using Mirror;
using System.Collections;

public class Trap : NetworkBehaviour
{
    [Header("Settings")]
    public float rootDuration = 2f;      // На сколько замирает кот
    public float lifeTime = 10f;         // Время жизни ловушки до деспавна
    
    [HideInInspector] public GameObject owner; // Назначается при спавне в PlayerTrapSetter

    // Вызывается на сервере при создании объекта
    public override void OnStartServer()
    {
        // Запускаем таймер "смерти" ловушки сразу после появления
        Invoke(nameof(DestroyTrap), lifeTime);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в триггер зашел именно игрок
        if (other.CompareTag("Player"))
        {
            // Не даем ловушке сработать на того, кто ее поставил
            if (other.gameObject == owner) return;

            // Пытаемся достать скрипт статов (где лежит переменная рута)
            if (other.TryGetComponent(out PlayerStats targetStats))
            {
                // Если цель уже в руте, можем не тратить ловушку (или наоборот, обновить время)
                targetStats.ServerApplyRoot(rootDuration);
                
                // Удаляем ловушку, так как контакт произошел
                DestroyTrap();
            }
        }
    }

    [Server]
    private void DestroyTrap()
    {
        // Сетевое удаление объекта (он исчезнет у всех клиентов)
        NetworkServer.Destroy(gameObject);
    }
}