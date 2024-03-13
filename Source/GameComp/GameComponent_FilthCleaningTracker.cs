using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FilthCreatesTrash.GameComp;

// Could be a map comp, but would require each method (we don't even use),
// like tick and update calls, to be called once per map. It would also be
// more annoying to get the instance for a specific map, etc.
public class GameComponent_FilthCleaningTracker : GameComponent
{
    public static GameComponent_FilthCleaningTracker Instance { get; private set; }

    private Dictionary<int, TrashProgressTracker> mapTrackers = new();

    // ReSharper disable once UnusedParameter.Local
    public GameComponent_FilthCleaningTracker(Game _)
    {
        Instance = this;
    }

    private static void SpawnTrash(Map map, IntVec3 pos, ThingDef def)
    {
        var thing = ThingMaker.MakeThing(def);
        thing.stackCount = 1;
        GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
    }

    public void Notify_FilthCleaned(Thing filth) => Notify_FilthCleaned(filth.Map, filth.Position, filth);

    public void Notify_FilthCleaned(Map map, IntVec3 pos, Thing filth)
    {
        if (!mapTrackers.TryGetValue(map.uniqueID, out var tracker))
            mapTrackers[map.uniqueID] = tracker = new TrashProgressTracker();

        CleanFilthForTracker(map, tracker, pos, filth);
    }

    public void Notify_FilthCleaned(ThingOwner thingOwner, DestroyMode originalDestroyMode = DestroyMode.Vanish)
    {
        var map = ThingOwnerUtility.GetRootMap(thingOwner.Owner);
        var position = ThingOwnerUtility.GetRootPosition(thingOwner.Owner);

        Notify_FilthCleaned(thingOwner, map, position, originalDestroyMode);
    }

    public void Notify_FilthCleaned(ThingOwner thingOwner, Map map, IntVec3 position, DestroyMode originalDestroyMode = DestroyMode.Vanish)
    {
        if (map == null || !position.IsValid)
        {
            Log.Warning("Cannot drop without a dropLoc and with an owner whose map is null.");
            thingOwner.ClearAndDestroyContents(originalDestroyMode);
            return;
        }

        if (!thingOwner.Any)
            return;

        if (!mapTrackers.TryGetValue(map.uniqueID, out var tracker))
            mapTrackers[map.uniqueID] = tracker = new TrashProgressTracker();

        foreach (var thing in thingOwner)
        {
            if (thing is Filth filth)
            {
                CleanFilthForTracker(map, tracker, position, filth);
            }
            else
            {
                Log.WarningOnce($"Inner container had a non-filth Thing, dropping it: {thing}",
                    Gen.HashCombineInt(thing?.def.GetHashCode() ?? 1964025854, -1209628188));
                GenDrop.TryDropSpawn(thing, position, map, ThingPlaceMode.Near, out _);
            }
        }
    }

    protected static void CleanFilthForTracker(Map map, TrashProgressTracker tracker, IntVec3 pos, Thing filth)
    {
        var trash = FilthCreatesTrashModCore.settings.GetTrashForFilth(filth);
        var counter = tracker.progress.GetValueOrDefault(trash, 0) + (filth as Filth)?.thickness ?? 1;

        FilthCreatesTrashModCore.settings.ValidateFilthBeforeTrashCreated();
        while (counter >= FilthCreatesTrashModCore.settings.filthBeforeTrashCreated)
        {
            counter -= FilthCreatesTrashModCore.settings.filthBeforeTrashCreated;
            SpawnTrash(map, pos, trash);
        }

        if (counter > 0)
            tracker[trash] = counter;
        else
            tracker.progress.Remove(trash);
    }

    public override void ExposeData()
    {
        base.ExposeData();

        // Remove all maps that aren't active
        if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.PostLoadInit)
            mapTrackers.RemoveAll(x => Find.Maps.All(m => m.uniqueID != x.Key));

        Scribe_Collections.Look(ref mapTrackers, nameof(mapTrackers), LookMode.Value, LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            foreach (var (_, value) in mapTrackers)
                value.Cleanup();
        }
    }
}