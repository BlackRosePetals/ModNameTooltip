using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace ModNameTooltip.Features;

public sealed class Draw_CursorHUD(int screenId)
{
    private const int BORDER_WIDTH = 16;

    private readonly WeakReference<NPC?> hoveredNPC = new(null);
    private readonly WeakReference<FarmAnimal?> hoveredFarmAnimal = new(null);
    private readonly WeakReference<SObject?> hoveredObject = new(null);
    private readonly WeakReference<TerrainFeature?> hoveredTerrain = new(null);

    private void ClearWeakRefs()
    {
        hoveredNPC.SetTarget(null);
        hoveredFarmAnimal.SetTarget(null);
        hoveredObject.SetTarget(null);
        hoveredTerrain.SetTarget(null);
    }

    private IModNameInfo? hoveredModName;
    private string? hoveredName;

    private Vector2 hoveredNamePos = Vector2.Zero;
    private Vector2 hoveredModNamePos = Vector2.Zero;
    private Vector2 hoveredSize = Vector2.Zero;

    private readonly int screenId = screenId;

    private bool TryMatchNPC(ICursorPosition cursor, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_NPC)
            return false;
        Rectangle searchBounds = new((int)cursor.Tile.X * 64, (int)cursor.Tile.Y * 64, 64, 128);
        foreach (NPC character in location.characters)
        {
            if (character.GetBoundingBox().Intersects(searchBounds))
            {
                if (hoveredNPC.TryGetTarget(out NPC? target) && target == character)
                    return true;
                ClearWeakRefs();
                hoveredNPC.SetTarget(character);
                if (ModEntry.modNameAPI.TryGetModName(character, out IModNameInfo? modName))
                {
                    hoveredModName = modName;
                    hoveredName = string.IsNullOrEmpty(character.displayName) ? character.Name : character.displayName;
                    CalculateSizes();
                    return true;
                }
            }
        }
        return false;
    }

    private bool TryMatchFarmAnimal(ICursorPosition cursor, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_FarmAnimal)
            return false;
        foreach (FarmAnimal farmAnimal in location.animals.Values)
        {
            if (farmAnimal.GetCursorPetBoundingBox().Contains(cursor.AbsolutePixels))
            {
                if (hoveredFarmAnimal.TryGetTarget(out FarmAnimal? target) && target == farmAnimal)
                    return true;
                ClearWeakRefs();
                hoveredFarmAnimal.SetTarget(farmAnimal);
                if (ModEntry.modNameAPI.TryGetModName(farmAnimal, out IModNameInfo? modName))
                {
                    hoveredModName = modName;
                    hoveredName = string.IsNullOrEmpty(farmAnimal.displayName)
                        ? farmAnimal.Name
                        : farmAnimal.displayName;
                    CalculateSizes();
                    return true;
                }
            }
        }
        return false;
    }

    private bool TryMatchObject(ICursorPosition cursor, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_Object)
            return false;
        if (location.objects.TryGetValue(cursor.Tile, out SObject? obj))
        {
            if (hoveredObject.TryGetTarget(out SObject? target) && target == obj)
                return true;
            ClearWeakRefs();
            hoveredObject.SetTarget(obj);
            if (ModEntry.modNameAPI.TryGetModName(obj, out IModNameInfo? modName))
            {
                hoveredName = obj.DisplayName;
                hoveredModName = modName;
                CalculateSizes();
                return true;
            }
        }
        hoveredObject.SetTarget(null);
        return false;
    }

    private static readonly Func<Tree, string>? lookupAnythingTreeSubjectGetName = AccessTools
        .DeclaredMethod("Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures.TreeSubject:GetName")
        ?.CreateDelegate<Func<Tree, string>>();

    private static string GetWildTreeName(Tree tree)
    {
        if (
            tree.GetData()?.CustomFields?.TryGetValue("UIInfoSuite.ExtendedData/DisplayName", out string? treeName1)
            ?? false
        )
        {
            return treeName1;
        }
        else if (lookupAnythingTreeSubjectGetName?.Invoke(tree) is string treeName2)
        {
            return treeName2;
        }
        return tree.treeType.Value.ToString();
    }

    private bool TryMatchTerrainFeature(ICursorPosition cursor, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_TerrainFeature)
            return false;
        if (location.terrainFeatures.TryGetValue(cursor.Tile, out TerrainFeature? terrain))
        {
            if (hoveredTerrain.TryGetTarget(out TerrainFeature? target) && target == terrain)
                return true;
            ClearWeakRefs();
            hoveredTerrain.SetTarget(terrain);
            if (ModEntry.modNameAPI.TryGetModName(terrain, out IModNameInfo? modName))
            {
                if (terrain is HoeDirt dirt && dirt.crop is Crop crop)
                    hoveredName = ItemRegistry.GetDataOrErrorItem(crop.netSeedIndex.Value).DisplayName;
                else if (terrain is Tree tree)
                    hoveredName = GetWildTreeName(tree);
                else if (terrain is FruitTree fruitTree)
                    hoveredName = TokenParser.ParseText(fruitTree.GetData()?.DisplayName ?? fruitTree.treeId.Value);
                else
                    hoveredName = $"{terrain.GetType().Name}[{cursor.Tile}]";
            }
            hoveredModName = modName;
            CalculateSizes();
            return true;
        }
        return false;
    }

    private void ClearHovered()
    {
        ClearWeakRefs();
        hoveredName = null;
        hoveredModName = null;
        CalculateSizes();
    }

    internal void OnCursorMoved(CursorMovedEventArgs e)
    {
        if (
            ModEntry.config.Enable_HUD
            && screenId == Context.ScreenId
            && Game1.currentLocation is GameLocation location
            && Game1.activeClickableMenu == null
            && Game1.currentLocation.currentEvent == null
        )
        {
            ICursorPosition newPos = e.NewPosition;
            if (
                TryMatchNPC(newPos, location)
                || TryMatchFarmAnimal(newPos, location)
                || TryMatchObject(newPos, location)
                || TryMatchTerrainFeature(newPos, location)
            )
            {
                return;
            }
        }
        ClearHovered();
    }

    internal void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (
            !(
                ModEntry.config.Enable_HUD
                && screenId == Context.ScreenId
                && Game1.currentLocation != null
                && Game1.activeClickableMenu == null
                && Game1.currentLocation.currentEvent == null
            )
            || hoveredModName == null
            || hoveredName == null
        )
            return;

        int x = 8;
        int y = 80;

        foreach (IClickableMenu clickableMenu in Game1.onScreenMenus)
        {
            if (clickableMenu is Toolbar toolbar)
            {
                x = (int)(Game1.uiViewport.Width / 2 - (hoveredSize.X / 2));
                if (toolbar.yPositionOnScreen == Game1.uiViewport.Height)
                    y = 4;
                else
                    y = (int)(Game1.viewport.Height - hoveredSize.Y - 4);
                break;
            }
        }

        IClickableMenu.drawTextureBox(
            e.SpriteBatch,
            Game1.menuTexture,
            new Rectangle(0, 256, 60, 60),
            x,
            y,
            (int)hoveredSize.X,
            (int)hoveredSize.Y,
            Color.White,
            drawShadow: false
        );
        Utility.drawTextWithShadow(
            e.SpriteBatch,
            hoveredName,
            Game1.smallFont,
            new(x + hoveredNamePos.X, y + hoveredNamePos.Y),
            Game1.textColor
        );
        Utility.drawTextWithShadow(
            e.SpriteBatch,
            hoveredModName.ModName,
            Game1.smallFont,
            new(x + hoveredModNamePos.X, y + hoveredModNamePos.Y),
            hoveredModName.ModNameColor
        );
    }

    private void CalculateSizes()
    {
        if (hoveredName == null || hoveredModName == null)
        {
            hoveredSize = Vector2.Zero;
            hoveredNamePos = Vector2.Zero;
            hoveredModNamePos = Vector2.Zero;
            return;
        }

        Vector2 hoveredCharacterNameSize = Game1.smallFont.MeasureString(hoveredName);
        Vector2 hoveredModNameSize = Game1.smallFont.MeasureString(hoveredModName.ModName);
        hoveredSize = new(
            MathF.Ceiling(MathF.Max(hoveredCharacterNameSize.X, hoveredModNameSize.X)) + BORDER_WIDTH * 2,
            MathF.Ceiling(hoveredCharacterNameSize.Y + hoveredModNameSize.Y) + BORDER_WIDTH * 2 - 4
        );
        if (hoveredCharacterNameSize.X < hoveredModNameSize.X)
        {
            hoveredNamePos = new(
                BORDER_WIDTH + hoveredModNameSize.X / 2 - hoveredCharacterNameSize.X / 2,
                BORDER_WIDTH
            );
            hoveredModNamePos = new(BORDER_WIDTH, BORDER_WIDTH + hoveredCharacterNameSize.Y);
        }
        else
        {
            hoveredNamePos = new(BORDER_WIDTH, BORDER_WIDTH);
            hoveredModNamePos = new(
                BORDER_WIDTH + hoveredCharacterNameSize.X / 2 - hoveredModNameSize.X / 2,
                BORDER_WIDTH + hoveredCharacterNameSize.Y
            );
        }
    }
}
