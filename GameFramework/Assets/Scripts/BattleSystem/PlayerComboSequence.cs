using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboSequence : MonoBehaviour
{
    [Header("기본 콤보")]
    [SerializeField]
    private ComboSequenceData defaultSequence;

    [Header("슬롯 용량")]
    [SerializeField,Min(1)]
    private int initialSlotCapacity = 3;

    [Min(1)]
    [SerializeField] private int maximumSlotCapacity = 12;

    private readonly List<AttackDefinition> sequence = new();

    private bool initialized;
    private int slotCapacity;

    public IReadOnlyList<AttackDefinition> Sequence => sequence;

    public int Count => sequence.Count;

    public int SlotCapacity => slotCapacity;

    public int EmptySlotCount =>
        Mathf.Max(0, slotCapacity - sequence.Count);

    // UI에서 최대 슬롯 수용량을 확인하기 위한 프로퍼티
    public int MaximumSlotCapacity =>
        Mathf.Max(1, maximumSlotCapacity);

    public event Action OnSequenceChanged;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        int maxCapacity = Mathf.Max(1, maximumSlotCapacity);

        slotCapacity = Mathf.Clamp(
            initialSlotCapacity,
            1,
            maxCapacity
        );

        sequence.Clear();

        if (defaultSequence != null)
        {
            foreach (AttackDefinition attack
                     in defaultSequence.DefaultAttacks)
            {
                if (attack != null &&
                    sequence.Count < maxCapacity)
                {
                    sequence.Add(attack);
                }
            }
        }

        // 기본 콤보가 슬롯 용량보다 길면 용량도 늘린다.
        slotCapacity = Mathf.Max(
            slotCapacity,
            sequence.Count
        );
    }

    public AttackDefinition GetAttackAt(int index)
    {
        if (index < 0 || index >= sequence.Count)
            return null;

        return sequence[index];
    }

    // 새로운 공격을 콤보 끝에 추가한다.
    public bool TryAppendAttack(AttackDefinition attack)
    {
        if (attack == null)
            return false;

        if (sequence.Count >= slotCapacity)
            return false;

        sequence.Add(attack);

        NotifyChanged();
        return true;
    }

    // 특정 슬롯의 공격을 다른 공격으로 교체한다.
    public bool ReplaceAttackAt(
        int index,
        AttackDefinition newAttack)
    {
        if (newAttack == null)
            return false;

        if (index < 0 || index >= sequence.Count)
            return false;

        sequence[index] = newAttack;

        NotifyChanged();
        return true;
    }

    // 특정 콤보 슬롯의 공격을 삭제한다.
    public bool RemoveAttackAt(int index)
    {
        if (index < 0 || index >= sequence.Count)
            return false;

        sequence.RemoveAt(index);

        NotifyChanged();
        return true;
    }

    // 두 슬롯의 위치를 바꾼다.
    public bool MoveAttack(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= sequence.Count)
            return false;

        if (toIndex < 0 || toIndex >= sequence.Count)
            return false;

        if (fromIndex == toIndex)
            return true;

        AttackDefinition attack = sequence[fromIndex];

        sequence.RemoveAt(fromIndex);
        sequence.Insert(toIndex, attack);

        NotifyChanged();
        return true;
    }

    // 공격 슬롯 수용량을 늘린다.
    public bool IncreaseSlotCapacity(int amount = 1)
    {
        if (amount <= 0)
            return false;

        int maxCapacity = Mathf.Max(1, maximumSlotCapacity);

        if (slotCapacity >= maxCapacity)
            return false;

        slotCapacity = Mathf.Min(
            slotCapacity + amount,
            maxCapacity
        );

        NotifyChanged();
        return true;
    }

    // 기본 콤보로 복구한다.
    public void ResetToDefault()
    {
        int maxCapacity = Mathf.Max(1, maximumSlotCapacity);

        sequence.Clear();

        if (defaultSequence != null)
        {
            foreach (AttackDefinition attack
                     in defaultSequence.DefaultAttacks)
            {
                if (attack == null)
                    continue;

                if (sequence.Count >= maxCapacity)
                    break;

                sequence.Add(attack);
            }
        }

        slotCapacity = Mathf.Clamp(
            Mathf.Max(initialSlotCapacity, sequence.Count),
            1,
            maxCapacity
        );

        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnSequenceChanged?.Invoke();
    }
}