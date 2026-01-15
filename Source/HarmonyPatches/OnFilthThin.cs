using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FilthCreatesTrash.GameComp;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FilthCreatesTrash.HarmonyPatches;

[HarmonyPatch]
public static class OnFilthThin
{
    private static bool Prepare(MethodBase method)
    {
        if (method != null)
            return true;

        if (FilthCreatesTrashModCore.settings.enableTrashOnRainCleaning ||
            FilthCreatesTrashModCore.settings.enableTrashOnPawnCleaning ||
            FilthCreatesTrashModCore.settings.enableTrashOnCarriedFilth ||
            FilthCreatesTrashModCore.settings.enableTrashOnSelfCleaning)
            return TargetMethods().Any();

        return false;
    }

    private static IEnumerable<MethodBase> TargetMethods()
    {
        if (FilthCreatesTrashModCore.settings.enableTrashOnRainCleaning)
        {
            var method = typeof(SteadyEnvironmentEffects).DeclaredMethod(nameof(SteadyEnvironmentEffects.DoCellSteadyEffects));
            if (method != null)
                yield return method;
            else
                Log.Error($"[{FilthCreatesTrashModCore.ModName}] - trash generation on cleaning by rain will not work, could not find {nameof(SteadyEnvironmentEffects)}:{nameof(SteadyEnvironmentEffects.DoCellSteadyEffects)}");
        }

        if (FilthCreatesTrashModCore.settings.enableTrashOnPawnCleaning)
        {
            var method = MethodUtil.GetLambda(typeof(JobDriver_CleanFilth), nameof(JobDriver_CleanFilth.MakeNewToils), lambdaOrdinal: 1);
            if (method.IsSuccess)
                yield return method;
            else
                Log.Error($"[{FilthCreatesTrashModCore.ModName}] - trash generation on cleaning will not work, could not find {nameof(JobDriver_CleanFilth)}+<>c__DisplayClass7_0:<{nameof(JobDriver_CleanFilth.MakeNewToils)}>b__1");

            if (ModLister.AnyModActiveNoSuffix(["avilmask.CommonSense"]))
            {
                var type = AccessTools.TypeByName("CommonSense.Utility");
                method = MethodUtil.GetLambda(type, "CleanFilthToil", lambdaOrdinal: 1);

                if (method.IsSuccess)
                    yield return method;
                else
                    Log.Error($"[{FilthCreatesTrashModCore.ModName}] - (Common Sense compat) trash generation won't work on any Common Sense cleaning interactions. {method.error}");
            }
        }

        if (FilthCreatesTrashModCore.settings.enableTrashOnCarriedFilth)
        {
            var method = typeof(Pawn_FilthTracker).DeclaredMethod(nameof(Pawn_FilthTracker.ThinCarriedFilth));
            if (method != null)
                yield return method;
            else
                Log.Error($"[{FilthCreatesTrashModCore.ModName}] - trash generation on cleaning carried filth will not fork, could not find {nameof(Pawn_FilthTracker)}:{nameof(Pawn_FilthTracker.ThinCarriedFilth)}");
        }

        if (FilthCreatesTrashModCore.settings.enableTrashOnSelfCleaning)
        {
            const string methodName = "VEF.Maps.TerrainComp_SelfClean:FinishClean";
            var method = AccessTools.DeclaredMethod(methodName);
            if (method != null)
                yield return method;
            else
                Log.Error($"[{FilthCreatesTrashModCore.ModName}] - (Vanilla Expanded Framework compat) trash generation for self-cleaning floors will not work, could not find {methodName}");
        }
    }

    private static Filth ThinFilthPassthrough(Filth filth)
    {
        var map = filth.MapHeld;
        if (map != null)
            GameComponent_FilthCleaningTracker.Instance.Notify_FilthCleaned(map, filth.PositionHeld, filth, 1);
        return filth;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var target = AccessTools.DeclaredMethod(typeof(Filth), nameof(Filth.ThinFilth));
        var passthrough = MethodUtil.MethodOf(ThinFilthPassthrough);
        var patchCount = 0;

        foreach (var ci in instr)
        {
            if (ci.Calls(target))
            {
                yield return new CodeInstruction(OpCodes.Call, passthrough);
                patchCount++;
            }

            yield return ci;
        }

        const int expectedPatches = 1;
        if (patchCount != expectedPatches)
            Log.Error($"[{FilthCreatesTrashModCore.ModName}] - patched incorrect number of calls to Filth.ThinFilth (expected: {expectedPatches}, patched: {patchCount}) for method {MethodUtil.GetMemberName(baseMethod)}");
    }
}