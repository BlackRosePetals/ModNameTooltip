using System.Diagnostics;
using HarmonyLib;
using ModNameTooltip.Features;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ModNameTooltip;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.ModNameTooltip";
    private static IMonitor mon = null!;
    internal static IModHelper help = null!;
    internal static ModConfig config = null!;

    public const string ModNameString = $"{ModId}/ModName";

    internal static readonly Harmony harmony = new(ModId);
    internal static readonly List<TraceContext> traceCtx = [];
    internal static readonly Dictionary<string, TraceContext> itemTypeToTraceCtx = [];
    internal static TraceContext npcTraceCtx = null!;
    internal static TraceContext farmAnimalTraceCtx = null!;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        help = helper;
        config = helper.ReadConfig<ModConfig>();

        try
        {
            harmony.Patch(
                original: AccessTools.DeclaredMethod(
                    typeof(AssetRequestedEventArgs),
                    nameof(AssetRequestedEventArgs.Edit)
                ),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(AssetRequestedEventArgs_Edit_Prefix))
                {
                    priority = Priority.First,
                }
            );
        }
        catch (Exception ex)
        {
            Log($"Failed to patch AssetRequestedEventArgs.Edit\n{ex}", LogLevel.Error);
            return;
        }

        help.Events.Content.AssetReady += OnAssetReady;
        help.Events.Content.AssetRequested += OnAssetRequested;

        AddItemTraceCtx("(O)", "Data/Objects");
        AddItemTraceCtx("(BC)", "Data/BigCraftables");
        AddItemTraceCtx("(F)", "Data/Furniture");
        AddItemTraceCtx("(W)", "Data/Weapons");
        AddItemTraceCtx("(B)", "Data/Boots");
        AddItemTraceCtx("(H)", "Data/hats");
        AddItemTraceCtx("(M)", "Data/Mannequins");
        AddItemTraceCtx("(P)", "Data/Pants");
        AddItemTraceCtx("(S)", "Data/Shirts");
        AddItemTraceCtx("(T)", "Data/Tools");
        AddItemTraceCtx("(TR)", "Data/Trinkets");
        // special handling for walls and floors
        AddTraceCtxForWallsAndFloors();

        AddCharacterTraceCtx();

        help.ConsoleCommands.Add("mnt-print", "Print currently found keys", ConsoleDebugPrint);

        Patch_HoverText.Toggle();
        Patch_SMAPIGetFromNamespacedId.Toggle();
    }

    public override object? GetApi()
    {
        return new ModNameAPI();
    }

    private void ConsoleDebugPrint(string arg1, string[] arg2)
    {
        foreach (TraceContext ctx in traceCtx)
        {
            Log(ctx.TracedAsset.Name, LogLevel.Info);
            foreach ((string key, ModNameInfo modNameText) in ctx.KeyToMod)
            {
                Log($"- {key} : {modNameText}", LogLevel.Info);
            }
        }
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(ModNameString))
        {
            string stringsAsset = Path.Combine("i18n", e.Name.LanguageCode.ToString() ?? "default", "mod-names.json");
            if (File.Exists(Path.Combine(help.DirectoryPath, stringsAsset)))
                e.LoadFromModFile<Dictionary<string, string>>(stringsAsset, AssetLoadPriority.Exclusive);
            else
                e.LoadFromModFile<Dictionary<string, string>>(
                    "i18n/default/mod-names.json",
                    AssetLoadPriority.Exclusive
                );
            return;
        }
        foreach (TraceContext ctx in traceCtx)
        {
            // do a bogus edit so that our prefix always happens bolb
            if (ctx.TracedAsset.IsEquivalentTo(e.NameWithoutLocale))
                e.Edit(asset => { }, AssetEditPriority.Early);
        }
    }

    private void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        foreach (TraceContext ctx in traceCtx)
        {
            ctx.PopulateKeyToMod(e.NameWithoutLocale);
        }
    }

    private static void AddItemTraceCtx(string itemTypeId, string assetNameStr)
    {
        IAssetName assetName = help.GameContent.ParseAssetName(assetNameStr);
        TraceContext ctx = new(assetName);
        itemTypeToTraceCtx[itemTypeId] = ctx;
        traceCtx.Add(ctx);
    }

    private static void AddTraceCtxForWallsAndFloors()
    {
        IAssetName assetName = help.GameContent.ParseAssetName("Data/AdditionalWallpaperFlooring");
        TraceContext ctx = new(
            assetName,
            static (ctx, itemId) =>
            {
                if (int.TryParse(itemId, out _))
                {
                    return ModNameInfo.STARDEW;
                }
                string[] parts = itemId.Split(':');
                if (parts.Length > 1 && int.TryParse(parts.Last(), out int idx))
                {
                    itemId = string.Join(':', parts.Take(parts.Length - 1));
                }
                if (ctx.KeyToMod.TryGetValue(itemId, out ModNameInfo? text))
                {
                    return text;
                }
                return null;
            }
        );
        itemTypeToTraceCtx["(WP)"] = ctx;
        itemTypeToTraceCtx["(FL)"] = ctx;
        traceCtx.Add(ctx);
    }

    private static void AddCharacterTraceCtx()
    {
        npcTraceCtx = new(help.GameContent.ParseAssetName("Data/Characters"));
        traceCtx.Add(npcTraceCtx);
        farmAnimalTraceCtx = new(help.GameContent.ParseAssetName("Data/FarmAnimals"));
        traceCtx.Add(farmAnimalTraceCtx);
    }

    private static void AssetRequestedEventArgs_Edit_Prefix(
        AssetRequestedEventArgs __instance,
        ref Action<IAssetData> apply,
        string? onBehalfOf
    )
    {
        foreach (TraceContext ctx in traceCtx)
        {
            ctx.HandleEdit(__instance.AssetInfo, __instance.Mod, __instance.LoadOperations, ref apply, onBehalfOf);
        }
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.LogOnce(msg, level);
    }

    /// <summary>SMAPI static monitor Log wrapper, debug only</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.Log(msg, level);
    }
}
