using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FilthCreatesTrash;

public class FilthCreatesTrashModSettings : ModSettings
{
    public bool enableTrashOnPawnCleaning = true;
    public bool enableTrashOnRainCleaning = false;
    public bool enableTrashOnBinsCleaning = true;
    public bool enableTrashOnSelfCleaning = false;

    private const int DefaultFilthBeforeTrashCreated = 20;
    public int filthBeforeTrashCreated = DefaultFilthBeforeTrashCreated;
    public float rareTrashChance = 0.01f;

    public Dictionary<ThingDef, float> rareTrashTypes = new();
    public Dictionary<ThingDef, ThingDef> guaranteedFilthToTrash = new();

    public ThingDef GetTrashForFilth(Thing filth)
    {
        var def = filth?.def;
        if (def == null)
            return ModDefOfs.VRecyclingE_Trash;
        if (guaranteedFilthToTrash.TryGetValue(def, out var trash))
            return trash;
        if (rareTrashTypes.Any() && Rand.Chance(rareTrashChance))
            return rareTrashTypes.RandomElementByWeight(x => x.Value).Key;
        return ModDefOfs.VRecyclingE_Trash;
    }

    public void ValidateFilthBeforeTrashCreated()
    {
        if (filthBeforeTrashCreated > 0)
            return;

        filthBeforeTrashCreated = DefaultFilthBeforeTrashCreated;
        Log.Error($"[{FilthCreatesTrashModCore.ModName}] - {nameof(filthBeforeTrashCreated)} must be positive, it was {filthBeforeTrashCreated} - fixing by setting it to default value of {DefaultFilthBeforeTrashCreated}.");
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref enableTrashOnPawnCleaning, nameof(enableTrashOnPawnCleaning), true);
        Scribe_Values.Look(ref enableTrashOnRainCleaning, nameof(enableTrashOnRainCleaning), false);
        Scribe_Values.Look(ref enableTrashOnBinsCleaning, nameof(enableTrashOnBinsCleaning), true);
        Scribe_Values.Look(ref enableTrashOnSelfCleaning, nameof(enableTrashOnSelfCleaning), false);

        Scribe_Values.Look(ref filthBeforeTrashCreated, nameof(filthBeforeTrashCreated), 20);
        Scribe_Values.Look(ref rareTrashChance, nameof(rareTrashChance), 0.01f);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.ColumnWidth = 570f;

        listing.Label("FilthCreatesTrashRequiresRestart".Translate());

        listing.CheckboxLabeled(
            "FilthCreatesTrashEnableTrashOnPawnCleaning".Translate(),
            ref enableTrashOnPawnCleaning,
            "FilthCreatesTrashEnableTrashOnPawnCleaningTooltip".Translate());
        listing.CheckboxLabeled(
            "FilthCreatesTrashEnableTrashOnRainCleaning".Translate(),
            ref enableTrashOnRainCleaning,
            "FilthCreatesTrashEnableTrashOnRainCleaningTooltip".Translate());
        listing.CheckboxLabeled(
            "FilthCreatesTrashEnableTrashOnBinsCleaning".Translate(),
            ref enableTrashOnBinsCleaning,
            "FilthCreatesTrashEnableTrashOnBinsCleaningTooltip".Translate());
        listing.CheckboxLabeled(
            "FilthCreatesTrashEnableTrashOnSelfCleaning".Translate(),
            ref enableTrashOnSelfCleaning,
            "FilthCreatesTrashEnableTrashOnSelfCleaningTooltip".Translate());

        listing.GapLine(24f);

        string temp = null;
        listing.TextFieldNumericLabeled(
            "FilthCreatesTrashFilthBeforeTrash".Translate(),
            ref filthBeforeTrashCreated,
            ref temp,
            1,
            int.MaxValue,
            "FilthCreatesTrashFilthBeforeTrashTooltip".Translate());

        rareTrashChance = listing.SliderLabeled(
            "FilthCreatesTrashRareTrashChance".Translate(rareTrashChance),
            rareTrashChance,
            0f,
            1f,
            tooltip: "FilthCreatesTrashRareTrashChanceTooltip".Translate());
        rareTrashChance = GenMath.RoundTo(rareTrashChance, 0.01f);

        listing.End();
    }
}