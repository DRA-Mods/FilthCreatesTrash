using System.Reflection;
using FilthCreatesTrash.GameComp;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FilthCreatesTrash.HarmonyPatches;

[HarmonyPatch("VanillaFurnitureEC.CompBinClean", nameof(ThingComp.PostDestroy))]
public static class OnBinDestroyed
{
    public static bool Prepare(MethodBase method)
    {
        // Only do a mod check for initial pass (is enabled globally)
        if (method == null)
            return FilthCreatesTrashModCore.settings.enableTrashOnBinsCleaning &&
                   ModLister.AllModsActiveNoSuffix(["VanillaExpanded.VFECore"]) &&
                   AccessTools.DeclaredMethod($"VanillaFurnitureEC.CompBinClean:{nameof(ThingComp.PostDestroy)}") != null;
        // Return true if a method is not null (is enabled for specific method)
        return true;
    }

    public static bool Prefix(Map __1, ThingComp __instance, ThingOwner ___innerContainer)
    {
        GameComponent_FilthCleaningTracker.Instance.Notify_FilthCleaned(___innerContainer, __1, __instance.parent.Position);
        ___innerContainer.ClearAndDestroyContents();
        return false;
    }
}