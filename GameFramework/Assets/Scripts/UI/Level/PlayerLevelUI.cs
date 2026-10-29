using UnityEngine;
using TMPro;

public class PlayerLevelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLevel playerLevel;
    [SerializeField] private TextMeshProUGUI levelText;

    private int lastLevel = -1;

    private void Start()
    {
        if (playerLevel == null)
            playerLevel = FindAnyObjectByType<PlayerLevel>();

        if (levelText == null)
            levelText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (playerLevel == null || levelText == null) return;

        if (lastLevel != playerLevel.currentLevel)
        {
            lastLevel = playerLevel.currentLevel;
            levelText.text = $"Lv.{lastLevel}";
        }
    }
}