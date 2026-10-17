using UnityEngine;
using TMPro; // TextMeshPro 사용

public class PlayerLevelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLevel playerLevel;
    [SerializeField] private TextMeshProUGUI levelText;

    // 이전 레벨을 기억해두기 위한 변수 (초기값은 -1)
    private int lastLevel = -1;

    private void Start()
    {
        if (playerLevel == null)
        {
            // 직접 연결하지 않았다면 씬에서 PlayerLevel을 가진 플레이어를 자동으로 찾음
            playerLevel = FindFirstObjectByType<PlayerLevel>();
        }

        if (levelText == null)
        {
            levelText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (playerLevel == null || levelText == null) return;

        // 현재 레벨이 기억하고 있던 레벨과 다를 때만(즉, 레벨업 했을 때만) 텍스트 변경
        if (lastLevel != playerLevel.currentLevel)
        {
            lastLevel = playerLevel.currentLevel;

            // 화면에 표시될 텍스트 형식 (원하는 대로 수정 가능)
            levelText.text = $"Lv.{lastLevel}";
        }
    }
}