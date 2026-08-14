using System.Diagnostics.CodeAnalysis;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

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
        if (character is NPC)
        {
            return TryGetModName_FromNpcName(character.Name, out modName);
        }
        else if (character is FarmAnimal farmAnimal)
        {
            return TryGetModName_FromFarmAnimalType(farmAnimal.type.Value, out modName);
        }
        return false;
    }

    // <inheritdoc/>
    public bool TryGetModName(TerrainFeature? terrainFeature, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (terrainFeature is HoeDirt dirt && dirt.crop is Crop crop)
        {
            return TryGetModName_FromCropId(crop.netSeedIndex.Value, out modName);
        }
        else if (terrainFeature is Tree tree)
        {
            return TryGetModName_FromWildTreeId(tree.treeType.Value, out modName);
        }
        else if (terrainFeature is FruitTree fruitTree)
        {
            return TryGetModName_FromFruitTreeId(fruitTree.treeId.Value, out modName);
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
    public bool TryGetModName_FromNpcName(string npcName, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.npcTraceCtx, npcName, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromFarmAnimalType(string farmAnimalType, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.farmAnimalTraceCtx, farmAnimalType, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromCropId(string cropId, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.cropTraceCtx, cropId, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromWildTreeId(string treeId, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.wildTreeTraceCtx, treeId, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromFruitTreeId(string treeId, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.fruitTreeTraceCtx, treeId, out modName);
    }

    private static bool TryGetModNameFromCtx(TraceContext ctx, string id, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (!string.IsNullOrEmpty(id) && ctx.TryGetModName(id, out ModNameInfo? modNameInner))
        {
            modName = modNameInner;
            return true;
        }
        return false;
    }
}
