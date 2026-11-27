using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackOptionCardUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text titleText;

    private void Awake()
    {
        if (selectButton == null)
            selectButton = GetComponent<Button>();

        if (titleText == null)
            titleText = GetComponentInChildren<TMP_Text>();
    }

    public void Bind(
        AttackDefinition definition,
        Action<AttackDefinition> onSelected)
    {
        if (selectButton == null || titleText == null)
        {
            Debug.LogError(
                "AttackOptionCardUI의 Button 또는 TMP_Text가 없습니다.",
                this
            );
            return;
        }

        selectButton.onClick.RemoveAllListeners();

        if (definition == null)
        {
            titleText.text = "공격 없음";
            selectButton.interactable = false;
            return;
        }

        titleText.text = definition.name;
        selectButton.interactable = true;

        selectButton.onClick.AddListener(
            () => onSelected?.Invoke(definition)
        );
    }
}