using UnityEngine;

public class BossDamageReceiver : MonoBehaviour, IDamageable
{
    [SerializeField] private BossController boss;
    [SerializeField] private bool isCore;
    private void Awake() { if (!boss) boss = GetComponentInParent<BossController>(); }
    public void TakeDamage(int damage, GameObject attacker) { if (boss) boss.TakePartDamage(damage, isCore); }
    public bool HitByBomb()
    {
        if (!boss || !isCore || boss.State != BossController.BossState.Phase2) return false;
        boss.HitCoreWithBomb();
        return true;
    }
}
