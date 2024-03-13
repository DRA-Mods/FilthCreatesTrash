using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FilthCreatesTrash.GameComp;
using HarmonyLib;
using Verse;

namespace FilthCreatesTrash.HarmonyPatches;

[HarmonyPatch]
public static class OnBinCleaned
{
    public static bool Prepare(MethodBase method)
    {
        // Only do a mod check for initial pass (is enabled globally)
        // Also for the future: remove the `_steam` call once this gets fixed
        if (method == null)
            return FilthCreatesTrashModCore.settings.enableTrashOnBinsCleaning && (ModsConfig.IsActive("VanillaExpanded.VFECore") || ModsConfig.IsActive("VanillaExpanded.VFECore_steam"));
        // Return true if a method is not null (is enabled for specific method)
        return true;
    }

    public static MethodBase TargetMethod()
    {
        const string target = "VanillaFurnitureEC.JobDriver_CleanBin";
        var type = AccessTools.TypeByName(target);
        if (type != null)
            return MethodUtil.GetLambda(type, "MakeNewToils", lambdaOrdinal: 1);

        Log.Error($"Could not find target type, patch will fail: '{target}'");
        return null;
    }

    private static void InterceptedClearAndDestroy(ThingOwner instance, DestroyMode destroyMode)
        => GameComponent_FilthCleaningTracker.Instance.Notify_FilthCleaned(instance, destroyMode);

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var target = AccessTools.DeclaredMethod(typeof(ThingOwner), nameof(ThingOwner.ClearAndDestroyContents));
        var replacement = MethodUtil.MethodOf(InterceptedClearAndDestroy);
        var patchCount = 0;

        foreach (var ci in instr)
        {
            if (ci.Calls(target))
            {
                ci.opcode = OpCodes.Call;
                ci.operand = replacement;

                patchCount++;
            }

            yield return ci;
        }

        const int expectedPatches = 1;
        if (patchCount != expectedPatches)
        {
            var name = (baseMethod.DeclaringType?.Namespace).NullOrEmpty() ? baseMethod.Name : $"{baseMethod.DeclaringType!.Name}:{baseMethod.Name}";
            Log.Error($"[{FilthCreatesTrashModCore.ModName}] - patched incorrect number of calls to ThingOwner.ClearAndDestroyContents (expected: {expectedPatches}, patched: {patchCount}) for method {name}");
        }
    }
}