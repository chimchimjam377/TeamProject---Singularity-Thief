using UnityEngine;
using GameFramework.Core;
using GameFramework.Services;
using GameFramework.Gameplay;
using UnityEngine.InputSystem;

namespace GameFramework.EXample
{
    public class Inventory_Demo : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return;

            // [1] 아이템 획득 (Ba)
            if (kb.digit1Key.wasPressedThisFrame)
                InventoryManager.Instance.AddItem("Ba", 10);

            // [2] 아이템 획득 (Ka) - 새로 추가된 부분
            if (kb.digit2Key.wasPressedThisFrame)
                InventoryManager.Instance.AddItem("Ka", 10);

            // [V] 인벤토리 팝업창 열기
            if (kb.vKey.wasPressedThisFrame)
                UIManager.Instance.Toggle<InventoryPopup>("InventoryPopup");

        }
    }
}