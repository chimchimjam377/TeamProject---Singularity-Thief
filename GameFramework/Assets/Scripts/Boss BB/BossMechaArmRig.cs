using UnityEngine;

// Fixed-length visual arm: calibrated once from the scene's initial pose.
// Root movement never changes segment scale. Keep the rig under Boss (scale 1,1,1).
public class BossMechaArmRig : MonoBehaviour
{
    [SerializeField] private Transform shoulderPivot;
    [SerializeField] private Transform handTarget;
    [SerializeField] private SpriteRenderer upperArm;
    [SerializeField] private SpriteRenderer forearm;
    [SerializeField] private Transform elbowVisual;
    [SerializeField] private float elbowBend = 0.8f;
    [SerializeField] private float segmentOverlap = 0.12f;
    public Transform ShoulderPivot => shoulderPivot;
    public Transform HandTarget => handTarget;
    private SpriteRenderer[] shoulderVisuals;
    private bool calibrated;

    private void Awake()
    {
        if (shoulderPivot) shoulderVisuals = shoulderPivot.GetComponentsInChildren<SpriteRenderer>(true);
    }
    private float upperLength, foreLength;

    private void LateUpdate()
    {
        if (!shoulderPivot || !handTarget || !upperArm || !forearm || !upperArm.sprite || !forearm.sprite) return;
        bool visible = handTarget.gameObject.activeInHierarchy;
        if (!visible && shoulderVisuals != null)
            foreach (var sr in shoulderVisuals) if (sr) sr.enabled = false;
        upperArm.enabled = visible;
        forearm.enabled = visible;
        if (elbowVisual && elbowVisual.gameObject.activeSelf != visible) elbowVisual.gameObject.SetActive(visible);
        if (!visible) return;
        if (!TryGetPose(handTarget.position, out Vector2 a, out Vector2 elbow, out Vector2 wrist)) return;
        SetPose(upperArm, a, elbow);
        SetPose(forearm, elbow, wrist);
        Vector2 direction = wrist - elbow;
        handTarget.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f);
        if (elbowVisual) elbowVisual.position = new Vector3(elbow.x, elbow.y, elbowVisual.position.z);
    }

    // The controller and visual rig use the same fixed-length geometry.
    public bool TryGetPose(Vector2 requestedHand, out Vector2 a, out Vector2 elbow, out Vector2 wrist)
    {
        a = elbow = wrist = Vector2.zero;
        if (!shoulderPivot || !handTarget || !upperArm || !forearm || !upperArm.sprite || !forearm.sprite) return false;
        a = shoulderPivot.position;
        Vector2 d = requestedHand - a;
        if (d.sqrMagnitude < 0.00001f) return false;
        Vector2 dir = d.normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x);
        if (!calibrated)
        {
            Vector2 initialElbow = (a + requestedHand) * 0.5f + normal * elbowBend;
            upperLength = Vector2.Distance(a, initialElbow);
            foreLength = Vector2.Distance(initialElbow, requestedHand);
            CalibrateScale(upperArm, upperLength);
            CalibrateScale(forearm, foreLength);
            calibrated = true;
        }
        float distance = Mathf.Clamp(d.magnitude, Mathf.Abs(upperLength - foreLength) + 0.0001f, upperLength + foreLength);
        wrist = a + dir * distance;
        float along = (upperLength * upperLength - foreLength * foreLength + distance * distance) / (2f * distance);
        float height = Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - along * along));
        elbow = a + dir * along + normal * height * (elbowBend < 0f ? -1f : 1f);
        return true;
    }
    private void CalibrateScale(SpriteRenderer part, float length)
    {
        Vector3 scale = part.transform.localScale;
        float parentScale = part.transform.parent ? Mathf.Abs(part.transform.parent.lossyScale.y) : 1f;
        scale.y = (length + segmentOverlap) / Mathf.Max(0.001f, part.sprite.bounds.size.y * parentScale);
        part.transform.localScale = scale;
    }
    private static void SetPose(SpriteRenderer part, Vector2 a, Vector2 b)
    {
        Vector2 mid = (a + b) * 0.5f;
        Vector2 d = b - a;
        part.transform.position = new Vector3(mid.x, mid.y, part.transform.position.z);
        part.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + 90f);
    }
}
