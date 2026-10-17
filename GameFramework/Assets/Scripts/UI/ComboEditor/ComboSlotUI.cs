using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboSlotUI : MonoBehaviour
{
    [Header("표시")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image backgroundImage;

    [Header("버튼")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;

    [Header("선택 색상")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    public void Bind(
        int index,
        AttackDefinition definition,
        int occupiedCount,
        bool isSelected,
        Action<int> onSelected,
        Action<int> onRemoved,
        Action<int, int> onMoved)
    {
        // 이전에 연결된 리스너 제거
        selectButton.onClick.RemoveAllListeners();
        removeButton.onClick.RemoveAllListeners();
        moveLeftButton.onClick.RemoveAllListeners();
        moveRightButton.onClick.RemoveAllListeners();

        bool hasAttack = definition != null;

        // 공격 이름 표시
        if (hasAttack)
        {
            titleText.text = $"슬롯 {index + 1}\n{definition.name}";
        }
        else if (index == occupiedCount)
        {
            titleText.text = $"슬롯 {index + 1}\n+ 공격 추가";
        }
        else
        {
            titleText.text = $"슬롯 {index + 1}\n빈 슬롯";
        }

        // 이미 공격이 있거나, 바로 다음 빈 슬롯이면 선택 가능
        selectButton.interactable =
            hasAttack || index == occupiedCount;

        int slotIndex = index;

        selectButton.onClick.AddListener(
            () => onSelected?.Invoke(slotIndex)
        );

        // 선택한 공격 삭제
        removeButton.gameObject.SetActive(hasAttack);

        if (hasAttack)
        {
            removeButton.onClick.AddListener(
                () => onRemoved?.Invoke(slotIndex)
            );
        }

        // 앞뒤 순서 변경
        moveLeftButton.gameObject.SetActive(hasAttack);
        moveRightButton.gameObject.SetActive(hasAttack);

        moveLeftButton.interactable =
            hasAttack && index > 0;

        moveRightButton.interactable =
            hasAttack && index < occupiedCount - 1;

        if (hasAttack)
        {
            moveLeftButton.onClick.AddListener(
                () => onMoved?.Invoke(slotIndex, slotIndex - 1)
            );

            moveRightButton.onClick.AddListener(
                () => onMoved?.Invoke(slotIndex, slotIndex + 1)
            );
        }

        // 선택된 슬롯 강조
        if (backgroundImage != null)
        {
            backgroundImage.color =
                isSelected ? selectedColor : normalColor;
        }
    }
}