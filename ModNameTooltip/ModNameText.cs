using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ModNameTooltip;

public sealed record ModNameInfo(string ModId, IModInfo? ModInfo, Color? ModNameColor = null) : IModNameInfo
{
    internal const string ModNameColorKey = $"{ModEntry.ModId}/ModNameColor";
    internal const string STARDEW_VALLEY = "STARDEW_VALLEY";
    internal static readonly ModNameInfo STARDEW = new(STARDEW_VALLEY, null, Color.ForestGreen);
    internal static readonly ModNameInfo EMPTY = new(string.Empty, null);

    public static ModNameInfo Make(string modId)
    {
        if (modId == STARDEW_VALLEY)
            return STARDEW;

        IModInfo? modInfo = ModEntry.help.ModRegistry.Get(modId);
        Color? modNameColor = null;
        if (
            !modNameColor.HasValue
            && (modInfo?.Manifest.ExtraFields.TryGetValue(ModNameColorKey, out object? modNameColorThing) ?? false)
            && modNameColorThing is string modNameColorStr
        )
        {
            modNameColor = Utility.StringToColor(modNameColorStr);
        }
        return new(modId, modInfo, modNameColor);
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
        Utility.drawTextWithShadow(b, txt.ModName, font, new Vector2(x + 16, y), txt.ModNameColor ?? Color.SlateBlue);
    }
}
