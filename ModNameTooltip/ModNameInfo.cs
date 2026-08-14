using System.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace ModNameTooltip;

public sealed record ModNameInfo(string ModId, IModInfo? ModInfo) : IModNameInfo
{
    internal const string ModNameColorKey = $"{ModEntry.ModId}/ModNameColor";
    internal const string STARDEW_VALLEY = "STARDEW_VALLEY";
    internal static readonly ModNameInfo STARDEW = new(STARDEW_VALLEY, null);
    internal static readonly ModNameInfo EMPTY = new(string.Empty, null);
    private static Color? colorMenu = null;
    private static Color? colorTriadic1 = null;
    private static Color? colorTriadic2 = null;

    private readonly bool isStardew = ModId == STARDEW_VALLEY;

    public string ModName =>
        field ??=
            Game1.content.LoadStringReturnNullIfNotFound($"{ModEntry.Asset_ModNames}:{ModId}")
            ?? ModInfo?.Manifest.Name
            ?? ModId;

    public Color ModNameColor =>
        (
            isStardew
                ? (ModEntry.config.Color_SDV_Parsed ?? colorTriadic1)
                : (ModEntry.config.Color_Mod_Parsed ?? colorTriadic2)
        ) ?? Color.SlateBlue;

    public static ModNameInfo Make(string modId)
    {
        if (modId == STARDEW_VALLEY)
            return STARDEW;

        IModInfo? modInfo = ModEntry.help.ModRegistry.Get(modId);
        return new(modId, modInfo);
    }

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
        Utility.drawTextWithShadow(b, txt.ModName, font, new Vector2(x + 16, y), txt.ModNameColor, 1f, -1f, 2, 2);
    }

    internal static void ResetMenuColor()
    {
        colorMenu = null;
        colorTriadic1 = null;
        colorTriadic2 = null;
    }

    internal static void MaybeUpdateMenuColor()
    {
        if (Game1.mouseCursors == null || colorMenu != null)
            return;

        Color[] colors = ArrayPool<Color>.Shared.Rent(Game1.mouseCursors.GetElementCount());
        Game1.mouseCursors.GetData(colors, 0, Game1.mouseCursors.GetElementCount());
        Color menuClr = colors[306 + 320 * 704];
        colorMenu = menuClr;
        ArrayPool<Color>.Shared.Return(colors);

        Utility.RGBtoHSL(menuClr.R, menuClr.G, menuClr.B, out double h1, out double s1, out double l1);

        colorTriadic1 = MakeColor((h1 + 120) % 360, s1, l1);
        colorTriadic2 = MakeColor((h1 + 240) % 360, s1, l1);

        static Color MakeColor(double h2, double s1, double l1)
        {
            Utility.HSLtoRGB(h2, s1, l1 / 2, out int r, out int g, out int b);
            return new(r, g, b);
        }
    }
}
