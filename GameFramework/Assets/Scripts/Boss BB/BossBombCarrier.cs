using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossBombCarrier : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private Behaviour[] disableWhileGrabbed;
    private BossBomb held;
    private Rigidbody2D rb;
    private Transform grabPoint;
    private bool grabbed;
    private RigidbodyType2D oldBodyType;
    private float oldGravity;
    private bool[] oldEnabled;
    private void Awake() { rb = GetComponent<Rigidbody2D>(); }
    private void Update()
    {
        if (grabbed)
        {
            if (!grabPoint) { EndGrab(); return; }
            rb.position = grabPoint.position;
            return;
        }
        if (Input.GetKeyDown(KeyCode.F) && !held && holdPoint)
        {
            BossBomb closest = null;
            float best = pickupRange;
            foreach (BossBomb bomb in FindObjectsByType<BossBomb>(FindObjectsSortMode.None))
            {
                if (!bomb.CanPickUp) continue;
                float distance = Vector2.Distance(transform.position, bomb.transform.position);
                if (distance <= best) { closest = bomb; best = distance; }
            }
            if (closest && closest.PickUp(holdPoint)) held = closest;
        }
        if (Input.GetKeyDown(KeyCode.G) && held)
        {
            Camera cam = Camera.main;
            if (!cam) return;
            Vector2 direction = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition) - (Vector2)holdPoint.position;
            held.Throw(direction);
            held = null;
        }
    }
    public bool BeginGrab(Transform point)
    {
        if (grabbed || !point) return false;
        PlayerDodge dodge = GetComponent<PlayerDodge>();
        if (dodge && dodge.IsInvincible) return false;
        PlayerState state = GetComponent<PlayerState>();
        if (state && (state.Is(PlayerStateType.Dodge) || state.Is(PlayerStateType.Attack) || state.Is(PlayerStateType.Dead))) return false;
        grabbed = true; grabPoint = point;
        oldBodyType = rb.bodyType; oldGravity = rb.gravityScale;
        oldEnabled = new bool[disableWhileGrabbed == null ? 0 : disableWhileGrabbed.Length];
        for (int i = 0; i < oldEnabled.Length; i++)
        {
            var b = disableWhileGrabbed[i];
            if (!b || b == this) continue;
            oldEnabled[i] = b.enabled; b.enabled = false;
        }
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.position = point.position;
        return true;
    }
    public void EndGrab()
    {
        if (!grabbed) return;
        grabbed = false; grabPoint = null;
        if (rb) { rb.bodyType = oldBodyType; rb.gravityScale = oldGravity; rb.linearVelocity = Vector2.zero; }
        for (int i = 0; i < oldEnabled.Length; i++) if (disableWhileGrabbed[i] && disableWhileGrabbed[i] != this) disableWhileGrabbed[i].enabled = oldEnabled[i];
    }
    private void OnDisable() { EndGrab(); }
}
