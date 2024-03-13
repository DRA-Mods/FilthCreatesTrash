using System.Collections.Generic;
using Verse;

namespace FilthCreatesTrash.DefExtensions;

public class GuaranteedTrashExtension : DefModExtension
{
    public ThingDef trashType = null;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
            yield return error;

        if (trashType == null)
            yield return $"{nameof(GuaranteedTrashExtension)} error - {nameof(trashType)} must be declared";
    }
}