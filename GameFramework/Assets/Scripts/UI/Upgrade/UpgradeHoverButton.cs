using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events; // 💡 UnityEvent를 사용하기 위해 반드시 추가!

public class UpgradeHoverButton : MonoBehaviour, IPointerEnterHandler
{
    [Header("이 버튼에 해당하는 장비 데이터")]
    public EquipmentPartData partData;

    [Header("마우스를 올렸을 때 즉시 실행할 기능")]
    public UnityEvent onHoverEvent; // 💡 인스펙터에 On Click()과 똑같이 생긴 칸을 만들어줍니다.

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. 툴팁(설명창) 띄우기
        if (UpgradeTooltip.Instance != null && partData != null)
        {
            UpgradeTooltip.Instance.ShowTooltip(partData);
        }

        // 2. 💡 버튼 클릭을 흉내내는게 아니라, 연결된 기능을 다이렉트로 강제 실행합니다!
        if (onHoverEvent != null)
        {
            onHoverEvent.Invoke();
        }
    }
}