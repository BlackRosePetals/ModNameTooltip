using System.Diagnostics.CodeAnalysis;
using StardewValley;

namespace ModNameTooltip;

public sealed class ModNameTooltipAPI : IModNameTooltip
{
    // <inheritdoc/>
    public bool TryGetModName(Item? item, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (
            item != null
            && ModEntry.itemTypeToTraceCtx.TryGetValue(item.GetItemTypeId(), out TraceContext? ctx)
            && ctx.TryGetItemModName(item.ItemId, out ModNameInfo? modNameInner)
        )
        {
            modName = modNameInner;
            return true;
        }
        return false;
    }

    // <inheritdoc/>
    public bool TryGetModName(string itemType, string itemId, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (
            ModEntry.itemTypeToTraceCtx.TryGetValue(itemType, out TraceContext? ctx)
            && ctx.TryGetItemModName(itemId, out ModNameInfo? modNameInner)
        )
        {
            modName = modNameInner;
            return true;
        }
        return false;
    }
}
