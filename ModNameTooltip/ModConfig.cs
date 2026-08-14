using Microsoft.Xna.Framework;
using ModNameTooltip.Integration;
using StardewModdingAPI;
using StardewValley;

namespace ModNameTooltip;

public sealed class ModConfig
{
    public bool Enable_Tooltip { get; set; } = true;
    public bool Enable_HUD { get; set; } = true;
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
                    Color_Modded_Parsed = null;
                else
                    Color_Modded_Parsed = Utility.StringToColor(field);
            }
        }
    } = null;

    internal Color? Color_SDV_Parsed = null;
    internal Color? Color_Modded_Parsed = null;

    public void Register(IManifest mod, IGenericModConfigMenuApi? gmcm)
    {
        if (gmcm == null)
            return;
        gmcm.Register(mod, Reset, Save);
        gmcm.AddBoolOption(
            mod,
            () => Enable_Tooltip,
            (value) => Enable_Tooltip = value,
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
