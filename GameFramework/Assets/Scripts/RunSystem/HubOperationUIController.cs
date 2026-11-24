using UnityEngine;
using UnityEngine.UI;

public class HubOperationUIController : MonoBehaviour
{
    [Header("Operation Popup")]
    [SerializeField] private GameObject confirmPanel;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private bool isPopupOpen;

    private void Awake()
    {
        confirmPanel.SetActive(false);
    }

    private void OnEnable()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(OnConfirmClicked);
        cancelButton.onClick.RemoveListener(OnCancelClicked);
    }

    // 작전 시작 팝업 표시
    public void ShowConfirmPopup()
    {
        if (isPopupOpen)
            return;

        if (RunManager.Instance == null)
            return;

        if (RunManager.Instance.IsRunActive)
            return;

        isPopupOpen = true;
        confirmPanel.SetActive(true);
    }

    // 작전 시작
    private void OnConfirmClicked()
    {
        ClosePopup();

        // 추후 플레이어 사인 연출을 이 위치에서 실행할 예정
        // 현재는 바로 런을 시작
        StartOperation();
    }

    // 취소
    private void OnCancelClicked()
    {
        ClosePopup();
    }

    private void ClosePopup()
    {
        isPopupOpen = false;
        confirmPanel.SetActive(false);
    }

    // 작전 시작 실행
    public void StartOperation()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager를 찾을 수 없습니다.");
            return;
        }

        RunManager.Instance.StartRun();
    }
}