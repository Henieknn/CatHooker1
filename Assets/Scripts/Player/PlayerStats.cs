using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerStats : NetworkBehaviour
{
    [Header("Economy")]
    // SyncVar обновляет переменную у клиентов. 
    // hook вызывает функцию OnGoldChanged при каждом обновлении.
    [SyncVar(hook = nameof(OnGoldChanged))]
    public int gold = 0;

    [SyncVar]
    public int kills = 0;

    [Header("Hook Upgrades (0-10 lvl)")]
    [SyncVar] public int rangeLvl = 0;
    [SyncVar] public int speedLvl = 0;
    [SyncVar] public int widthLvl = 0;
    [SyncVar] public int pierceLvl = 0;
    [SyncVar] public int deaths = 0;
    [SyncVar] public int team; // 0 или 1
    [SyncVar] public bool isRooted = false;
    [SyncVar] public int baseUpgradePrice = 10;

    // Статическое событие, на которое подписывается UIManager
    public static event System.Action<int> OnLocalGoldUpdate;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // Сразу уведомляем UI о стартовом золоте
        OnLocalGoldUpdate?.Invoke(gold);
        // Ищем на сцене наш раздатчик команд и просим назначить нам сторону
        TeamAssigner assigner = FindObjectOfType<TeamAssigner>();
        if (assigner != null)
        {
            assigner.CmdAssignTeam(gameObject);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        // Красим кота в цвет команды (например, синий и красный)
        GetComponentInChildren<Renderer>().material.color = (team == 0) ? Color.blue : Color.red;
    }

    // Срабатывает только на Сервере (Хосте)
    public override void OnStartServer()
    {
        base.OnStartServer();
        // Запускаем корутину или повтор функции для пассивного дохода
        InvokeRepeating(nameof(AddPassiveGold), 1f, 1f);
    }

    [Server]
    void AddPassiveGold()
    {
        gold += 2;
    }

    // Функция-хук для синхронизации UI
    void OnGoldChanged(int oldGold, int newGold)
    {
        // Выполняем только для владельца этого персонажа
        if (isLocalPlayer)
        {
            OnLocalGoldUpdate?.Invoke(newGold);
        }
    }

    [Command]
    public void CmdBuyUpgrade(string upgradeType)
    {
        Debug.Log($"[Server] CmdBuyUpgrade вызван для {upgradeType}, золото: {gold}");
        
        int currentPrice = CalculatePrice(upgradeType);
        
        if (gold >= currentPrice)
        {
            bool upgraded = false;
            
            switch (upgradeType)
            {
                case "range":
                    if (rangeLvl < 5) { rangeLvl++; upgraded = true; }
                    break;
                case "speed":
                    if (speedLvl < 5) { speedLvl++; upgraded = true; }
                    break;
                case "width":
                    if (widthLvl < 3) { widthLvl++; upgraded = true; }
                    break;
                case "pierce":
                    if (pierceLvl < 2) { pierceLvl++; upgraded = true; }
                    break;
            }
            
            if (upgraded)
            {
                gold -= currentPrice;
                Debug.Log($"[Server] {upgradeType} улучшен до {GetLevel(upgradeType)}. Осталось золота: {gold}");
            }
            else
            {
                Debug.Log($"[Server] Не удалось улучшить {upgradeType} (макс. уровень)");
            }
        }
        else
        {
            Debug.Log($"[Server] Недостаточно золота для {upgradeType}. Нужно: {currentPrice}, есть: {gold}");
        }
    }

    int CalculatePrice(string type)
    {
        int lvl = GetLevel(type);
        
        switch (type)
        {
            case "range":
                // Старт 200, шаг 70. Уровни: 200, 270, 340, 410, 480.
                return 200 + (lvl * 70);
                
            case "speed":
                // Старт 300, шаг 100. Уровни: 300, 400, 500, 600, 700.
                return 300 + (lvl * 100);
                
            case "width":
                // Ширина — очень сильный стат. Старт 400, шаг 200.
                // Уровни: 400, 600, 800. Всего 3 уровня.
                return 400 + (lvl * 200);
                
            case "pierce":
                // Пробитие — элитный навык. Старт 600, шаг 400.
                // Уровни: 600, 1000. Всего 2 уровня.
                return 600 + (lvl * 400);
                
            default:
                return 999999;
        }
    }

    int GetLevel(string type)
    {
        switch (type)
        {
            case "range": return rangeLvl;
            case "speed": return speedLvl;
            case "width": return widthLvl;
            case "pierce": return pierceLvl;
            default: return 0;
        }
    }

    [Server]
    public void ServerApplyRoot(float duration)
    {
        isRooted = true;
        // Останавливаем старую корутину, если кот наступил во вторую ловушку сразу
        StopCoroutine(nameof(UnrootRoutine)); 
        StartCoroutine(UnrootRoutine(duration));
    }

    private IEnumerator UnrootRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isRooted = false;
    }
}