using System.Collections;
using UnityEngine;

// Add to each detached-arm platform. Put its Transform at the FINAL platform position in the editor.
// Keep the Phase2Platforms parent inactive at scene start.
public class BossDetachedPlatformMotion : MonoBehaviour
{
    [SerializeField] private Transform sourcePart;
    [SerializeField] private BossChainDevice groundChain;
    [SerializeField] private float tensionRecoil = 0.15f;
    [SerializeField] private float travelTime = 1.2f;
    [SerializeField] private float arcHeight = 0.6f;
    [SerializeField] private float spinAngle = 25f;
    private Vector3 destination;
    private Quaternion endRotation;
    private Collider2D[] colliders;
    private bool[] colliderEnabled;
    private void Awake()
    {
        destination = transform.position;
        endRotation = transform.rotation;
        colliders = GetComponentsInChildren<Collider2D>(true);
        colliderEnabled = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++) colliderEnabled[i] = colliders[i].enabled;
    }
    private void OnEnable()
    {
        if (sourcePart) StartCoroutine(Detach());
    }
    private IEnumerator Detach()
    {
        Vector3 start = sourcePart.position;
        for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
        Vector3 anchor = groundChain ? groundChain.GroundAnchorPosition : destination;
        float finalLength = Vector3.Distance(anchor, destination);
        float startLength = Vector3.Distance(anchor, start);
        if (groundChain && (destination.y <= anchor.y || finalLength < startLength))
            Debug.LogWarning("Place the tethered platform above the ground anchor, farther away than its release point.", this);
        float duration = Mathf.Max(0.01f, travelTime);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            // Rapid upward pull, then a short recoil as the ground tether becomes taut.
            float rise = Mathf.Clamp01(p / 0.72f);
            float f = 1f - Mathf.Pow(1f - rise, 2f);
            Vector3 pos = Vector3.Lerp(start, destination, f) + Vector3.up * (Mathf.Sin(rise * Mathf.PI) * arcHeight);
            if (groundChain && finalLength > 0.001f)
            {
                float limit = Mathf.Lerp(Mathf.Max(startLength, finalLength), finalLength, f);
                Vector3 offset = pos - anchor;
                pos = anchor + Vector3.ClampMagnitude(offset, limit);
                if (p > 0.72f)
                {
                    float q = (p - 0.72f) / 0.28f;
                    pos = destination - (destination - anchor).normalized * Mathf.Min(finalLength, tensionRecoil) * Mathf.Sin(q * Mathf.PI);
                }
            }
            transform.position = pos;
            transform.rotation = endRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(p * Mathf.PI) * spinAngle);
            yield return null;
        }
        Restore();
    }
    private void Restore()
    {
        transform.position = destination;
        transform.rotation = endRotation;
        if (colliders == null) return;
        for (int i = 0; i < colliders.Length; i++) if (colliders[i]) colliders[i].enabled = colliderEnabled[i];
    }
    private void OnDisable() { StopAllCoroutines(); Restore(); }
}
