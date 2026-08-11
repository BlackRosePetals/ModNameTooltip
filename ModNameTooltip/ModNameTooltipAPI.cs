using System.Diagnostics.CodeAnalysis;
using StardewValley;

namespace ModNameTooltip;

public sealed class ModNameTooltipAPI : IModNameTooltip
{
    // <inheritdoc/>
    public bool TryGetItemModName(Item? item, [NotNullWhen(true)] out string? modName)
    {
        return ModEntry.TryGetItemModName(item, out modName);
    }
}
