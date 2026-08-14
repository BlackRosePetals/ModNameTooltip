using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;

namespace ModNameTooltip;

public interface IModNameInfo
{
    /// <summary>The mod's unique id</summary>
    string ModId { get; }

    /// <summary>The mod info, if this entry matches a real mod</summary>
    IModInfo? ModInfo { get; }

    /// <summary>The mod's name, derived from either the manifest or the special translation asset</summary>
    string ModName { get; }

    /// <summary>Display color for this mod's name</summary>
    Color ModNameColor { get; }
}

public interface IModNameAPI
{
    /// <summary>
    /// Try and get info about which mod added an item using a real item instance.
    /// Supports all vanilla item types, but not mod added item definitions.
    /// </summary>
    /// <param name="item">Item to find mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(Item? item, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod using a real character instance. This supports:
    /// <list type="bullet">
    /// <item>NPC</item>
    /// <item>Character</item>
    /// </list>
    /// </summary>
    /// <param name="character">The character to find mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(Character? character, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod using a real terrain feature instance. This supports:
    /// <list type="bullet">
    /// <item>HoeDirt/Crop</item>
    /// <item>Tree (a.k.a. wild trees)</item>
    /// <item>FruitTree</item>
    /// </list>
    /// </summary>
    /// <param name="terrainFeature">The terrain feature to find mod id for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(TerrainFeature? terrainFeature, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod using a real building instance.
    /// </list>
    /// </summary>
    /// <param name="building">The building mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(Building? building, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added an item using the item id
    /// </summary>
    /// <param name="itemId">The item id, qualified or unqualified</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromItemId(string itemId, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a NPC using the name
    /// </summary>
    /// <param name="npcName">The NPC's internal name</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromNpcName(string npcName, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a pet using the pet type
    /// </summary>
    /// <param name="itemId">The item id, qualified or unqualified</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromPetType(string petType, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a farm animal using the farm animal type
    /// </summary>
    /// <param name="farmAnimalType">The farm animal type</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromFarmAnimalType(string farmAnimalType, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a crop using the crop id
    /// </summary>
    /// <param name="cropId">The crop id</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromCropId(string cropId, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a wild tree using the tree id
    /// </summary>
    /// <param name="treeId">The tree id</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromWildTreeId(string treeId, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a wild tree using the tree id
    /// </summary>
    /// <param name="treeId">The tree id</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromFruitTreeId(string treeId, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a building using the building id
    /// </summary>
    /// <param name="buildingId">The building id</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromBuildingId(string buildingId, [NotNullWhen(true)] out IModNameInfo? modName);
}
