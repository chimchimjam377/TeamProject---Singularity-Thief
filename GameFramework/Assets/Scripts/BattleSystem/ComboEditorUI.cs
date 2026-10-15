using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboEditorUI : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private AttackLibrary attackLibrary;
    [SerializeField] private PlayerComboSequence comboSequence;

    [Header("왼쪽: 공격 선택")]
    [SerializeField] private AttackOptionCardUI optionCardPrefab;
    [SerializeField] private Transform optionContent;

    [Header("오른쪽: 콤보 편성")]
    [SerializeField] private ComboSlotUI comboSlotPrefab;
    [SerializeField] private Transform comboContent;

    [Header("버튼")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button deleteSelectedButton;
    [SerializeField] private Button upgradeCapacityButton;

    [Header("텍스트")]
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text hintText;

    [Header("설정")]
    [SerializeField, Min(1)] private int offerCount = 3;
    [SerializeField, Min(1)] private int capacityUpgradeAmount = 1;

    private readonly List<AttackDefinition> currentOffers = new();

    // -1: 선택한 슬롯 없음
    private int selectedSlotIndex = -1;

    private void Awake()
    {
        if (rerollButton != null)
            rerollButton.onClick.AddListener(RollOffers);

        if (deleteSelectedButton != null)
            deleteSelectedButton.onClick.AddListener(
                DeleteSelectedAttack
            );

        if (upgradeCapacityButton != null)
            upgradeCapacityButton.onClick.AddListener(
                UpgradeCapacity
            );
    }

    private void OnEnable()
    {
        if (comboSequence != null)
        {
            // Awake 호출 순서에 의존하지 않도록 초기화 보장
            comboSequence.Initialize();

            comboSequence.OnSequenceChanged +=
                HandleSequenceChanged;
        }

        RollOffers();
        RefreshAll();
    }

    private void OnDisable()
    {
        if (comboSequence != null)
        {
            comboSequence.OnSequenceChanged -=
                HandleSequenceChanged;
        }
    }

    // ========================================
    // 1. 공격 후보 생성
    // ========================================

    private void RollOffers()
    {
        if (attackLibrary == null)
        {
            SetHint("AttackLibrary가 연결되지 않았습니다.");
            return;
        }

        List<AttackDefinition> allAttacks = new();

        foreach (AttackDefinition attack in attackLibrary.Attacks)
        {
            if (attack != null && !allAttacks.Contains(attack))
                allAttacks.Add(attack);
        }

        if (allAttacks.Count == 0)
        {
            currentOffers.Clear();
            RefreshOffers();

            SetHint("선택 가능한 공격이 없습니다.");
            return;
        }

        // 기존 후보와 다른 공격을 우선 선택한다.
        List<AttackDefinition> candidates = new();

        foreach (AttackDefinition attack in allAttacks)
        {
            if (!currentOffers.Contains(attack))
                candidates.Add(attack);
        }

        // 다른 공격만으로 후보 수를 채우지 못하면
        // 전체 공격 목록을 다시 사용한다.
        if (candidates.Count < Mathf.Min(
                offerCount, allAttacks.Count))
        {
            candidates = new List<AttackDefinition>(allAttacks);
        }

        Shuffle(candidates);
        currentOffers.Clear();

        int count = Mathf.Min(offerCount, candidates.Count);

        for (int i = 0; i < count; i++)
            currentOffers.Add(candidates[i]);

        RefreshOffers();
        SetHint("공격 후보를 갱신했습니다.");
    }

    private void Shuffle(List<AttackDefinition> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            AttackDefinition temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void RefreshOffers()
    {
        ClearChildren(optionContent);

        if (optionCardPrefab == null || optionContent == null)
            return;

        foreach (AttackDefinition attack in currentOffers)
        {
            AttackOptionCardUI card = Instantiate(
                optionCardPrefab,
                optionContent
            );

            card.Bind(attack, HandleOfferSelected);
        }
    }

    // ========================================
    // 2. 공격 후보 선택
    // ========================================

    private void HandleOfferSelected(AttackDefinition attack)
    {
        if (comboSequence == null || attack == null)
            return;

        int targetIndex = selectedSlotIndex;
        bool success;

        // 데이터 변경 이벤트가 발생할 때 이미 선택이
        // 해제된 상태가 되도록 먼저 변경한다.
        selectedSlotIndex = -1;

        if (targetIndex < 0)
        {
            // 슬롯을 선택하지 않았다면 마지막에 추가
            success = comboSequence.TryAppendAttack(attack);
        }
        else if (targetIndex == comboSequence.Count)
        {
            // 첫 번째 빈 슬롯이라면 마지막에 추가
            success = comboSequence.TryAppendAttack(attack);
        }
        else if (targetIndex < comboSequence.Count)
        {
            // 이미 공격이 있는 슬롯이라면 교체
            success = comboSequence.ReplaceAttackAt(
                targetIndex,
                attack
            );
        }
        else
        {
            success = false;
        }

        if (!success)
        {
            selectedSlotIndex = targetIndex;

            RefreshAll();
            SetHint("편성에 실패했습니다. 빈 슬롯과 용량을 확인하세요.");
            return;
        }

        SetHint($"{attack.name} 편성 완료");

        // 성공하면 PlayerComboSequence의 이벤트가 UI를 갱신한다.
    }

    // ========================================
    // 3. 콤보 슬롯 선택
    // ========================================

    private void HandleSlotSelected(int index)
    {
        if (comboSequence == null)
            return;

        // 실제 공격 사이에 있는 첫 번째 빈 슬롯까지만 허용
        if (index > comboSequence.Count)
            return;

        // 빈 슬롯이 없는데 이전 선택이 남은 경우 방지
        if (index == comboSequence.Count &&
            comboSequence.Count >= comboSequence.SlotCapacity)
        {
            SetHint("콤보 슬롯이 가득 찼습니다.");
            return;
        }

        selectedSlotIndex = index;

        if (index == comboSequence.Count)
        {
            SetHint("공격 카드를 선택하면 이 슬롯에 추가됩니다.");
        }
        else
        {
            SetHint("공격 카드를 선택하면 기존 공격이 교체됩니다.");
        }

        RefreshAll();
    }

    // ========================================
    // 4. 공격 삭제
    // ========================================

    private void HandleRemoveAttack(int index)
    {
        if (comboSequence == null)
            return;

        selectedSlotIndex = -1;

        if (comboSequence.RemoveAttackAt(index))
        {
            SetHint("공격을 콤보에서 제거했습니다.");
            // OnSequenceChanged가 UI를 갱신한다.
        }
        else
        {
            RefreshAll();
            SetHint("공격 삭제에 실패했습니다.");
        }
    }

    private void DeleteSelectedAttack()
    {
        if (selectedSlotIndex < 0 ||
            selectedSlotIndex >= comboSequence.Count)
        {
            SetHint("먼저 삭제할 공격 슬롯을 선택하세요.");
            return;
        }

        HandleRemoveAttack(selectedSlotIndex);
    }

    // ========================================
    // 5. 공격 순서 변경
    // ========================================

    private void HandleMoveAttack(int fromIndex, int toIndex)
    {
        if (comboSequence == null)
            return;

        selectedSlotIndex = -1;

        if (comboSequence.MoveAttack(fromIndex, toIndex))
        {
            SetHint("공격 순서를 변경했습니다.");
            // OnSequenceChanged가 UI를 갱신한다.
        }
        else
        {
            RefreshAll();
            SetHint("공격 순서를 변경하지 못했습니다.");
        }
    }

    // ========================================
    // 6. 슬롯 용량 증가
    // ========================================

    private void UpgradeCapacity()
    {
        if (comboSequence == null)
            return;

        if (comboSequence.IncreaseSlotCapacity(
                capacityUpgradeAmount))
        {
            SetHint("콤보 슬롯 용량이 증가했습니다.");
            // OnSequenceChanged가 UI를 갱신한다.
        }
        else
        {
            SetHint("최대 슬롯 용량에 도달했습니다.");
        }
    }

    // ========================================
    // 7. UI 갱신
    // ========================================

    private void HandleSequenceChanged()
    {
        // 삭제 등으로 선택 인덱스가 유효하지 않게 된 경우
        if (comboSequence == null ||
            selectedSlotIndex > comboSequence.Count)
        {
            selectedSlotIndex = -1;
        }

        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshComboSlots();
        RefreshStatus();
    }

    private void RefreshComboSlots()
    {
        if (comboSequence == null ||
            comboSlotPrefab == null ||
            comboContent == null)
        {
            return;
        }

        ClearChildren(comboContent);

        IReadOnlyList<AttackDefinition> attacks =
            comboSequence.Sequence;

        for (int i = 0; i < comboSequence.SlotCapacity; i++)
        {
            if (i > 0)
                CreateArrow();

            AttackDefinition attack =
                i < comboSequence.Count ? attacks[i] : null;

            ComboSlotUI slot = Instantiate(
                comboSlotPrefab,
                comboContent
            );

            slot.Bind(
                i,
                attack,
                comboSequence.Count,
                i == selectedSlotIndex,
                HandleSlotSelected,
                HandleRemoveAttack,
                HandleMoveAttack
            );
        }
    }

    private void CreateArrow()
    {
        GameObject arrowObject = new GameObject(
            "ComboArrow",
            typeof(RectTransform),
            typeof(TextMeshProUGUI),
            typeof(LayoutElement)
        );

        arrowObject.transform.SetParent(comboContent, false);

        TextMeshProUGUI arrow =
            arrowObject.GetComponent<TextMeshProUGUI>();

        arrow.text = "→";
        arrow.fontSize = 32;
        arrow.alignment = TextAlignmentOptions.Center;
        arrow.raycastTarget = false;

        LayoutElement layout =
            arrowObject.GetComponent<LayoutElement>();

        layout.preferredWidth = 36;
        layout.preferredHeight = 100;
    }

    private void RefreshStatus()
    {
        if (comboSequence == null)
            return;

        if (capacityText != null)
        {
            capacityText.text =
                $"편성: {comboSequence.Count} / " +
                $"{comboSequence.SlotCapacity}";
        }

        if (deleteSelectedButton != null)
        {
            deleteSelectedButton.interactable =
                selectedSlotIndex >= 0 &&
                selectedSlotIndex < comboSequence.Count;
        }

        if (upgradeCapacityButton != null)
        {
            upgradeCapacityButton.interactable =
                comboSequence.SlotCapacity <
                comboSequence.MaximumSlotCapacity;
        }
    }

    private void SetHint(string message)
    {
        if (hintText != null)
            hintText.text = message;
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }
}