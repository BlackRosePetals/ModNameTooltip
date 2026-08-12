using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Framework.ModHelpers;

namespace ModNameTooltip.Features;

public static class Patch_SMAPIGetFromNamespacedId
{
    private static readonly MethodInfo GetFromNamespacedIdMethod = AccessTools.DeclaredMethod(
        typeof(ModRegistryHelper),
        nameof(ModRegistryHelper.GetFromNamespacedId)
    );
    private static readonly HarmonyMethod GetFromNamespacedIdMethod_Postfix = new(
        typeof(Patch_SMAPIGetFromNamespacedId),
        nameof(ModRegistryHelper_GetFromNamespacedId_Postfix)
    )
    {
        priority = Priority.Last,
    };

    public static void Toggle()
    {
        if (ModEntry.config.Enable_GetFromNamespacedId)
        {
            try
            {
                ModEntry.harmony.Patch(original: GetFromNamespacedIdMethod, postfix: GetFromNamespacedIdMethod_Postfix);
            }
            catch (Exception ex)
            {
                ModEntry.Log($"Failed to Patch_SMAPIGetFromNamespacedId.Toggle \n{ex}", LogLevel.Error);
            }
        }
        else
        {
            ModEntry.harmony.Unpatch(
                original: GetFromNamespacedIdMethod,
                HarmonyPatchType.Postfix,
                harmonyID: ModEntry.ModId
            );
        }
    }

    private static void ModRegistryHelper_GetFromNamespacedId_Postfix(string? namespacedId, ref IModInfo? __result)
    {
        if (__result != null || string.IsNullOrEmpty(namespacedId))
            return;
        foreach (TraceContext ctx in ModEntry.traceCtx)
        {
            if (ctx.TryGetModName(namespacedId, out ModNameInfo? modName))
            {
                __result = modName.ModInfo;
                return;
            }
        }
    }
}
