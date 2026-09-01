using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace ModNameTooltip.Features;

public static class Patch_BetterCrafting
{
    private static bool hasBCBuildings = false;

    public static void Apply()
    {
        if (!ModEntry.help.ModRegistry.IsLoaded("leclair.bettercrafting"))
        {
            return;
        }
        hasBCBuildings = ModEntry.help.ModRegistry.IsLoaded("leclair.bcbuildings");
        if (
            AccessTools.DeclaredMethod("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:GetRecipeTooltip")
            is MethodInfo bcGetRecipeTooltip
        )
        {
            ModEntry.Log($"Patching Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:GetRecipeTooltip");
            try
            {
                ModEntry.harmony.Patch(
                    original: bcGetRecipeTooltip,
                    transpiler: new HarmonyMethod(
                        typeof(Patch_BetterCrafting),
                        nameof(BetterCraftingPage_GetRecipeTooltip_Postfix)
                    )
                );
            }
            catch (Exception ex)
            {
                ModEntry.Log(
                    $"Failed to patch Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:GetRecipeTooltip\n{ex}",
                    LogLevel.Warn
                );
            }
        }
        // AccessTools.DeclaredMethod("Leclair.Stardew.BetterCrafting.DynamicRules.SourceModRuleHandler:GetOptions") is not MethodInfo bcSourceModGetOptions
    }

    private static IEnumerable<CodeInstruction> BetterCraftingPage_GetRecipeTooltip_Postfix(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        try
        {
            CodeMatcher matcher = new(instructions, generator);

            // IL_177d: callvirt instance bool Leclair.Stardew.BetterCrafting.ModConfig::get_ShowSourceModInTooltip()
            // IL_1782: brfalse IL_1916
            // IL_1787: ldnull
            // IL_1788: stloc.s 66
            // IL_178a: ldarg.0
            // IL_178b: ldfld class Leclair.Stardew.Common.Crafting.IRecipe Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage::hoverRecipe
            // IL_1790: callvirt instance string Leclair.Stardew.Common.Crafting.IRecipe::get_Name()
            // IL_1795: stloc.s 67
            // IL_1797: ldloc.s 67

            matcher
                .MatchEndForward([
                    new(
                        OpCodes.Callvirt,
                        AccessTools.PropertyGetter("Leclair.Stardew.BetterCrafting.ModConfig:ShowSourceModInTooltip")
                    ),
                    new(OpCodes.Brfalse),
                    new(OpCodes.Ldnull),
                    new(inst => inst.IsStloc()),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld),
                    new(OpCodes.Callvirt, AccessTools.PropertyGetter("Leclair.Stardew.Common.Crafting.IRecipe:Name")),
                    new(inst => inst.IsStloc()),
                    new(inst => inst.IsLdloc()),
                ])
                .ThrowIfNotMatch("Mod.Config.ShowSourceModInTooltip");

            CodeInstruction stlocModInfo = matcher.InstructionAt(-5).Clone();
            CodeInstruction ldlocRecipeName = matcher.InstructionAt(0).Clone();

            // if (info == null && recipeItem != null)
            matcher
                .MatchStartForward([
                    new(OpCodes.Ldloc_S, stlocModInfo.operand),
                    new(OpCodes.Brtrue_S),
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Brfalse_S),
                ])
                .ThrowIfNotMatch("if (info == null && recipeItem != null)");

            CodeInstruction ldlocRecipeItem = matcher.InstructionAt(2).Clone();

            CodeInstruction previousHead = matcher.Instruction.Clone();
            matcher
                .Advance(1)
                .Insert([
                    ldlocRecipeName,
                    new(OpCodes.Ldarg_0),
                    new(
                        OpCodes.Callvirt,
                        AccessTools.PropertyGetter("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:Cooking")
                    ),
                    ldlocRecipeItem,
                    new(
                        OpCodes.Call,
                        AccessTools.DeclaredMethod(
                            typeof(Patch_BetterCrafting),
                            nameof(Patch_BetterCrafting_GetModInfo)
                        )
                    ),
                    stlocModInfo,
                    previousHead,
                ]);

            return matcher.Instructions();
        }
        catch (Exception ex)
        {
            ModEntry.Log($"Failed in BetterCraftingPage_GetRecipeTooltip_Postfix\n{ex}", LogLevel.Warn);
            return instructions;
        }
    }

    private static IModInfo? Patch_BetterCrafting_GetModInfo(
        IModInfo? info,
        string name,
        bool isCooking,
        Item? recipeItem
    )
    {
        if (info != null)
            return info;
        if (isCooking && ModEntry.modNameAPI.TryGetModName_CookingRecipe(name, out IModNameInfo? modName))
        {
            return modName.ModInfo;
        }
        else if (ModEntry.modNameAPI.TryGetModName_CraftingRecipe(name, out modName))
        {
            return modName.ModInfo;
        }
        else if (ModEntry.modNameAPI.TryGetModName(recipeItem, out modName))
        {
            return modName.ModInfo;
        }
        else if (hasBCBuildings && ModEntry.modNameAPI.TryGetModName_FromBuildingId(name, out modName))
        {
            return modName.ModInfo;
        }
        return info;
    }
}
