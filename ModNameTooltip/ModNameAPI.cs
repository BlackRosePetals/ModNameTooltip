using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
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
        if (character is Pet pet)
        {
            return TryGetModName_FromPetType(pet.petType.Value, out modName);
        }
        else if (character is NPC)
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
    public bool TryGetModName(Building? building, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        return building != null && TryGetModName_FromBuildingId(building.buildingType.Value, out modName);
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

    public bool TryGetModName(GameLocation? location, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (location == null)
            return false;
        string locationId = location.Name;
        if (location is MineShaft)
            locationId = "UndergroundMine";
        else if (location is Cellar && locationId.StartsWith("Cellar"))
            locationId = "Cellar";
        if (string.IsNullOrEmpty(locationId))
            return false;
        return TryGetModName_FromLocationId(locationId, out modName);
    }

    public bool TryGetModName(Event? sdvEvent, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        modName = null;
        if (sdvEvent?.fromAssetName == null)
            return false;
        return TryGetModName_FromEventId(sdvEvent.fromAssetName, sdvEvent.id, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromNpcName(string npcName, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.npcTraceCtx, npcName, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromPetType(string petType, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.petTraceCtx, petType, out modName);
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

    // <inheritdoc/>
    public bool TryGetModName_FromBuildingId(string buildingId, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.buildingsTraceCtx, buildingId, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromLocationId(string locationId, [NotNullWhen(true)] out IModNameInfo? modName)
    {
        return TryGetModNameFromCtx(ModEntry.locationsTraceCtx, locationId, out modName);
    }

    // <inheritdoc/>
    public bool TryGetModName_FromEventId(
        string eventAsset,
        string eventId,
        [NotNullWhen(true)] out IModNameInfo? modName
    )
    {
        modName = null;
        IAssetName eventAssetParsed = ModEntry.help.GameContent.ParseAssetName(eventAsset);
        if (!ModEntry.eventAssetToTraceCtx.TryGetValue(eventAssetParsed, out TraceContext? ctx))
            return false;
        return TryGetModNameFromCtx(ctx, eventId, out modName);
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
