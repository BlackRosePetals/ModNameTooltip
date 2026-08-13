using System.Security.Cryptography;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ModNameTooltip;

public sealed record ModNameInfo(string ModId, IModInfo? ModInfo, Color ModNameColor) : IModNameInfo
{
    internal const string ModNameColorKey = $"{ModEntry.ModId}/ModNameColor";
    internal const string STARDEW_VALLEY = "STARDEW_VALLEY";
    internal static readonly ModNameInfo STARDEW = new(STARDEW_VALLEY, null, HashColor(STARDEW_VALLEY));
    internal static readonly ModNameInfo EMPTY = new(string.Empty, null, Color.Transparent);

    public static ModNameInfo Make(string modId)
    {
        if (modId == STARDEW_VALLEY)
            return STARDEW;

        IModInfo? modInfo = ModEntry.help.ModRegistry.Get(modId);
        // TODO: maybe this should be an asset thing too
        // if (
        //     (modInfo?.Manifest.ExtraFields.TryGetValue(ModNameColorKey, out object? modNameColorThing) ?? false)
        //     && modNameColorThing is string modNameColorStr
        // )
        // {
        //     modNameColor = Utility.StringToColor(modNameColorStr) ?? Color.SlateBlue;
        // }
        return new(modId, modInfo, HashColor(modId));
    }

    internal static Color HashColor(string modId)
    {
        if (string.IsNullOrEmpty(modId))
            return Color.Transparent;
        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(modId));
        Color hashColor = new(hash[15], hash[5], hash[10]);
        Utility.RGBtoHSL(hashColor.R, hashColor.G, hashColor.B, out double h, out double s, out double l);
        Utility.HSLtoRGB(h, s, l / 2, out int r, out int g, out int b);
        hashColor = new(r, g, b);
        if (ModEntry.menuColor is Color menuColor && IsColorTooSimilar(hashColor, menuColor))
        {
            return new(menuColor.PackedValue ^ 0x00FFFFFF);
        }
        if (IsColorTooSimilar(hashColor, Game1.textColor))
        {
            return ModEntry.menuColor.HasValue
                ? new(ModEntry.menuColor.Value.PackedValue ^ 0x00FFFFFF)
                : Color.SlateBlue;
        }
        return hashColor;
    }

    private static bool IsColorTooSimilar(Color hashColor, Color menuColor)
    {
        Utility.RGBtoHSL(menuColor.R, menuColor.G, menuColor.B, out double h1, out double s1, out double l1);
        Utility.RGBtoHSL(hashColor.R, hashColor.G, hashColor.B, out double h2, out double s2, out double l2);
        return Math.Abs(h1 - h2) < 0.1 && Math.Abs(s1 - s2) < 0.1 && Math.Abs(l1 - l2) < 0.1;
    }

    public string ModName { get; } =
        Game1.content.LoadStringReturnNullIfNotFound($"{ModEntry.ModNameString}:{ModId}")
        ?? ModInfo?.Manifest.Name
        ?? ModId;

    public static Vector2 Measure(ModNameInfo? txt, SpriteFont font)
    {
        if (txt == null)
            return Vector2.Zero;
        if (string.IsNullOrEmpty(txt.ModId))
            return Vector2.Zero;
        return font.MeasureString(txt.ModName);
    }

    public static void Draw(ModNameInfo? txt, SpriteBatch b, SpriteFont font, int x, int y)
    {
        if (txt == null)
            return;
        if (string.IsNullOrEmpty(txt.ModId))
            return;
        Utility.drawTextWithShadow(b, txt.ModName, font, new Vector2(x + 16, y), txt.ModNameColor);
    }
}
