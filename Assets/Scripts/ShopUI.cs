using UnityEngine;
using UnityEngine.UI;
using TMPro;

    public class ShopUI : MonoBehaviour
    {
        [Header("Upgrade Buttons")]
        public Button rangeButton;
        public Button speedButton;
        public Button widthButton;
        public Button pierceButton;
        
        [Header("Upgrade Indicators")]
        public UpgradeIndicator rangeIndicator;
        public UpgradeIndicator speedIndicator;
        public UpgradeIndicator widthIndicator;
        public UpgradeIndicator pierceIndicator;
        
        [Header("Price Labels")]
        public TextMeshProUGUI rangePriceText;
        public TextMeshProUGUI speedPriceText;
        public TextMeshProUGUI widthPriceText;
        public TextMeshProUGUI piercePriceText;
        
        private PlayerStats localStats;
        
        void Start()
    {
        // Если панель отключена, Start не вызовется
        // Поэтому подписываемся в OnEnable
    }

    void OnEnable()
    {
        SubscribeToButtons();
    }

    void OnDisable()
    {
        UnsubscribeFromButtons();
    }

    void SubscribeToButtons()
    {
        if (rangeButton != null)
            rangeButton.onClick.AddListener(() => BuyUpgrade("range"));
        if (speedButton != null)
            speedButton.onClick.AddListener(() => BuyUpgrade("speed"));
        if (widthButton != null)
            widthButton.onClick.AddListener(() => BuyUpgrade("width"));
        if (pierceButton != null)
            pierceButton.onClick.AddListener(() => BuyUpgrade("pierce"));
        Debug.Log($"саб");
    }

    void UnsubscribeFromButtons()
    {
        if (rangeButton != null)
            rangeButton.onClick.RemoveAllListeners();
        if (speedButton != null)
            speedButton.onClick.RemoveAllListeners();
        if (widthButton != null)
            widthButton.onClick.RemoveAllListeners();
        if (pierceButton != null)
            pierceButton.onClick.RemoveAllListeners();
        Debug.Log($"ансаб");
    }
    
    void FindLocalPlayer()
    {
        foreach (var p in FindObjectsOfType<PlayerStats>())
        {
            if (p.isLocalPlayer)
            {
                localStats = p;
                break;
            }
        }
        Debug.Log($"нашелся");
    }
    
    void Update()
    {
        if (localStats == null)
        {
            FindLocalPlayer();
            return;
        }
        
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (localStats == null) return;
        
        // Обновляем индикаторы
        if (rangeIndicator) rangeIndicator.UpdateLevel(localStats.rangeLvl);
        if (speedIndicator) speedIndicator.UpdateLevel(localStats.speedLvl);
        if (widthIndicator) widthIndicator.UpdateLevel(localStats.widthLvl);
        if (pierceIndicator) pierceIndicator.UpdateLevel(localStats.pierceLvl);
        
        // Обновляем цены
        if (rangePriceText) rangePriceText.text = GetPrice("range", localStats.rangeLvl).ToString();
        if (speedPriceText) speedPriceText.text = GetPrice("speed", localStats.speedLvl).ToString();
        if (widthPriceText) widthPriceText.text = GetPrice("width", localStats.widthLvl).ToString();
        if (piercePriceText) piercePriceText.text = GetPrice("pierce", localStats.pierceLvl).ToString();
        
        // БЛОКИРУЕМ/РАЗБЛОКИРУЕМ КНОПКИ
        if (rangeButton) rangeButton.interactable = CanBuy("range");
        if (speedButton) speedButton.interactable = CanBuy("speed");
        if (widthButton) widthButton.interactable = CanBuy("width");
        if (pierceButton) pierceButton.interactable = CanBuy("pierce");
    }
    
    int GetPrice(string type, int currentLevel)
    {
        switch (type)
        {
            case "range": return 200 + (currentLevel * 70);
            case "speed": return 300 + (currentLevel * 100);
            case "width": return 400 + (currentLevel * 200);
            case "pierce": return 600 + (currentLevel * 400);
            default: return 999999;
        }
    }
    
    bool CanBuy(string type)
    {
        if (localStats == null) return false;
        
        int currentLevel = 0;
        int maxLevel = 0;
        
        switch (type)
        {
            case "range": currentLevel = localStats.rangeLvl; maxLevel = 5; break;
            case "speed": currentLevel = localStats.speedLvl; maxLevel = 5; break;
            case "width": currentLevel = localStats.widthLvl; maxLevel = 3; break;
            case "pierce": currentLevel = localStats.pierceLvl; maxLevel = 2; break;
        }
        
        if (currentLevel >= maxLevel) return false;
        
        int price = GetPrice(type, currentLevel);
        return localStats.gold >= price;
    }
    
    public void BuyUpgrade(string upgradeType)
    {
        Debug.Log($"Попытка купить {upgradeType}"); // Проверка, вызывается ли метод
        
        if (localStats == null)
        {
            Debug.LogError("localStats = null!");
            return;
        }
        
        if (!CanBuy(upgradeType))
        {
            Debug.Log($"Нельзя купить {upgradeType}: недостаточно золота или макс. уровень");
            return;
        }
        
        Debug.Log($"Отправка команды на сервер для {upgradeType}");
        localStats.CmdBuyUpgrade(upgradeType);
    }
}