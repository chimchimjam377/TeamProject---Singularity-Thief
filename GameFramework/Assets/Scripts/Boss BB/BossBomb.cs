using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BossBomb : MonoBehaviour
{
    private enum BombState { Falling, Ready, Held, Thrown }
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color dudColor = new Color(0.95f, 0.75f, 0.30f);
    [SerializeField] private Color warningColor = new Color(1f, 0.45f, 0.35f);
    [SerializeField] private float fallSpeed = 4f, throwSpeed = 14f, blastRadius = 1.1f, maxLife = 25f;
    [SerializeField] private float fallAcceleration = 11f, maxFallSpeed = 12f;
    [SerializeField] private float spinSpeed = 95f, driftSpeed = 0.22f;
    [SerializeField] private int damage = 20;
    [SerializeField] private BossMineBurst burstPrefab;
    private Rigidbody2D rb;
    private Collider2D col;
    private BombState state;
    private bool dud, initialized, spent;
    private PlayerStats player;
    private Transform holdPoint;
    private float timer, spinSign, driftPhase;
    private bool launched;
    private float launchGravity, launchTargetX;
    public float IntendedLandingX => launched ? launchTargetX : transform.position.x;
    private Color baseColor;
    public bool CanPickUp => initialized && !spent && state == BombState.Ready;
    public bool IsAvailableDud => initialized && !spent && dud && (state == BombState.Falling || state == BombState.Ready);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); col = GetComponent<Collider2D>(); col.isTrigger = true;
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
    }
    public void Initialize(bool isDud, PlayerStats target)
    {
        dud = isDud; player = target; initialized = true;
        state = BombState.Falling; timer = 0f; launched = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.down * Mathf.Max(0.1f, fallSpeed);
        spinSign = Random.value < 0.5f ? -1f : 1f;
        driftPhase = Random.Range(0f, Mathf.PI * 2f);
        rb.angularVelocity = spinSign * spinSpeed;
        if (spriteRenderer && dud) spriteRenderer.color = dudColor;
    }
    // Launch from the robot, arc above the arena, and descend onto the authored lane.
    public void InitializeLaunch(bool isDud, PlayerStats target, Vector3 guide, float minRise, float gravity)
    {
        Initialize(isDud, target);
        launched = true;
        launchGravity = Mathf.Max(1f, gravity);
        launchTargetX = guide.x;
        Vector2 origin = rb.position;
        float landingY = guide.y - 8f;
        foreach (var hit in Physics2D.RaycastAll(guide, Vector2.down, 100f, groundLayer))
        {
            if (!hit.collider || hit.collider.isTrigger) continue;
            landingY = hit.point.y;
            break;
        }
        float apex = Mathf.Max(origin.y + Mathf.Max(0.5f, minRise), guide.y, landingY + 1f);
        float vy = Mathf.Sqrt(2f * launchGravity * (apex - origin.y));
        float totalTime = vy / launchGravity + Mathf.Sqrt(2f * launchGravity * (apex - landingY)) / launchGravity;
        rb.linearVelocity = new Vector2((guide.x - origin.x) / Mathf.Max(0.1f, totalTime), vy);
    }

    private void FixedUpdate()
    {
        if (!initialized || spent) return;
        if (state == BombState.Falling)
        {
            if (launched)
            {
                rb.linearVelocity += Vector2.down * launchGravity * Time.fixedDeltaTime;
                return;
            }
            float speed = Mathf.Min(Mathf.Max(0.1f, maxFallSpeed), Mathf.Max(0f, -rb.linearVelocity.y) + fallAcceleration * Time.fixedDeltaTime);
            float drift = dud ? 0f : Mathf.Sin(timer * 5f + driftPhase) * driftSpeed;
            rb.linearVelocity = new Vector2(drift, -speed);
        }
        if (state == BombState.Held && holdPoint) rb.position = holdPoint.position;
    }
    private void Update()
    {
        if (!initialized || spent) return;
        if (state == BombState.Held)
        {
            if (!holdPoint) { Destroy(gameObject); return; }
            transform.position = holdPoint.position;
            return;
        }
        if (state == BombState.Ready) return; // No time limit for required mechanic pickups.
        timer += Time.deltaTime;
        if (timer >= maxLife) { Destroy(gameObject); return; }
        if (state == BombState.Falling && !dud && spriteRenderer)
            spriteRenderer.color = Color.Lerp(baseColor, warningColor, Mathf.PingPong(timer * 4f, 1f) * 0.6f);
    }
    public bool PickUp(Transform point)
    {
        if (!CanPickUp || !point) return false;
        state = BombState.Held; holdPoint = point;
        rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;
        transform.position = point.position;
        return true;
    }
    public void Throw(Vector2 direction)
    {
        if (state != BombState.Held) return;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        state = BombState.Thrown; timer = 0f; holdPoint = null;
        rb.bodyType = RigidbodyType2D.Dynamic; rb.gravityScale = 0f;
        col.enabled = true;
        rb.linearVelocity = direction.normalized * throwSpeed;
        rb.angularVelocity = spinSign * spinSpeed;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || spent || state == BombState.Held || state == BombState.Ready) return;
        if (other.GetComponentInParent<BossBomb>() || other.GetComponentInParent<PlayerStats>()) return;
        bool ground = (groundLayer.value & (1 << other.gameObject.layer)) != 0;
        if (state == BombState.Falling)
        {
            if (!ground || other.isTrigger || (launched && rb.linearVelocity.y > 0f)) return;
            if (!dud) { Explode(true); return; }
            state = BombState.Ready; timer = 0f;
            rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            float shift = other.bounds.max.y - col.bounds.min.y;
            rb.position += Vector2.up * (Mathf.Max(0f, shift) + 0.02f);
            if (spriteRenderer) spriteRenderer.color = dudColor;
            return;
        }
        if (state == BombState.Thrown)
        {
            var wall = other.GetComponentInParent<BossBreakableWall>();
            var part = other.GetComponentInParent<BossDamageReceiver>();
            if (wall) { wall.Break(); Explode(false); }
            else if (part) { if (part.HitByBomb()) Explode(false); }
            else if (ground && !other.isTrigger) Explode(false);
        }
    }
    private void Explode(bool hurtsPlayer)
    {
        if (spent) return;
        spent = true;
        if (hurtsPlayer && player && !player.IsDead && Vector2.Distance(transform.position, player.transform.position) <= blastRadius)
            player.TakeDamage(damage, gameObject);
        // Use an optional custom effect, or a small procedural pixel blast.
        BossMineBurst.Play(transform.position, blastRadius, spriteRenderer, burstPrefab);
        Destroy(gameObject);
    }
}
