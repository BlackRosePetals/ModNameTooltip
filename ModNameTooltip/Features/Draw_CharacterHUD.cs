using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ModNameTooltip.Features;

public sealed class Draw_CharacterHUD
{
    private const int BORDER_WIDTH = 16;
    private string? hoveredCharacterName;
    private IModNameInfo? hoveredModName;
    private Character? hoveredCharacter;
    private Vector2 hoveredCharacterNamePos = Vector2.Zero;
    private Vector2 hoveredModNamePos = Vector2.Zero;
    private Vector2 hoveredSize = Vector2.Zero;

    private readonly int screenId;

    public Draw_CharacterHUD(int screenId)
    {
        this.screenId = screenId;
        Toggle();
    }

    public void Toggle()
    {
        if (screenId != Context.ScreenId)
            return;
        if (ModEntry.config.Enable_HUD)
        {
            ModEntry.help.Events.Display.RenderedHud += OnRenderedHud;
            ModEntry.help.Events.Input.CursorMoved += OnCursorMoved;
        }
        else
        {
            ModEntry.help.Events.Display.RenderedHud -= OnRenderedHud;
            ModEntry.help.Events.Input.CursorMoved -= OnCursorMoved;
        }
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (
            screenId != Context.ScreenId
            || Game1.currentLocation == null
            || Game1.activeClickableMenu != null
            || Game1.currentLocation.currentEvent != null
        )
        {
            SetHoveredCharacter(null);
            return;
        }
        Rectangle searchBounds = new((int)e.NewPosition.Tile.X * 64, (int)e.NewPosition.Tile.Y * 64, 64, 128);
        foreach (NPC character in Game1.currentLocation.characters)
        {
            if (character.GetBoundingBox().Intersects(searchBounds))
            {
                SetHoveredCharacter(character);
                return;
            }
        }
        foreach (FarmAnimal farmAnimal in Game1.currentLocation.animals.Values)
        {
            if (farmAnimal.GetCursorPetBoundingBox().Contains(e.NewPosition.AbsolutePixels))
            {
                SetHoveredCharacter(farmAnimal);
                return;
            }
        }
        SetHoveredCharacter(null);
    }

    private void SetHoveredCharacter(Character? character)
    {
        if (character == hoveredCharacter)
            return;
        hoveredCharacter = character;
        if (hoveredCharacter == null)
        {
            hoveredCharacterName = null;
            hoveredModName = null;
            hoveredSize = Vector2.Zero;
            hoveredCharacterNamePos = Vector2.Zero;
            hoveredModNamePos = Vector2.Zero;
            return;
        }
        if (ModEntry.modNameAPI.TryGetModName(hoveredCharacter, out IModNameInfo? modName))
        {
            if (character is FarmAnimal farmAnimal)
            {
                hoveredCharacterName = I18n.Hud_FarmAnimal(
                    string.IsNullOrEmpty(hoveredCharacter.displayName)
                        ? hoveredCharacter.Name
                        : hoveredCharacter.displayName,
                    farmAnimal.displayType
                );
            }
            else
            {
                hoveredCharacterName = string.IsNullOrEmpty(hoveredCharacter.displayName)
                    ? hoveredCharacter.Name
                    : hoveredCharacter.displayName;
            }
            hoveredModName = modName;
            Vector2 hoveredCharacterNameSize = Game1.smallFont.MeasureString(hoveredCharacterName);
            Vector2 hoveredModNameSize = Game1.smallFont.MeasureString(hoveredModName.ModName);
            hoveredSize = new(
                MathF.Ceiling(MathF.Max(hoveredCharacterNameSize.X, hoveredModNameSize.X)) + BORDER_WIDTH * 2,
                MathF.Ceiling(hoveredCharacterNameSize.Y + hoveredModNameSize.Y) + BORDER_WIDTH * 2 - 4
            );
            if (hoveredCharacterNameSize.X < hoveredModNameSize.X)
            {
                hoveredCharacterNamePos = new(
                    BORDER_WIDTH + hoveredModNameSize.X / 2 - hoveredCharacterNameSize.X / 2,
                    BORDER_WIDTH
                );
                hoveredModNamePos = new(BORDER_WIDTH, BORDER_WIDTH + hoveredCharacterNameSize.Y);
            }
            else
            {
                hoveredCharacterNamePos = new(BORDER_WIDTH, BORDER_WIDTH);
                hoveredModNamePos = new(
                    BORDER_WIDTH + hoveredCharacterNameSize.X / 2 - hoveredModNameSize.X / 2,
                    BORDER_WIDTH + hoveredCharacterNameSize.Y
                );
            }
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (
            screenId != Context.ScreenId
            || Game1.activeClickableMenu != null
            || Game1.currentLocation.currentEvent != null
            || hoveredModName == null
            || hoveredCharacterName == null
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
            hoveredCharacterName,
            Game1.smallFont,
            new(x + hoveredCharacterNamePos.X, y + hoveredCharacterNamePos.Y),
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
}
