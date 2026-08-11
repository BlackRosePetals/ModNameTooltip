using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ModNameTooltip;

public sealed record ModNameText(string ModId, string ModName, Color? ModNameColor = null) : IModNameText
{
    internal const string STARDEW_VALLEY = "STARDEW_VALLEY";
    internal static readonly ModNameText STARDEW = new(STARDEW_VALLEY, I18n.Game_Title());
    internal static readonly ModNameText EMPTY = new(string.Empty, string.Empty);

    public static Vector2 Measure(ModNameText? txt, SpriteFont font)
    {
        if (txt == null)
            return Vector2.Zero;
        if (string.IsNullOrEmpty(txt.ModName))
            return Vector2.Zero;
        return font.MeasureString(txt.ModName);
    }

    public static void Draw(ModNameText? txt, SpriteBatch b, SpriteFont font, int x, int y)
    {
        if (txt == null)
            return;
        if (string.IsNullOrEmpty(txt.ModName))
            return;
        Utility.drawTextWithShadow(b, txt.ModName, font, new Vector2(x + 16, y), txt.ModNameColor ?? Color.SlateBlue);
    }
}
