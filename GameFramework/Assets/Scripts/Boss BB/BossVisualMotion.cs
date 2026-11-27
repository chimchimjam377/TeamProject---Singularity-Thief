using UnityEngine;

[RequireComponent(typeof(BossController))]
public class BossVisualMotion : MonoBehaviour
{
    [Header("Visual children only (not attack or collider roots)")]
    [SerializeField] private Transform headVisual;
    [SerializeField] private Transform leftArmVisual;
    [SerializeField] private Transform rightArmVisual;
    [Header("Idle motion")]
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 0.8f;
    [SerializeField] private float tiltAngle = 3f;
    [Header("Pixel animation: 0 = smooth")]
    [Min(0f)] [SerializeField] private float visualFPS = 12f;
    [Header("Overload")]
    [SerializeField] private float shakeAmount = 0.06f;
    private BossController boss;
    private Vector3 headHome, leftHome, rightHome;
    private Quaternion headRotation, leftRotation, rightRotation;
    private float elapsed;
    private Vector3 previousRoot;
    private float lean, leanVelocity;
    [SerializeField] private float movementLean = 0.5f;
    [SerializeField] private float leanSmoothTime = 0.16f;

    private void Awake()
    {
        boss = GetComponent<BossController>();
        previousRoot = transform.position;
        SavePose(headVisual, out headHome, out headRotation);
        SavePose(leftArmVisual, out leftHome, out leftRotation);
        SavePose(rightArmVisual, out rightHome, out rightRotation);
    }

    private void LateUpdate()
    {
        elapsed += Time.deltaTime;
        float t = visualFPS > 0f ? Mathf.Floor(elapsed * visualFPS) / visualFPS : elapsed;
        float wave = t * bobSpeed * Mathf.PI * 2f;
        float vx = (transform.position.x - previousRoot.x) / Mathf.Max(0.001f, Time.deltaTime);
        previousRoot = transform.position;
        float targetLean = Mathf.Clamp(-vx * movementLean, -5f, 5f);
        lean = Mathf.SmoothDamp(lean, targetLean, ref leanVelocity, Mathf.Max(0.02f, leanSmoothTime));
        bool charging = boss.State == BossController.BossState.ChargingLaser;
        bool stunned = boss.State == BossController.BossState.Stunned;
        bool phase2 = boss.State == BossController.BossState.Phase2;
        float shake = charging || stunned ? Mathf.Sin(t * 37f) * shakeAmount : 0f;
        float bob = Mathf.Sin(wave) * bobHeight * (stunned || boss.BodyAttackActive ? 0.15f : 0.35f);
        float tilt = stunned ? -6f : Mathf.Sin(wave * 0.7f) * tiltAngle + lean;
        if (phase2) tilt += Mathf.Sin(t * 13f) * 1.5f;
        ApplyPose(headVisual, headHome, headRotation, new Vector3(shake, bob, 0f), tilt);
        ApplyPose(leftArmVisual, leftHome, leftRotation,
            Vector3.zero, boss.BodyAttackActive ? 0f : Mathf.Sin(wave) * tiltAngle * 0.4f);
        ApplyPose(rightArmVisual, rightHome, rightRotation,
            Vector3.zero, boss.BodyAttackActive ? 0f : -Mathf.Sin(wave) * tiltAngle * 0.4f);
    }

    private static void SavePose(Transform part, out Vector3 position, out Quaternion rotation)
    {
        position = part ? part.localPosition : Vector3.zero;
        rotation = part ? part.localRotation : Quaternion.identity;
    }

    private static void ApplyPose(Transform part, Vector3 position, Quaternion rotation, Vector3 offset, float angle)
    {
        if (!part) return;
        part.localPosition = position + offset;
        part.localRotation = rotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDisable()
    {
        ApplyPose(headVisual, headHome, headRotation, Vector3.zero, 0f);
        ApplyPose(leftArmVisual, leftHome, leftRotation, Vector3.zero, 0f);
        ApplyPose(rightArmVisual, rightHome, rightRotation, Vector3.zero, 0f);
    }
}
