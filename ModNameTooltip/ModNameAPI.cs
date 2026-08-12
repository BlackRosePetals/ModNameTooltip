using System.Diagnostics.CodeAnalysis;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace ModNameTooltip;

public sealed class ModNameAPI : IModNameAPI
{
    // <inheritdoc/>
    public bool TryGetModName(Item? item, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (
            item != null
            && ModEntry.itemTypeToTraceCtx.TryGetValue(item.GetItemTypeId(), out TraceContext? ctx)
            && ctx.TryGetModName(item.ItemId, out ModNameInfo? modNameInner)
        )
        {
            modName = modNameInner;
            return true;
        }
        return false;
    }

    // <inheritdoc/>
    public bool TryGetModName(Character? character, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (character is FarmAnimal farmAnimal)
        {
            return TryGetModName_FromFarmAnimalType(farmAnimal.type.Value, out modName);
        }
        else if (character is NPC)
        {
            return TryGetModName_FromNpcName(character.Name, out modName);
        }
        return false;
    }

    // <inheritdoc/>
    public bool TryGetModName_FromItemId(string itemId, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (
            ItemRegistry.GetData(itemId) is ParsedItemData parsedItemData
            && ModEntry.itemTypeToTraceCtx.TryGetValue(parsedItemData.GetItemTypeId(), out TraceContext? ctx)
            && ctx.TryGetModName(parsedItemData.ItemId, out ModNameInfo? modNameInner)
        )
        {
            modName = modNameInner;
            return true;
        }
        return false;
    }

    // <inheritdoc/>
    public bool TryGetModName_FromFarmAnimalType(string farmAnimalType, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (ModEntry.farmAnimalTraceCtx.TryGetModName(farmAnimalType, out ModNameInfo? modNameInner))
        {
            modName = modNameInner;
            return true;
        }
        return false;
    }

    // <inheritdoc/>
    public bool TryGetModName_FromNpcName(string npcName, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (ModEntry.npcTraceCtx.TryGetModName(npcName, out ModNameInfo? modNameInner))
        {
            modName = modNameInner;
            return true;
        }
        return false;
    }
}
