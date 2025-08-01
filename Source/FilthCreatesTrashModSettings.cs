using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FilthCreatesTrash;

public class FilthCreatesTrashModSettings : ModSettings
{
    public bool enableTrashOnPawnCleaning;
    public bool enableTrashOnRainCleaning;
    public bool enableTrashOnCarriedFilth;
    public bool enableTrashOnBinsCleaning;
    public bool enableTrashOnSelfCleaning;

    private const int DefaultFilthBeforeTrashCreated = 20;
    public int filthBeforeTrashCreated;
    public float rareTrashChance;

    public Dictionary<ThingDef, float> rareTrashTypes = new();
    public Dictionary<ThingDef, ThingDef> guaranteedFilthToTrash = new();

    public FilthCreatesTrashModSettings() => RestoreDefaults();

    public void RestoreDefaults()
    {
        enableTrashOnPawnCleaning = true;
        enableTrashOnRainCleaning = false;
        enableTrashOnCarriedFilth = false;
        enableTrashOnBinsCleaning = true;
        enableTrashOnSelfCleaning = false;

        filthBeforeTrashCreated = DefaultFilthBeforeTrashCreated;
        rareTrashChance = 0.01f;
    }

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
        Scribe_Values.Look(ref enableTrashOnCarriedFilth, nameof(enableTrashOnCarriedFilth), false);
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

        listing.Label("FCT_RequiresRestart".Translate());

        listing.CheckboxLabeled(
            "FCT_EnableTrashOnPawnCleaning".Translate(),
            ref enableTrashOnPawnCleaning,
            "FCT_EnableTrashOnPawnCleaningTooltip".Translate());
        listing.CheckboxLabeled(
            "FCT_EnableTrashOnRainCleaning".Translate(),
            ref enableTrashOnRainCleaning,
            "FCT_EnableTrashOnRainCleaningTooltip".Translate());
        listing.CheckboxLabeled(
            "FCT_EnableTrashOnCarriedFilthCleaning".Translate(),
            ref enableTrashOnCarriedFilth,
            "FCT_EnableTrashOnCarriedFilthCleaningTooltip".Translate());
        listing.CheckboxLabeled(
            "FCT_EnableTrashOnBinsCleaning".Translate(),
            ref enableTrashOnBinsCleaning,
            "FCT_EnableTrashOnBinsCleaningTooltip".Translate());
        listing.CheckboxLabeled(
            "FCT_EnableTrashOnSelfCleaning".Translate(),
            ref enableTrashOnSelfCleaning,
            "FCT_EnableTrashOnSelfCleaningTooltip".Translate());

        listing.GapLine(24f);

        string temp = null;
        listing.TextFieldNumericLabeled(
            "FCT_FilthBeforeTrash".Translate(),
            ref filthBeforeTrashCreated,
            ref temp,
            1,
            int.MaxValue,
            "FCT_FilthBeforeTrashTooltip".Translate());

        rareTrashChance = listing.SliderLabeled(
            "FCT_RareTrashChance".Translate(rareTrashChance),
            rareTrashChance,
            0f,
            1f,
            tooltip: "FCT_RareTrashChanceTooltip".Translate());
        rareTrashChance = GenMath.RoundTo(rareTrashChance, 0.01f);

        listing.GapLine(24f);

        if (listing.ButtonText("FCT_ResetDefault".Translate(), widthPct: 0.5f))
            RestoreDefaults();

        listing.End();
    }
}