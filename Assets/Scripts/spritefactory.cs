using UnityEngine;

/// <summary>
/// Procedurally generated sprites (no external assets needed).
/// All textures are created at runtime, so the WebGL build stays self-contained.
/// </summary>
public static class SpriteFactory
{
    static Sprite _white;
    static Sprite _circle;

    public static Sprite White()
    {
        if (_white == null)
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color32[16];
            for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply();
            _white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            _white.name = "White1x1";
        }
        return _white;
    }

    public static Sprite Circle()
    {
        if (_circle == null)
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var px = new Color32[s * s];
            var c = new Vector2(s / 2f - 0.5f, s / 2f - 0.5f);
            float r = s / 2f - 1f;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    px[y * s + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            _circle = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            _circle.name = "Circle1x1";
        }
        return _circle;
    }
}
