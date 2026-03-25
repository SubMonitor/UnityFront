using TMPro;
using UnityEngine;

namespace SubMonitor.App.UI.Common
{
    public static class UiTheme
    {
        public static readonly Color Background = new Color32(244, 246, 249, 255);
        public static readonly Color Surface = new Color32(255, 255, 255, 255);
        public static readonly Color SurfaceMuted = new Color32(236, 241, 248, 255);
        public static readonly Color Accent = new Color32(30, 100, 225, 255);
        public static readonly Color AccentDark = new Color32(17, 75, 170, 255);
        public static readonly Color TextPrimary = new Color32(34, 41, 52, 255);
        public static readonly Color TextSecondary = new Color32(112, 123, 139, 255);
        public static readonly Color Success = new Color32(43, 150, 88, 255);
        public static readonly Color Error = new Color32(196, 66, 66, 255);
        public static readonly Color Warning = new Color32(205, 138, 23, 255);
        public static readonly Color Overlay = new Color32(19, 26, 36, 186);
        public static readonly Color White = Color.white;

        public static TMP_FontAsset ResolveFontAsset()
        {
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                return defaultFont;
            }

            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        public static Sprite CreateWhiteSprite(ref Sprite cachedSprite, string spriteName)
        {
            if (cachedSprite != null)
            {
                return cachedSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.name = spriteName + "_Texture";
            cachedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            cachedSprite.name = spriteName;
            return cachedSprite;
        }
    }
}
