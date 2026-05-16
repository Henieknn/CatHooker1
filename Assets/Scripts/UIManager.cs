using UnityEngine;
using UnityEngine.UI; // Для работы с Legacy UI (Text, Button, Panel)
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("Shop References")]
    public GameObject shopPanel;    // Ссылка на саму панель
    public Text goldText;           // Ссылка на компонент Text (Legacy) внутри кнопки
    public ShopUI shopUI;
    
    private bool isShopOpen = false;

    private void OnEnable()
    {
        PlayerStats.OnLocalGoldUpdate += UpdateGoldDisplay;
    }

    private void OnDisable()
    {
        PlayerStats.OnLocalGoldUpdate -= UpdateGoldDisplay;
    }

    void Start()
    {
        // Проверка на случай, если забыли назначить в инспекторе
    if (shopPanel != null) 
        shopPanel.SetActive(false);
    
    // Находим ShopUI на панели магазина
    if (shopUI == null && shopPanel != null)
        shopUI = shopPanel.GetComponentInChildren<ShopUI>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleShop();
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && isShopOpen)
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        // Вот здесь вылетала ошибка, если shopPanel был пустой
        if (shopPanel != null)
        {
            isShopOpen = !isShopOpen;
            shopPanel.SetActive(isShopOpen);
        }
    }

    void UpdateGoldDisplay(int currentGold)
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
    }
}