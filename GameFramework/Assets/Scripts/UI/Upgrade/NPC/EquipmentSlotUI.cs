using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    private EquipmentPartData partData;
    private ShopManager shopManager;

    [Header("슬롯 자체 UI")]
    public TextMeshProUGUI nameText;

    public void InitSlot(EquipmentPartData data, ShopManager manager)
    {
        partData = data;
        shopManager = manager;

        // 아이콘과 비용은 삭제되었으므로 이름만 슬롯에 띄워줍니다.
        if (nameText != null)
        {
            nameText.text = data.partName;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (partData != null && shopManager != null)
        {
            shopManager.ShowDetailInfo(partData);
        }
    }
}