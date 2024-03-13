using FilthCreatesTrash.DefExtensions;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FilthCreatesTrash;

public class FilthCreatesTrashModCore : Mod
{
    public const string ModName = "Filth Creates Trash";

    internal static Harmony Harmony { get; } = new("Dra.FilthCreatesTrash");
    public static FilthCreatesTrashModSettings settings;

    public FilthCreatesTrashModCore(ModContentPack content) : base(content)
    {
        settings = GetSettings<FilthCreatesTrashModSettings>();

        LongEventHandler.ExecuteWhenFinished(() =>
        {
            Harmony.PatchAll();

            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                var guaranteed = def.GetModExtension<GuaranteedTrashExtension>();
                var special = def.GetModExtension<SpecialTrashExtension>();

                if (guaranteed != null)
                {
                    if (def.IsFilth)
                    {
                        if (guaranteed.trashType != null)
                            settings.guaranteedFilthToTrash[def] = guaranteed.trashType;
                    }
                    else Log.Error($"[{ModName}] - {def} has {nameof(GuaranteedTrashExtension)}, but is not filth. Ignoring.");
                }

                if (special is { weight: > 0 } && float.IsFinite(special.weight))
                    settings.rareTrashTypes[def] = special.weight;
            }
        });
    }

    public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);

    public override string SettingsCategory() => ModName;
}