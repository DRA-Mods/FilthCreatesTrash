using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FilthCreatesTrash;

public class TrashProgressTracker : IExposable
{
    public Dictionary<ThingDef, int> progress = new();

    public int this[ThingDef def]
    {
        get => progress[def];
        set => progress[def] = value;
    }

    public void ExposeData() => Scribe_Collections.Look(ref progress, nameof(progress), LookMode.Def, LookMode.Value);

    public void Cleanup()
    {
        // Cleanup the progress from values 0 or less and from defs that
        // don't have a chance to be spawned from special conditions.
        progress.RemoveAll(kvp =>
        {
            // If 0 or negative, remove
            if (kvp.Value <= 0)
                return true;
    
            // If the normal trash, always keep
            var def = kvp.Key;
            if (def == ModDefOfs.VRecyclingE_Trash)
                return false;
            // Check if any of the 2 collections contain the def and return false, or true if none of them have this def.
            var settings = FilthCreatesTrashModCore.settings;
            return settings.rareTrashTypes.All(x => x.Key != def) && settings.guaranteedFilthToTrash.All(x => x.Value != def);
        });
    }
}