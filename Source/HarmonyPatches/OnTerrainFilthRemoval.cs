using System.Collections.Generic;
using System.Reflection;
using FilthCreatesTrash.GameComp;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FilthCreatesTrash.HarmonyPatches;

[HarmonyPatch]
public static class OnTerrainFilthRemoval
{
    public static bool Prepare(MethodBase method)
        => method != null || FilthCreatesTrashModCore.settings.enableTrashOnSelfCleaning;

    public static IEnumerable<MethodBase> TargetMethods()
    {
        IEnumerable<(string, string)> targets = new[]{ (compat: "Vanilla Expanded Framework", method: "VFECore.TerrainComp_SelfClean:FinishClean") };
        if (ModsConfig.IsActive("BiomesTeam.BiomesCore") || ModsConfig.IsActive("BiomesTeam.BiomesCore_steam"))
            targets = targets.Concat(("Biomes! Core", "BiomesCore.TerrainComp_SelfClean:FinishClean"));

        foreach (var (compatName, methodName) in targets)
        {
            var method = AccessTools.DeclaredMethod(methodName);
            if (method != null)
                yield return method;
            else
                Log.Error($"[{FilthCreatesTrashModCore.ModName}] - ({compatName} compat) trash generation for self-cleaning floors will not work, could not find {methodName}");
        }
    }

    public static void Prefix(Filth ___currentFilth)
    {
        if (___currentFilth is { Spawned: true })
            GameComponent_FilthCleaningTracker.Instance.Notify_FilthCleaned(___currentFilth);
    }
}