using UnityEngine;

public class BossChainDevice : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private Transform player, chainTarget;
    [SerializeField] private LineRenderer chainLine;
    [SerializeField] private Transform phase2ChainTarget;
    public Vector3 GroundAnchorPosition => transform.position;
    [SerializeField] private float interactRange = 2f;
    private bool unlocked, used;
    private void Start()
    {
        if (chainLine) { chainLine.useWorldSpace = true; chainLine.positionCount = 2; chainLine.enabled = false; }
    }
    public void Unlock() { unlocked = true; Debug.Log("Chain unlocked: press E nearby"); }
    private void Update()
    {
        if (!unlocked || used || !player || !boss) return;
        if (Vector2.Distance(player.position, transform.position) > interactRange) return;
        if (Input.GetKeyDown(KeyCode.E) && boss.AddChain()) { used = true; if (chainLine) chainLine.enabled = true; }
    }
    private void LateUpdate()
    {
        if (!used || !chainLine) return;
        if (boss && boss.State == BossController.BossState.Dead) { chainLine.enabled = false; return; }
        bool phase2 = boss && (boss.State == BossController.BossState.Transition || boss.State == BossController.BossState.Phase2);
        Transform target = phase2 ? phase2ChainTarget : chainTarget;
        chainLine.enabled = target != null;
        if (target)
        {
            chainLine.SetPosition(0, GroundAnchorPosition);
            chainLine.SetPosition(1, target.position);
        }
    }
}
