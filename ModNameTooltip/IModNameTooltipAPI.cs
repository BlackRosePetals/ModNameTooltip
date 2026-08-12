using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ModNameTooltip;

public interface IModNameInfo
{
    /// <summary>The mod's unique id</summary>
    string ModId { get; }

    /// <summary>The mod's name, derived from either the manifest or the special translation asset</summary>
    string ModName { get; }

    /// <summary>Display color for this mod's name</summary>
    Color? ModNameColor { get; }
}

public interface IModNameTooltip
{
    /// <summary>
    /// Try and get info about which mod added an item using a real item instance
    /// </summary>
    /// <param name="item">Item to find mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(Item? item, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod using the item type and item id
    /// </summary>
    /// <param name="itemType">The item type id, such as '(O)'</param>
    /// <param name="itemId">The unqualified item id</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(string itemType, string itemId, [NotNullWhen(true)] out IModNameInfo? modName);
}
