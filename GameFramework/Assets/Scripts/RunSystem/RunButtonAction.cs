using UnityEngine;
using UnityEngine.UI;

public enum RunButtonType
{
    StartRun,
    FinishShop
}

[RequireComponent(typeof(Button))]
public class RunButtonAction : MonoBehaviour
{
    [Header("버튼 기능")]
    [SerializeField] private RunButtonType buttonType;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(ExecuteAction);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(ExecuteAction);
        }
    }

    private void ExecuteAction()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager를 찾을 수 없습니다.");
            return;
        }

        switch (buttonType)
        {
            case RunButtonType.StartRun:

                RunManager.Instance.StartRun();
                break;

            case RunButtonType.FinishShop:

                RunManager.Instance.OnShopFinished();
                break;
        }
    }
}