using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCInteractionUI : MonoBehaviour
{
    public static NPCInteractionUI Instance { get; private set; }

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;

    [Header("월드 상호작용 UI")]
    [SerializeField] private RectTransform promptRoot;

    [SerializeField]
    private Vector3 promptOffset =
        new Vector3(0f, 1.2f, 0f);

    [Header("NPC 정보 UI")]
    [SerializeField] private GameObject infoPanelRoot;
    [SerializeField] private Image npcSpriteImage;
    [SerializeField] private TMP_Text factionText;
    [SerializeField] private TMP_Text npcNameText;

    private NPCInteractable currentTarget;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        Hide();
    }

    public void Show(NPCInteractable npc)
    {
        if (npc == null || npc.Profile == null)
            return;
            
        if (promptRoot == null || infoPanelRoot == null)
            return;



        currentTarget = npc;

        NPCProfile profile = npc.Profile;

        promptRoot.gameObject.SetActive(true);
        infoPanelRoot.SetActive(true);

        factionText.text = profile.FactionName;
        npcNameText.text = profile.NPCName;

        npcSpriteImage.sprite = profile.DisplaySprite;
        npcSpriteImage.enabled =
            profile.DisplaySprite != null;
    }

    public void Hide()
    {
        currentTarget = null;

        if (promptRoot != null)
            promptRoot.gameObject.SetActive(false);
        if (infoPanelRoot != null)
            infoPanelRoot.SetActive(false);
    }

    private void LateUpdate()
    {
        if (currentTarget == null || mainCamera == null || promptRoot == null || canvas == null)
            return;

        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(
                currentTarget.transform.position +
                promptOffset
            );

        if (screenPosition.z <= 0f)
        {
            promptRoot.gameObject.SetActive(false);
            return;
        }

        promptRoot.gameObject.SetActive(true);

        if (canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            promptRoot.position = screenPosition;
        }
        else
        {
            RectTransform canvasRect =
                canvas.transform as RectTransform;

            if (RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    canvas.worldCamera,
                    out Vector2 localPoint))
            {
                promptRoot.anchoredPosition =
                    localPoint;
            }
        }
    }
}