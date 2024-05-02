using System.Collections.Generic;
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
        => method != null || FilthCreatesTrashModCore.settings.enableTrashOnRainCleaning || FilthCreatesTrashModCore.settings.enableTrashOnPawnCleaning;

    private static IEnumerable<MethodBase> TargetMethods()
    {
        if (FilthCreatesTrashModCore.settings.enableTrashOnRainCleaning)
        {
            var method = AccessTools.DeclaredMethod(typeof(SteadyEnvironmentEffects), nameof(SteadyEnvironmentEffects.DoCellSteadyEffects));
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

            // For the future: remove the `_steam` call once this gets fixed
            if (ModsConfig.IsActive("avilmask.CommonSense") || ModsConfig.IsActive("avilmask.CommonSense_steam"))
            {
                var type = AccessTools.TypeByName("CommonSense.JobDriver_DoBill_MakeNewToils_CommonSensePatch");
                method = MethodUtil.GetLambda(type, "DoMakeToils", lambdaOrdinal: 22);

                if (method.IsSuccess)
                    yield return method;
                else
                    Log.Error($"[{FilthCreatesTrashModCore.ModName}] - (Common Sense compat) trash generation on cleaning before working will not work. {method.error}");

                type = AccessTools.TypeByName("CommonSense.JobDriver_PrepareToIngestToils_ToolUser_CommonSensePatch");
                method = MethodUtil.GetLambda(type, "MakeCleanToil", lambdaOrdinal: 1);

                if (method.IsSuccess)
                    yield return method;
                else
                    Log.Error($"[{FilthCreatesTrashModCore.ModName}] - (Common Sense compat) trash generation on cleaning before eating will not work. {method.error}");

                type = AccessTools.TypeByName("CommonSense.JobDriver_SocialRelax_MakeNewToils_CommonSensePatch");
                method = MethodUtil.GetLambda(type, "_MakeToils", lambdaOrdinal: 6);

                if (method.IsSuccess)
                    yield return method;
                else
                    Log.Error($"[{FilthCreatesTrashModCore.ModName}] - (Common Sense compat) trash generation on cleaning before relaxing will not work. {method.error}");
            }
        }
    }

    private static Filth RemoveFilthPassthrough(Filth filth)
    {
        if (filth is { Spawned: true })
            GameComponent_FilthCleaningTracker.Instance.Notify_FilthCleaned(filth);
        return filth;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var target = AccessTools.DeclaredMethod(typeof(Filth), nameof(Filth.ThinFilth));
        var passthrough = MethodUtil.MethodOf(RemoveFilthPassthrough);
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