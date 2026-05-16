using UnityEngine;
using UnityEngine.UI;

public class UpgradeIndicator : MonoBehaviour
{
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.yellow;
    
    private Image[] points;

    void Awake()
    {
        points = GetComponentsInChildren<Image>();
    }

    public void UpdateLevel(int currentLevel)
    {
        for (int i = 0; i < points.Length; i++)
        {
            // Если индекс меньше уровня, красим в желтый, иначе в серый
            points[i].color = (i < currentLevel) ? activeColor : inactiveColor;
        }
    }
    public void SetMaxLevel(int maxLevel)
    {
    // Если нужно ограничить количество точек
    for (int i = 0; i < points.Length; i++)
    {
        points[i].gameObject.SetActive(i < maxLevel);
    }
}
}