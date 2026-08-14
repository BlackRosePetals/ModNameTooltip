using Microsoft.Xna.Framework;
using ModNameTooltip.Features;
using ModNameTooltip.Integration;
using StardewModdingAPI;
using StardewValley;

namespace ModNameTooltip;

public sealed class ModConfig
{
    public bool Enable_Tooltip { get; set; } = true;
    public bool Enable_HUD { get; set; } = true;
    public bool Enable_HUD_NPC { get; set; } = true;
    public bool Enable_HUD_FarmAnimal { get; set; } = true;
    public bool Enable_HUD_Object { get; set; } = true;
    public bool Enable_HUD_TerrainFeature { get; set; } = true;
    public bool Enable_HUD_Building { get; set; } = true;
    public string? Color_SDV
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                if (string.IsNullOrEmpty(field))
                    Color_SDV_Parsed = null;
                else
                    Color_SDV_Parsed = Utility.StringToColor(field);
            }
        }
    } = null;
    public string? Color_Mod
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                if (string.IsNullOrEmpty(field))
                    Color_Mod_Parsed = null;
                else
                    Color_Mod_Parsed = Utility.StringToColor(field);
            }
        }
    } = null;

    internal Color? Color_SDV_Parsed = null;
    internal Color? Color_Mod_Parsed = null;

    public void Register(IManifest mod, IGenericModConfigMenuApi? gmcm)
    {
        if (gmcm == null)
            return;
        gmcm.Register(mod, Reset, Save);
        gmcm.AddBoolOption(
            mod,
            () => Enable_Tooltip,
            (value) =>
            {
                if (Enable_Tooltip != value)
                {
                    Enable_Tooltip = value;
                    Patch_HoverText.Toggle();
                }
            },
            I18n.Config_EnableTooltip_Name,
            I18n.Config_EnableTooltip_Desc
        );
        gmcm.AddBoolOption(
            mod,
            () => Enable_HUD,
            (value) => Enable_HUD = value,
            I18n.Config_EnableHUD_Name,
            I18n.Config_EnableHUD_Desc
        );
        gmcm.AddBoolOption(mod, () => Enable_HUD_NPC, (value) => Enable_HUD_NPC = value, I18n.Config_EnableHUDNPC_Name);
        gmcm.AddBoolOption(
            mod,
            () => Enable_HUD_FarmAnimal,
            (value) => Enable_HUD_FarmAnimal = value,
            I18n.Config_EnableHUDFarmAnimal_Name
        );
        gmcm.AddBoolOption(
            mod,
            () => Enable_HUD_Object,
            (value) => Enable_HUD_Object = value,
            I18n.Config_EnableHUDObject_Name
        );
        gmcm.AddBoolOption(
            mod,
            () => Enable_HUD_TerrainFeature,
            (value) => Enable_HUD_TerrainFeature = value,
            I18n.Config_EnableHUDTerrainFeature_Name
        );
        gmcm.AddBoolOption(
            mod,
            () => Enable_HUD_Building,
            (value) => Enable_HUD_Building = value,
            I18n.Config_EnableHUDTerrainFeature_Name
        );
        gmcm.AddTextOption(
            mod,
            () => Color_SDV ?? string.Empty,
            (value) => Color_SDV = value,
            I18n.Config_ColorSDV_Name,
            I18n.Config_ColorSDV_Desc
        );
        gmcm.AddTextOption(
            mod,
            () => Color_Mod ?? string.Empty,
            (value) => Color_Mod = value,
            I18n.Config_ColorMod_Name,
            I18n.Config_ColorMod_Desc
        );
    }

    private void Reset()
    {
        ModConfig defaultConfig = new();
        Enable_Tooltip = defaultConfig.Enable_Tooltip;
        Enable_HUD = defaultConfig.Enable_HUD;
        Color_SDV = defaultConfig.Color_SDV;
        Color_Mod = defaultConfig.Color_Mod;
    }

    private void Save()
    {
        ModEntry.help.WriteConfig(this);
    }
}
