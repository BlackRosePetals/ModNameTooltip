using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace ModNameTooltip.Features;

public static class Patch_HoverText
{
    private static readonly MethodInfo drawHoverTextMethod = AccessTools.DeclaredMethod(
        typeof(IClickableMenu),
        nameof(IClickableMenu.drawHoverText),
        [
            // 0
            typeof(SpriteBatch),
            typeof(StringBuilder),
            typeof(SpriteFont),
            typeof(int),
            typeof(int),
            // 5
            typeof(int),
            typeof(string),
            typeof(int),
            typeof(string[]),
            typeof(Item),
            // 10
            typeof(int),
            typeof(string),
            typeof(int),
            typeof(int),
            typeof(int),
            // 15
            typeof(float),
            typeof(CraftingRecipe),
            typeof(IList<Item>),
            typeof(Texture2D),
            typeof(Rectangle?),
            // 20
            typeof(Color?),
            typeof(Color?),
            typeof(float),
            typeof(int),
            typeof(int),
        ]
    );
    private static readonly HarmonyMethod drawHoverTextTranspiler = new(
        typeof(Patch_HoverText),
        nameof(IClickableMenu_drawHoverText_Transpiler)
    )
    {
        priority = Priority.Last,
    };

    public static void Toggle()
    {
        if (ModEntry.config.Enable_Tooltip)
        {
            try
            {
                ModEntry.harmony.Patch(original: drawHoverTextMethod, transpiler: drawHoverTextTranspiler);
            }
            catch (Exception ex)
            {
                ModEntry.Log($"Failed to Patch_HoverText.Toggle \n{ex}", LogLevel.Error);
            }
        }
        else
        {
            ModEntry.harmony.Unpatch(
                original: drawHoverTextMethod,
                HarmonyPatchType.Transpiler,
                harmonyID: ModEntry.ModId
            );
        }
    }

    private static IEnumerable<CodeInstruction> IClickableMenu_drawHoverText_Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        try
        {
            CodeMatcher matcher = new(instructions, generator);
            // text5 = hoveredItem.getCategoryName();
            // if (text5.Length > 0)
            // {
            //     num = Math.Max(num, (int)font.MeasureString(text5).X + 32);
            //     num2 += (int)font.MeasureString("T").Y;
            // }
            //
            // IL_0354: ldarg.s hoveredItem
            // IL_0356: callvirt instance string StardewValley.Item::getCategoryName()
            // IL_035b: stloc.3
            // IL_035c: ldloc.3
            // IL_035d: callvirt instance int32 [System.Runtime]System.String::get_Length()
            // IL_0362: ldc.i4.0
            // IL_0363: ble.s IL_0390
            //
            // IL_0365: ldloc.1
            // IL_0366: ldarg.2
            // IL_0367: ldloc.3
            // IL_0368: callvirt instance valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Vector2 [MonoGame.Framework]Microsoft.Xna.Framework.Graphics.SpriteFont::MeasureString(string)
            // IL_036d: ldfld float32 [MonoGame.Framework]Microsoft.Xna.Framework.Vector2::X
            // IL_0372: conv.i4
            // IL_0373: ldc.i4.s 32
            // IL_0375: add
            // IL_0376: call int32 [System.Runtime]System.Math::Max(int32, int32)
            // IL_037b: stloc.1
            // IL_037c: ldloc.2
            // IL_037d: ldarg.2
            // IL_037e: ldstr "T"
            // IL_0383: callvirt instance valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Vector2 [MonoGame.Framework]Microsoft.Xna.Framework.Graphics.SpriteFont::MeasureString(string)
            // IL_0388: ldfld float32 [MonoGame.Framework]Microsoft.Xna.Framework.Vector2::Y
            // IL_038d: conv.i4
            // IL_038e: add
            // IL_038f: stloc.2

            LocalBuilder modNameLoc = generator.DeclareLocal(typeof(ModNameInfo));
            LocalBuilder modNameSize = generator.DeclareLocal(typeof(Vector2));
            MethodInfo measureString = AccessTools.Method(
                typeof(SpriteFont),
                nameof(SpriteFont.MeasureString),
                [typeof(string)]
            );
            FieldInfo vector2X = AccessTools.Field(typeof(Vector2), nameof(Vector2.X));
            FieldInfo vector2Y = AccessTools.Field(typeof(Vector2), nameof(Vector2.Y));

            matcher
                .Start()
                .MatchEndForward([
                    new(OpCodes.Ldarg_S, (byte)9), // hoveredItem
                    new(OpCodes.Callvirt, AccessTools.DeclaredMethod(typeof(Item), nameof(Item.getCategoryName))),
                    new(inst => inst.IsStloc()),
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(string), nameof(string.Length))),
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Ble_S),
                ])
                .ThrowIfNotMatch("Failed to match 'hoveredItem.getCategoryName()' if block");

            // if block
            // width
            matcher
                .MatchEndForward([
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Ldarg_2),
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Callvirt, measureString),
                    new(OpCodes.Ldfld, vector2X),
                    new(OpCodes.Conv_I4),
                    new(OpCodes.Ldc_I4_S),
                    new(OpCodes.Add),
                    new(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Max), [typeof(int), typeof(int)])),
                    new(inst => inst.IsStloc()),
                ])
                .ThrowIfNotMatch("Failed to match 'width local'");
            CodeInstruction ldlocWidth = matcher.InstructionAt(-9);
            CodeInstruction stlocWidth = matcher.InstructionAt(0);

            // height
            matcher
                .MatchEndForward([
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Ldstr, "T"),
                    new(OpCodes.Callvirt, measureString),
                    new(OpCodes.Ldfld, vector2Y),
                    new(OpCodes.Conv_I4),
                    new(OpCodes.Add),
                    new(inst => inst.IsStloc()),
                ])
                .ThrowIfNotMatch("Failed to match 'height local'");
            CodeInstruction ldlocHeight = matcher.InstructionAt(-7);
            CodeInstruction stlocHeight = matcher.InstructionAt(0);

            // IL_03d5: stloc.2
            // IL_03d6: ldarg.s hoveredItem
            // IL_03d8: isinst StardewValley.Tools.MeleeWeapon
            // IL_03dd: stloc.s 19
            // IL_03df: ldloc.s 19
            // IL_03e1: brtrue.s IL_03f5
            matcher
                .MatchStartForward([new(OpCodes.Ldarg_S, (byte)9), new(OpCodes.Isinst, typeof(MeleeWeapon))])
                .ThrowIfNotMatch("Failed to match 'hoveredItem'")
                .InsertAndAdvance([
                    // try and get a mod name
                    new(OpCodes.Ldarg_S, (byte)9),
                    new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Patch_HoverText), nameof(GetModName))),
                    new(OpCodes.Stloc, modNameLoc.LocalIndex),
                    // measure
                    new(OpCodes.Ldloc, modNameLoc.LocalIndex),
                    new(OpCodes.Ldarg_2),
                    new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(ModNameInfo), nameof(ModNameInfo.Measure))),
                    new(OpCodes.Stloc, modNameSize.LocalIndex),
                    // adjust width
                    ldlocWidth.Clone(),
                    new(OpCodes.Ldloc, modNameSize.LocalIndex),
                    new(OpCodes.Ldfld, vector2X),
                    new(OpCodes.Conv_I4),
                    new(OpCodes.Ldc_I4_S, 32),
                    new(OpCodes.Add),
                    new(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Max), [typeof(int), typeof(int)])),
                    stlocWidth.Clone(),
                    // adjust height
                    ldlocHeight.Clone(),
                    new(OpCodes.Ldloc, modNameSize.LocalIndex),
                    new(OpCodes.Ldfld, vector2Y),
                    new(OpCodes.Conv_I4),
                    new(OpCodes.Add),
                    stlocHeight.Clone(),
                ]);

            // x + num > Utility.getSafeArea().Right
            // IL_06a7: ldloc.s 5
            // IL_06a9: ldloc.1
            // IL_06aa: add
            // IL_06ab: call valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Rectangle StardewValley.Utility::getSafeArea()
            // IL_06b0: stloc.s 23
            // IL_06b2: ldloca.s 23
            // IL_06b4: call instance int32 [MonoGame.Framework]Microsoft.Xna.Framework.Rectangle::get_Right()
            // IL_06b9: ble.s IL_06d4
            matcher
                .MatchStartForward([
                    new(inst => inst.IsLdloc()),
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Add),
                    new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Utility), nameof(Utility.getSafeArea))),
                    new(inst => inst.IsStloc()),
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(Rectangle), nameof(Rectangle.Right))),
                    new(OpCodes.Ble_S),
                ])
                .ThrowIfNotMatch("Failed to match 'x + num > Utility.getSafeArea().Right'");
            CodeInstruction ldlocX = matcher.InstructionAt(0);

            // drawTextureBox(b, boxTexture, boxSourceRect.Value, x, num6, num + ((craftingIngredients != null) ? 21 : 0), (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && text5.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, Color.White * alpha, 1f, drawShadow: false);
            // b.Draw(Game1.menuTexture, new Rectangle(x + 12, num6 + (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && text5.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, num - 4 * ((craftingIngredients != null) ? 1 : 6), 4), new Rectangle(44, 300, 4, 4), Color.White);
            // IL_07cf: ldarg.2
            // IL_07d0: ldstr "asd"
            // IL_07d5: callvirt instance valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Vector2 [MonoGame.Framework]Microsoft.Xna.Framework.Graphics.SpriteFont::MeasureString(string)
            // IL_07da: ldfld float32 [MonoGame.Framework]Microsoft.Xna.Framework.Vector2::Y

            // IL_07df: conv.i4
            // IL_07e0: add

            for (int i = 0; i < 2; ++i)
            {
                matcher
                    .MatchEndForward([
                        new(OpCodes.Ldarg_2),
                        new(OpCodes.Ldstr, "asd"),
                        new(OpCodes.Callvirt, measureString),
                        new(OpCodes.Ldfld),
                        new(OpCodes.Conv_I4),
                        new(OpCodes.Add),
                    ])
                    .ThrowIfNotMatch($"Failed to match {i} 'font.MeasureString(\"asd\")'")
                    .InsertAndAdvance([
                        new(OpCodes.Add),
                        new(OpCodes.Ldloc, modNameSize.LocalIndex),
                        new(OpCodes.Ldfld, vector2Y),
                        new(OpCodes.Conv_I4),
                    ]);
            }

            // num6 += ((boldTitleText != null) ? 16 : 0);
            // IL_0c28: ldloc.s 6
            // IL_0c2a: ldarg.s boldTitleText
            // IL_0c2c: brtrue.s IL_0c31

            // IL_0c2e: ldc.i4.0
            // IL_0c2f: br.s IL_0c33

            // IL_0c31: ldc.i4.s 16

            // IL_0c33: add
            // IL_0c34: stloc.s 6

            matcher
                .MatchEndForward([
                    new(inst => inst.IsLdloc()),
                    new(OpCodes.Ldarg_S, (byte)6),
                    new(OpCodes.Brtrue_S),
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Br_S),
                    new(OpCodes.Ldc_I4_S),
                    new(OpCodes.Add),
                    new(inst => inst.IsStloc()),
                    new(OpCodes.Ldarg_S, (byte)9),
                ])
                .ThrowIfNotMatch("Failed to match 'num6 += ((boldTitleText != null) ? 16 : 0)");
            CodeInstruction ldlocY = matcher.InstructionAt(-8);
            CodeInstruction stlocY = matcher.InstructionAt(-1);
            matcher.Opcode = OpCodes.Ldloc;
            matcher.Operand = modNameLoc.LocalIndex;
            matcher
                .Advance(1)
                .InsertAndAdvance([
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_2),
                    ldlocX.Clone(),
                    ldlocY.Clone(),
                    new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(ModNameInfo), nameof(ModNameInfo.Draw))),
                    ldlocY.Clone(),
                    new(OpCodes.Ldloc, modNameSize.LocalIndex),
                    new(OpCodes.Ldfld, vector2Y),
                    new(OpCodes.Conv_I4),
                    new(OpCodes.Add),
                    stlocY.Clone(),
                    new(OpCodes.Ldarg_S, (byte)9),
                ]);

            return matcher.Instructions();
        }
        catch (Exception ex)
        {
            ModEntry.Log($"Failed in IClickableMenu_drawHoverText_Transpiler\n{ex}", LogLevel.Error);
            return instructions;
        }
    }

    private static ModNameInfo GetModName(Item hoveredItem)
    {
        if (
            hoveredItem != null
            && ModEntry.itemTypeToTraceCtx.TryGetValue(hoveredItem.GetItemTypeId(), out TraceContext? ctx)
            && ctx.TryGetModName(hoveredItem.ItemId, out ModNameInfo? modName)
        )
        {
            return modName;
        }
        return ModNameInfo.EMPTY;
    }
}
