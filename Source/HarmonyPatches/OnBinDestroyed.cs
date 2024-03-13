using System.Reflection;
using FilthCreatesTrash.GameComp;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FilthCreatesTrash.HarmonyPatches;

[HarmonyPatch("CompBinClean", nameof(ThingComp.PostDestroy))]
public static class OnBinDestroyed
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

    public static bool Prefix(Map __1, ThingComp __instance, ThingOwner ___innerContainer)
    {
        GameComponent_FilthCleaningTracker.Instance.Notify_FilthCleaned(___innerContainer, __1, __instance.parent.Position);
        return false;
    }
}