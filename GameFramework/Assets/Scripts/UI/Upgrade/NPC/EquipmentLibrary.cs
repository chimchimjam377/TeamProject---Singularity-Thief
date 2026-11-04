using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopLibrary", menuName = "Shop/Equipment Library")]
public class EquipmentLibrary : ScriptableObject
{
    public List<EquipmentPartData> allEquipments = new List<EquipmentPartData>();

    // 💡 BodyPart를 기준으로 장비 데이터를 찾아주는 함수
    public EquipmentPartData GetPartData(BodyPart part)
    {
        foreach (var equip in allEquipments)
        {
            if (equip.bodyPart == part) return equip;
        }
        return null;
    }
}