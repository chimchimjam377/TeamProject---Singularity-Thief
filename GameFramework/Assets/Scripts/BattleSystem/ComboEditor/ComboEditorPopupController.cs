
using UnityEngine;
using UnityEngine.UI;

public class ComboEditorPopupController : MonoBehaviour
{
    [Header("팝업")]
    [SerializeField] private GameObject comboEditorPanel;

    [Header("버튼")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("키보드")]
    [SerializeField] private bool allowKeyboardToggle = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // 게임 시작 시 팝업을 숨긴다.
        if (comboEditorPanel != null)
            comboEditorPanel.SetActive(false);
    }

    private void Update()
    {
        if (!allowKeyboardToggle)
            return;

        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Open()
    {
        if (comboEditorPanel == null)
            return;

        comboEditorPanel.SetActive(true);
    }

    public void Close()
    {
        if (comboEditorPanel == null)
            return;

        comboEditorPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (comboEditorPanel == null)
            return;

        if (comboEditorPanel.activeSelf)
            Close();
        else
            Open();
    }
}
