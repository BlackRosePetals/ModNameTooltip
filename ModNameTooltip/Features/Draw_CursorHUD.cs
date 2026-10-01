using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace ModNameTooltip.Features;

public sealed class Draw_CursorHUD(int screenId)
{
    private const double EVENT_TIMER_LEN = 3500.0;
    private const double TIMER_FADE = 500.0;
    private const int BORDER_WIDTH = 16;

    private Vector2 lastCheckedTile = -Vector2.One;
    private readonly WeakReference<NPC?> hoveredNPC = new(null);
    private readonly WeakReference<FarmAnimal?> hoveredFarmAnimal = new(null);
    private readonly WeakReference<SObject?> hoveredObject = new(null);
    private readonly WeakReference<TerrainFeature?> hoveredTerrain = new(null);
    private readonly WeakReference<Building?> hoveredBuilding = new(null);
    private readonly WeakReference<Event?> hoveredEvent = new(null);
    private IModNameInfo? currentLocationModName = null;

    private double eventNameTimer = -1;

    private void ClearWeakRefs()
    {
        hoveredNPC.SetTarget(null);
        hoveredFarmAnimal.SetTarget(null);
        hoveredObject.SetTarget(null);
        hoveredTerrain.SetTarget(null);
        hoveredBuilding.SetTarget(null);
        hoveredEvent.SetTarget(null);
    }

    private IModNameInfo? hoveredModName;
    private string? hoveredName;
    private Vector2? hoveredPos;

    private Vector2 hoveredNamePos = Vector2.Zero;
    private Vector2 hoveredModNamePos = Vector2.Zero;
    private Vector2 hoveredSize = Vector2.Zero;

    private readonly int screenId = screenId;

    private bool TryMatchNPC(Vector2 mouse, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_NPC)
            return false;
        Rectangle searchBounds = new((int)mouse.X - 32, (int)mouse.Y - 64, 64, 128);
        foreach (NPC character in location.characters)
        {
            if (character.IsInvisible)
                continue;
            if (!character.GetBoundingBox().Intersects(searchBounds))
                continue;
            if (hoveredNPC.TryGetTarget(out NPC? target) && target == character)
                return true;
            ClearWeakRefs();
            hoveredNPC.SetTarget(character);
            if (ModEntry.modNameAPI.TryGetModName(character, out IModNameInfo? modName))
            {
                Vector2 standingPx = character.StandingPixel.ToVector2();
                standingPx.Y -= 48;
                hoveredPos = standingPx;
                if (character is Pet pet)
                {
                    hoveredName = I18n.Hud_FarmAnimal(
                        string.IsNullOrEmpty(pet.displayName) ? pet.Name : pet.displayName,
                        TokenParser.ParseText(pet.GetPetData()?.DisplayName)
                    );
                }
                else
                {
                    hoveredName = string.IsNullOrEmpty(character.displayName) ? character.Name : character.displayName;
                }
                hoveredModName = modName;
                CalculateSizes();
                return true;
            }
        }
        hoveredNPC.SetTarget(null);
        return false;
    }

    private bool TryMatchFarmAnimal(Vector2 mouse, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_FarmAnimal)
            return false;
        foreach (FarmAnimal farmAnimal in location.animals.Values)
        {
            if (!farmAnimal.GetCursorPetBoundingBox().Contains(mouse))
                continue;
            if (hoveredFarmAnimal.TryGetTarget(out FarmAnimal? target) && target == farmAnimal)
                return true;
            ClearWeakRefs();
            hoveredFarmAnimal.SetTarget(farmAnimal);
            if (ModEntry.modNameAPI.TryGetModName(farmAnimal, out IModNameInfo? modName))
            {
                Vector2 standingPx = farmAnimal.StandingPixel.ToVector2();
                standingPx.Y -= 48;
                hoveredPos = standingPx;
                hoveredModName = modName;
                hoveredName = I18n.Hud_FarmAnimal(
                    string.IsNullOrEmpty(farmAnimal.displayName) ? farmAnimal.Name : farmAnimal.displayName,
                    farmAnimal.displayType
                );
                CalculateSizes();
                return true;
            }
        }
        hoveredFarmAnimal.SetTarget(null);
        return false;
    }

    private bool TryMatchObject(Vector2 tile, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_Object)
            return false;
        if (location.objects.TryGetValue(tile, out SObject? obj))
        {
            if (hoveredObject.TryGetTarget(out SObject? target) && target == obj)
                return true;
            ClearWeakRefs();
            hoveredObject.SetTarget(obj);
            if (ModEntry.modNameAPI.TryGetModName(obj, out IModNameInfo? modName))
            {
                hoveredPos = new Vector2(obj.TileLocation.X + 0.5f, obj.TileLocation.Y) * Game1.tileSize;
                hoveredName = obj.DisplayName;
                hoveredModName = modName;
                CalculateSizes();
                return true;
            }
        }
        hoveredObject.SetTarget(null);
        return false;
    }

    private static string GetWildTreeName(Tree tree)
    {
        if (
            tree.GetData()?.CustomFields?.TryGetValue("UIInfoSuite.ExtendedData/DisplayName", out string? treeName1)
            ?? false
        )
        {
            return treeName1;
        }
        else if (I18n.GetByKey($"tree.name.{tree.treeType.Value}") is Translation treeName2)
        {
            return treeName2.ToString();
        }
        return tree.treeType.Value.ToString();
    }

    private bool TryMatchTerrainFeature(Vector2 tile, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_TerrainFeature)
            return false;
        if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature? terrain))
        {
            if (hoveredTerrain.TryGetTarget(out TerrainFeature? target) && target == terrain)
                return true;
            ClearWeakRefs();
            if (ModEntry.config.Enable_HUD_Crop && terrain is HoeDirt dirt && dirt.crop is Crop crop)
                hoveredName = ItemRegistry.GetDataOrErrorItem(crop.netSeedIndex.Value).DisplayName;
            else if (ModEntry.config.Enable_HUD_WildTree && terrain is Tree tree)
                hoveredName = GetWildTreeName(tree);
            else if (ModEntry.config.Enable_HUD_FruitTree && terrain is FruitTree fruitTree)
                hoveredName = TokenParser.ParseText(fruitTree.GetData()?.DisplayName ?? fruitTree.treeId.Value);
            else
                return false;
            hoveredTerrain.SetTarget(terrain);
            if (ModEntry.modNameAPI.TryGetModName(terrain, out IModNameInfo? modName))
            {
                hoveredPos = new Vector2(terrain.Tile.X + 0.5f, terrain.Tile.Y) * Game1.tileSize;
                hoveredModName = modName;
                CalculateSizes();
                return true;
            }
        }
        hoveredTerrain.SetTarget(null);
        return false;
    }

    private bool TryMatchBuilding(Vector2 tile, GameLocation location)
    {
        if (!ModEntry.config.Enable_HUD_Building)
            return false;
        foreach (Building building in location.buildings)
        {
            if (!building.occupiesTile(tile) || building.GetData() is not BuildingData data)
                continue;
            if (hoveredBuilding.TryGetTarget(out Building? target) && target == building)
                return true;
            ClearWeakRefs();
            hoveredBuilding.SetTarget(building);
            if (ModEntry.modNameAPI.TryGetModName(building, out IModNameInfo? modName))
            {
                hoveredPos = new Vector2(
                    (building.tileX.Value + (building.tilesWide.Value / 2f)) * Game1.tileSize,
                    (building.tileY.Value + building.tilesHigh.Value - 1) * Game1.tileSize
                );
                hoveredName = TokenParser.ParseText(data.Name);
                hoveredModName = modName;
                CalculateSizes();
                return true;
            }
        }
        hoveredBuilding.SetTarget(null);
        return false;
    }

    private bool TryMatchEvent()
    {
        if (
            ModEntry.config.Enable_HUD_Event
            && eventNameTimer == -1
            && !Game1.globalFade
            && Game1.currentLocation is GameLocation location
            && location.currentEvent is Event sdvEvent
        )
        {
            if (hoveredEvent.TryGetTarget(out Event? target) && target.id == sdvEvent.id)
                return true;
            ClearWeakRefs();
            hoveredEvent.SetTarget(sdvEvent);
            sdvEvent.onEventFinished = (Action)
                Delegate.Combine(
                    sdvEvent.onEventFinished,
                    () =>
                    {
                        ClearHovered();
                        hoveredEvent.SetTarget(null);
                        eventNameTimer = -1;
                    }
                );
            if (ModEntry.modNameAPI.TryGetModName(sdvEvent, out IModNameInfo? modName))
            {
                if (ModEntry.config.HoldToShow_HUD.IsBound)
                    eventNameTimer = -2;
                else
                    eventNameTimer = EVENT_TIMER_LEN;
                hoveredPos = null;
                hoveredName = I18n.Hud_Event(sdvEvent.id);
                hoveredModName = modName;
                CalculateSizes();
                return true;
            }
        }
        return false;
    }

    private bool FallbackToLocation(GameLocation location)
    {
        if (ModEntry.config.Enable_HUD_Location && currentLocationModName != null)
        {
            ClearWeakRefs();
            hoveredPos = null;
            hoveredName = location.DisplayName ?? location.NameOrUniqueName;
            hoveredModName = currentLocationModName;
            CalculateSizes();
            return true;
        }
        return false;
    }

    internal void ClearHovered()
    {
        ClearWeakRefs();
        hoveredName = null;
        hoveredModName = null;
        hoveredPos = null;
        lastCheckedTile = -Vector2.One;
        CalculateSizes();
    }

    internal void CheckTileOrCursor(Vector2 mouse, Vector2 tile)
    {
        if (!ModEntry.config.Enable_HUD || screenId != Context.ScreenId)
            return;
        if (lastCheckedTile == tile)
            return;
        if (
            Game1.currentLocation is GameLocation location
            && Game1.activeClickableMenu == null
            && location.currentEvent == null
            && eventNameTimer == -1
        )
        {
            lastCheckedTile = tile;
            if (
                TryMatchNPC(mouse, location)
                || TryMatchFarmAnimal(mouse, location)
                || TryMatchObject(tile, location)
                || TryMatchTerrainFeature(tile, location)
                || TryMatchBuilding(tile, location)
                || FallbackToLocation(location)
            )
            {
                return;
            }
        }
        if (eventNameTimer == -1)
            ClearHovered();
    }

    internal void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        if (eventNameTimer >= 0 && !Game1.globalFade)
        {
            eventNameTimer -= Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
            if (eventNameTimer < 0)
            {
                ClearHovered();
                eventNameTimer = -1;
            }
        }
    }

    internal void OnNewLocation(GameLocation location)
    {
        if (location.currentEvent != null)
        {
            TryMatchEvent();
        }
        else if (ModEntry.modNameAPI.TryGetModName(location, out currentLocationModName))
        {
            eventNameTimer = -1;
            FallbackToLocation(location);
        }
        else
        {
            ClearHovered();
        }
    }

    internal void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (
            !(ModEntry.config.Enable_HUD_Display && screenId == Context.ScreenId && Game1.currentLocation != null)
            || hoveredModName == null
            || hoveredName == null
        )
            return;

        float colorMult = 1f;
        if (eventNameTimer >= 0 && eventNameTimer <= TIMER_FADE)
        {
            double prog = eventNameTimer / TIMER_FADE;
            colorMult = (float)(prog * prog);
            if (colorMult <= 0)
                return;
        }

        int x = (int)(Game1.uiViewport.Width / 2 - (hoveredSize.X / 2));
        int y = 4;

        if (ModEntry.config.ShowHUDAboveTile && hoveredPos.HasValue)
        {
            Vector2 pos = Game1.GlobalToLocal(hoveredPos.Value);
            x = (int)pos.X;
            y = (int)pos.Y;
            x -= (int)(hoveredSize.X / 2);
            if (y + hoveredSize.Y + Game1.tileSize + 20 >= Game1.viewport.Height)
            {
                y -= (int)(hoveredSize.Y + Game1.tileSize);
            }
            else
            {
                y += Game1.tileSize + 20;
            }
        }
        else if (Game1.IsHudDrawn && Game1.activeClickableMenu == null && Game1.currentLocation.currentEvent == null)
        {
            foreach (IClickableMenu clickableMenu in Game1.onScreenMenus)
            {
                if (clickableMenu is Toolbar toolbar)
                {
                    if (toolbar.yPositionOnScreen == Game1.uiViewport.Height)
                        y = 4;
                    else
                        y = (int)(Game1.viewport.Height - hoveredSize.Y - 4);
                    break;
                }
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
            Color.White * colorMult,
            drawShadow: false
        );
        Utility.drawTextWithShadow(
            e.SpriteBatch,
            hoveredName,
            Game1.smallFont,
            new(x + hoveredNamePos.X, y + hoveredNamePos.Y),
            Game1.textColor * colorMult,
            shadowIntensity: colorMult
        );
        Utility.drawTextWithShadow(
            e.SpriteBatch,
            hoveredModName.ModName,
            Game1.smallFont,
            new(x + hoveredModNamePos.X, y + hoveredModNamePos.Y),
            hoveredModName.ModNameColor * colorMult,
            shadowIntensity: colorMult
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
