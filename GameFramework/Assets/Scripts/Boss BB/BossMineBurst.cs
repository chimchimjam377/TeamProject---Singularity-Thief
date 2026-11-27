using UnityEngine;

// Cosmetic-only expanding pixel blast, no collision or damage.
public class BossMineBurst : MonoBehaviour
{
    private static Sprite pixelDisk;
    private SpriteRenderer outer, inner;
    private float radius, elapsed;
    private const float Duration = 0.32f;
    public static void Play(Vector3 at, float radius, SpriteRenderer source, BossMineBurst prefab)
    {
        BossMineBurst fx;
        if (prefab) fx = Instantiate(prefab, at, Quaternion.identity);
        else fx = new GameObject("MineBurst").AddComponent<BossMineBurst>();
        fx.transform.position = at;
        fx.radius = Mathf.Max(0.1f, radius);
        if (!pixelDisk)
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                tex.SetPixel(x, y, d < 15f ? Color.white : Color.clear);
            }
            tex.Apply(); pixelDisk = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
        fx.outer = fx.MakeLayer("Outer", source, 2);
        fx.inner = fx.MakeLayer("Inner", source, 3);
        fx.Apply(0f);
    }
    private SpriteRenderer MakeLayer(string label, SpriteRenderer source, int order)
    {
        var layer = new GameObject(label); layer.transform.SetParent(transform, false);
        var sr = layer.AddComponent<SpriteRenderer>(); sr.sprite = pixelDisk;
        if (source) { sr.sortingLayerID = source.sortingLayerID; sr.sortingOrder = source.sortingOrder + order; }
        return sr;
    }
    private void Update()
    {
        if (!outer || !inner) return;
        elapsed += Time.deltaTime;
        Apply(Mathf.Clamp01(elapsed / Duration));
        if (elapsed >= Duration) Destroy(gameObject);
    }
    private void Apply(float p)
    {
        float expand = 1f - (1f - p) * (1f - p);
        float size = radius * 2f * Mathf.Lerp(0.25f, 1.3f, expand);
        outer.transform.localScale = Vector3.one * size;
        inner.transform.localScale = Vector3.one * size * Mathf.Lerp(0.65f, 0.22f, p);
        outer.color = new Color(1f, 0.38f + 0.25f * (1f - p), 0.12f, 1f - p);
        inner.color = new Color(1f, 0.92f, 0.60f, 1f - p);
    }
}
