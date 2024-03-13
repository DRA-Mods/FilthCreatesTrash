using RimWorld;
using Verse;

namespace FilthCreatesTrash;

[DefOf]
public static class ModDefOfs
{
    static ModDefOfs() => DefOfHelper.EnsureInitializedInCtor(typeof(ModDefOfs));

    public static ThingDef VRecyclingE_Trash;
}