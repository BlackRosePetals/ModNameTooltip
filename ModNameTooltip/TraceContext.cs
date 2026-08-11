using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AssetPipelineTrace;
using Sickhead.Engine.Util;
using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Content;
using StardewValley;

namespace ModNameTooltip;

public sealed record ModNameText(string ModId, string ModName);

public sealed class TraceContext(IAssetName tracedAsset, Func<TraceContext, Item, string?>? specialLookup = null)
{
    internal const string STARDEW_VALLEY = "StardewValley";

    public readonly IAssetName TracedAsset = tracedAsset;
    private readonly Func<TraceContext, Item, string?>? specialLookup = specialLookup;

    private bool active = true;
    private HashSet<string>? tracedKeys = null;
    private readonly List<DataTraceFrame> tracedFrames = [];
    internal static Dictionary<Type, Delegate?> keyGetters = [];
    internal static Dictionary<Type, Delegate?> idGetters = [];

    private readonly Dictionary<string, ModNameText> keyToMod = [];
    public IReadOnlyDictionary<string, ModNameText> KeyToMod => keyToMod;
    public int keyToModLastPopulated = -1;

    public bool TryGetItemModName(Item item, [NotNullWhen(true)] out string? modName)
    {
        modName = specialLookup?.Invoke(this, item);
        if (modName != null)
        {
            return true;
        }
        if (KeyToMod.TryGetValue(item.ItemId, out ModNameText? text))
        {
            modName = text.ModName;
            return true;
        }
        return false;
    }

    public void PopulateKeyToMod(IAssetName assetName)
    {
        if (!TracedAsset.IsEquivalentTo(assetName))
            return;
        if (keyToModLastPopulated == Game1.ticks)
            return;

        keyToMod.Clear();
        foreach (DataTraceFrame frame in tracedFrames)
        {
            string modId = frame.ModId;
            ModNameText modNameText;
            if (ModEntry.help.ModRegistry.Get(modId) is IModInfo modInfo)
            {
                modNameText = new(modId, modInfo.Manifest.Name);
            }
            else
            {
                modNameText = new(modId, modId == STARDEW_VALLEY ? I18n.Game_Title() : modId);
            }
            foreach (string key in frame.AddedKeys)
            {
                keyToMod[key] = modNameText;
            }
        }

        tracedKeys = null;
        tracedFrames.Clear();

        keyToModLastPopulated = Game1.ticks;
    }

    public void HandleEdit(
        IAssetInfo asset,
        IModMetadata? mod,
        List<AssetLoadOperation> loadOperations,
        ref Action<IAssetData> apply,
        string? onBehalfOf = null
    )
    {
        if (!ShouldTrace(asset))
            return;
        HandleEdit_TraceKindData(asset, mod, loadOperations, ref apply, onBehalfOf);
    }

    private bool ShouldTrace(IAssetInfo asset)
    {
        if (!active)
            return false;
        if (!TracedAsset.IsEquivalentTo(asset.Name))
            return false;
        if (!asset.DataType.IsGenericType)
            return false;
        Type genericDef = asset.DataType.GetGenericTypeDefinition();
        Type[] genericArgs = asset.DataType.GetGenericArguments();
        if (genericDef == typeof(List<>) || genericDef == typeof(Dictionary<,>) && genericArgs[0] == typeof(string))
        {
            return true;
        }
        return false;
    }

    private void HandleEdit_TraceKindData(
        IAssetInfo asset,
        IModMetadata? mod,
        List<AssetLoadOperation> loadOperations,
        ref Action<IAssetData> apply,
        string? onBehalfOf
    )
    {
        if (!active)
            return;
        Delegate? hashGetter = GetOrCreateKeyGetter(asset.DataType);
        if (hashGetter == null)
            return;
        Action<IAssetData> originalApply = apply;
        apply = asset =>
        {
            if (!active)
            {
                // original
                originalApply(asset);
                // original
                return;
            }

            if (tracedKeys == null)
            {
                tracedKeys = (HashSet<string>?)hashGetter.DynamicInvoke(asset);
                if (tracedKeys == null)
                {
                    ModEntry.Log("active -> false 1");
                    active = false;
                    // original
                    originalApply(asset);
                    // original
                    return;
                }
                AssetLoadOperation? loader = loadOperations.MaxBy(p => p.Priority);
                tracedFrames.Add(
                    new DataTraceFrame(
                        loader != null ? mod?.Manifest.UniqueID : STARDEW_VALLEY,
                        loader != null ? onBehalfOf : STARDEW_VALLEY,
                        tracedKeys.ToHashSet()
                    )
                );
            }

            // original
            originalApply(asset);
            // original

            HashSet<string>? tracedKeysAfter = (HashSet<string>?)hashGetter.DynamicInvoke(asset);
            if (tracedKeysAfter == null)
            {
                ModEntry.Log("active -> false 2");
                active = false;
                return;
            }

            HashSet<string> added = tracedKeysAfter.Except(tracedKeys).ToHashSet();
            tracedKeys = tracedKeysAfter;

            if (mod?.Manifest.UniqueID != ModEntry.ModId)
            {
                tracedFrames.Add(new DataTraceFrame(mod?.Manifest.UniqueID, onBehalfOf, added));
            }
        };
    }

    private static Delegate? GetOrCreateKeyGetter(Type typ)
    {
        if (!keyGetters.TryGetValue(typ, out Delegate? methodInfo))
        {
            methodInfo = CreateKeyGetter(typ);
            keyGetters[typ] = methodInfo;
        }
        return methodInfo;
    }

    private static Delegate? CreateKeyGetter(Type typ)
    {
        Type genericDef = typ.GetGenericTypeDefinition();
        Type[] genericArgs = typ.GetGenericArguments();
        if (genericDef == typeof(Dictionary<,>) && genericArgs[0] == typeof(string))
        {
            return CheckStringDictInfo
                ?.MakeGenericMethod(genericArgs[1])
                .CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(IAssetData), typeof(HashSet<string>)));
        }
        else if (genericDef == typeof(List<>))
        {
            return CheckIdListInfo
                ?.MakeGenericMethod(genericArgs[0])
                .CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(IAssetData), typeof(HashSet<string>)));
        }
        return null;
    }

    private static readonly MethodInfo? CheckStringDictInfo = typeof(TraceContext).GetMethod(
        nameof(CheckStringDict),
        BindingFlags.Static | BindingFlags.NonPublic
    );

    private static HashSet<string> CheckStringDict<TValue>(IAssetData asset)
    {
        IDictionary<string, TValue> data = asset.AsDictionary<string, TValue>().Data;
        return data.Keys.ToHashSet();
    }

    private static readonly MethodInfo? CheckIdListInfo = typeof(TraceContext).GetMethod(
        nameof(CheckIdList),
        BindingFlags.Static | BindingFlags.NonPublic
    );

    private static HashSet<string> CheckIdList<TValue>(IAssetData asset)
    {
        Delegate? getId = GetIdGetter(typeof(TValue));
        if (getId == null)
            return [];

        IList<TValue> data = asset.GetData<IList<TValue>>();
        HashSet<string> result = [];
        foreach (TValue item in data)
        {
            result.Add((string)getId.DynamicInvoke(item)!);
        }
        return result;
    }

    private static Delegate? GetIdGetter(Type typ)
    {
        if (!idGetters.TryGetValue(typ, out Delegate? idGetter))
        {
            idGetter = MakeIdGetter(typ);
            idGetters[typ] = idGetter;
        }
        return idGetter;
    }

    private static Delegate? MakeIdGetter(Type typ)
    {
        if (
            (typ.GetProperty("Id") ?? typ.GetProperty("ID")) is PropertyInfo propInfo
            && propInfo.GetDataType() == typeof(string)
        )
        {
            return propInfo.GetGetMethod()?.CreateDelegate(typeof(Func<,>).MakeGenericType(typ, typeof(string)));
        }
        else if (
            (typ.GetField("Id") ?? typ.GetField("ID")) is FieldInfo fieldInfo
            && fieldInfo.GetDataType() == typeof(string)
        )
        {
            return (object thing) => (string)fieldInfo.GetValue(thing)!;
        }
        return null;
    }
}
