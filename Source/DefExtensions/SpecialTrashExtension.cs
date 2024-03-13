using System.Collections.Generic;
using Verse;

namespace FilthCreatesTrash.DefExtensions;

public class SpecialTrashExtension : DefModExtension
{
    public float weight = 0;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
            yield return error;

        if (weight <= 0 || !float.IsFinite(weight))
            yield return $"{nameof(SpecialTrashExtension)} error - {nameof(weight)} must be a positive number, currently it is {weight}";
    }
}