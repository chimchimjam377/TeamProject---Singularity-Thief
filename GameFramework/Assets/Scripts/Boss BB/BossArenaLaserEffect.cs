using UnityEngine;

// Automatic full-viewport fallback. Optional authored warning/beam objects override each effect.
// Visual only; BossController handles the unavoidable damage.
[RequireComponent(typeof(BossController))]
public class BossArenaLaserEffect : MonoBehaviour
{
    private BossController boss;
    private Texture2D beam;
    private void Awake()
    {
        boss = GetComponent<BossController>();
        beam = new Texture2D(128, 1, TextureFormat.RGBA32, false);
        beam.wrapMode = TextureWrapMode.Clamp;
        beam.filterMode = FilterMode.Bilinear;
        Color red = new Color(0.92f, 0.22f, 0.12f);
        Color orange = new Color(1f, 0.62f, 0.32f);
        Color cream = new Color(1f, 0.97f, 0.80f);
        for (int x = 0; x < 128; x++)
        {
            float edge = Mathf.Min(x, 127 - x) / 127f;
            Color c = edge < 0.06f ? Color.Lerp(red, orange, edge / 0.06f)
                : Color.Lerp(orange, cream, Mathf.Clamp01((edge - 0.06f) / 0.12f));
            beam.SetPixel(x, 0, c);
        }
        beam.Apply(false, true);
    }
    private void OnGUI()
    {
        if (!boss || Event.current.type != EventType.Repaint) return;
        Rect px = boss.ArenaCamera ? boss.ArenaCamera.pixelRect : new Rect(0, 0, Screen.width, Screen.height);
        Rect view = new Rect(px.x, Screen.height - px.yMax, px.width, px.height);
        Color old = GUI.color;
        int oldDepth = GUI.depth;
        GUI.depth = -100;
        if (boss.UseAutomaticWarning && boss.LaserWarningVisible)
        {
            // Gentle pulse, not a high-frequency flash.
            float alpha = 0.12f + 0.06f * Mathf.Sin(Time.time * 3f);
            GUI.color = new Color(1f, 0.23f, 0.12f, alpha);
            GUI.DrawTexture(view, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.52f, 0.25f, 0.85f);
            GUI.DrawTexture(new Rect(view.x, view.y, view.width, 5f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(view.x, view.yMax - 5f, view.width, 5f), Texture2D.whiteTexture);
        }
        if (boss.UseAutomaticBeam && boss.LaserBeamVisible && beam)
        {
            GUI.color = new Color(1f, 1f, 1f, boss.LaserBeamOpacity);
            GUI.DrawTexture(new Rect(view.x, view.y, view.width,
                view.height * boss.LaserBeamDepth), beam, ScaleMode.StretchToFill, true);
        }
        GUI.color = old;
        GUI.depth = oldDepth;
    }
    private void OnDestroy() { if (beam) Destroy(beam); }
}
