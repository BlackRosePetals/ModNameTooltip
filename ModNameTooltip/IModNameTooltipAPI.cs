using System.Diagnostics.CodeAnalysis;
using StardewValley;

namespace ModNameTooltip;

public interface IModNameTooltip
{
    /// <summary>
    /// Try and get the name of the mod which added a particular item.
    /// </summary>
    /// <param name="item">Item to find mod name for</param>
    /// <param name="modName">The mod's name, or 'Stardew Valley' for vanilla items.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetItemModName(Item? item, [NotNullWhen(true)] out string? modName);
}
