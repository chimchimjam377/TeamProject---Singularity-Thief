using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    private BoxCollider2D hitboxCollider;

    private int damage;
    private GameObject attacker;

    // 공격 대상(예: Player 레이어)만 감지하기 위한 레이어 마스크
    [SerializeField] private LayerMask targetLayer;

    // 단일 공격 시 중복 피격 방지용
    private readonly HashSet<IDamageable> hitTargets = new();

    private void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();
        hitboxCollider.isTrigger = true; // Trigger 설정 보장
        hitboxCollider.enabled = false;
    }

    
    /// 적 공격 시작 시 히트박스 활성화
    
    public void Activate(int damage, GameObject attacker)
    {
        this.damage = damage;
        this.attacker = attacker;

        hitTargets.Clear();
        hitboxCollider.enabled = true;
    }

    
    /// 공격 종료 시 히트박스 비활성화
    
    public void Deactivate()
    {
        hitboxCollider.enabled = false;
        hitTargets.Clear();
    }

    
    /// 적의 바라보는 방향 및 크기에 따른 히트박스 설정
    
    /// <param name="width">너비</param>
    /// <param name="height">높이</param>
    /// <param name="offsetX">X 오프셋</param>
    /// <param name="offsetY">Y 오프셋</param>
    /// <param name="direction">바라보는 방향 (오른쪽: 1, 왼쪽: -1)</param>
    public void SetShape(float width, float height, float offsetX, float offsetY, int direction)
    {
        hitboxCollider.size = new Vector2(width, height);

        // 바라보는 방향에 따른 X축 오프셋 반전
        offsetX *= direction;

        hitboxCollider.offset = new Vector2(offsetX, offsetY);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitboxCollider.enabled)
            return;

        // 공격 주체 자신 제외
        if (other.gameObject == attacker)
            return;

        // 타겟 레이어 검사 (Player 레이어만 통과하도록 Inspector에서 설정)
        if (targetLayer != 0 && ((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        // 피격 대상 컴포넌트 탐색
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        // 동일 공격 애니메이션 내 중복 타격 방지
        if (hitTargets.Contains(damageable))
            return;

        hitTargets.Add(damageable);

        // 플레이어에게 대미지 전달
        damageable.TakeDamage(damage, attacker);
    }
}