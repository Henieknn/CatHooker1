using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Team Score")]
    public TextMeshProUGUI teamScoreText;

    [Header("Personal Stats")]
    public TextMeshProUGUI personalStatsText;

    [Header("Skills CD Masks")]
    public Image hookCDMask;
    public Image dashCDMask;
    public Image trapCDMask;
    public Image shieldCDMask; // Четвертая маска

    [Header("Respawn")]
    public GameObject respawnPanel;
    public TextMeshProUGUI respawnTimerText;

    private PlayerStats localStats;
    private ClickToMove localMovement;
    private PlayerShooting localHook;
    private PlayerDash localDash; 
    private PlayerTrapSetter localTrap;
    private PlayerShield localShield;

    [Header("Upgrade Levels")]
    public TextMeshProUGUI rangeLevelText;
    public TextMeshProUGUI speedLevelText;
    public TextMeshProUGUI widthLevelText;
    public TextMeshProUGUI pierceLevelText;

    void Update()
    {
        if (localStats == null)
        {
            FindLocalPlayer();
            return;
        }

        UpdateScore();
        UpdateCooldowns();
        UpdateRespawnUI();
    }

    void FindLocalPlayer()
    {
        foreach (var p in FindObjectsOfType<PlayerStats>())
        {
            if (p.isLocalPlayer)
            {
                localStats = p;
                localMovement = p.GetComponent<ClickToMove>();
                localHook = p.GetComponent<PlayerShooting>();
                localDash = p.GetComponent<PlayerDash>();
                localTrap = p.GetComponent<PlayerTrapSetter>();
                localShield = p.GetComponent<PlayerShield>();
                break;
            }
        }
    }

    void UpdateCooldowns()
    {
        // Логика: (Время_до_отката - Текущее_время) / Общее_время_отката
        
        if (localHook) UpdateMask(hookCDMask, localHook.nextReadyTime, localHook.cooldown);
        if (localDash) UpdateMask(dashCDMask, localDash.nextDashTime, localDash.cooldown);
        if (localTrap) UpdateMask(trapCDMask, localTrap.nextTrapTime, localTrap.cooldown);
        if (localShield) UpdateMask(shieldCDMask, localShield.nextShieldTime, localShield.cooldown);
    }

    void UpdateMask(Image mask, float nextReadyTime, float cooldown)
    {
        if (mask == null) return;
        
        float remaining = nextReadyTime - Time.time;
        if (remaining > 0 && cooldown > 0)
        {
            mask.fillAmount = remaining / cooldown;
        }
        else
        {
            mask.fillAmount = 0;
        }
    }

    void UpdateScore()
    {
        int scoreTeam0 = 0;
        int scoreTeam1 = 0;
        
        foreach (var p in FindObjectsOfType<PlayerStats>())
        {
            if (p.team == 0) scoreTeam0 += p.kills;
            else scoreTeam1 += p.kills;
        }

        teamScoreText.text = $"<color=blue>TEAM A: {scoreTeam0}</color> | <color=red>TEAM B: {scoreTeam1}</color>";
        personalStatsText.text = $"K/D: {localStats.kills}/{localStats.deaths}\nGold: {localStats.gold}";
    }

    void UpdateUpgradeLevels()
    {
    if (localStats == null) return;
    
    if (rangeLevelText) rangeLevelText.text = $"Lvl {localStats.rangeLvl}/5";
    if (speedLevelText) speedLevelText.text = $"Lvl {localStats.speedLvl}/5";
    if (widthLevelText) widthLevelText.text = $"Lvl {localStats.widthLvl}/3";
    if (pierceLevelText) pierceLevelText.text = $"Lvl {localStats.pierceLvl}/2";
    }

    void UpdateRespawnUI()
    {
        if (localMovement == null) return;

        bool isDead = localMovement.GetIsDead();
        respawnPanel.SetActive(isDead);

        if (isDead)
        {
            // Берем значение из нашего нового таймера
            float timeLeft = localMovement.timeUntilRespawn;
            
            // Если время еще есть, пишем секунды, если меньше 0 — пишем "Ready"
            if (timeLeft > 0) {
                respawnTimerText.text = $"RESPAWN IN: {timeLeft:F1}s";
            } else {
                respawnTimerText.text = "PREPARING...";
            }
        }
    }
}