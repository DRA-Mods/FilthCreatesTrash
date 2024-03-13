using UnityEngine;
using Verse;

namespace FilthCreatesTrash;

public static class ListingExtensions
{
    public static void TextFieldNumericLabeled<T>(this Listing_Standard listing, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f, string tooltip = null) where T : struct
    {
        var rect = listing.GetRect(Text.LineHeight);
        if (listing.BoundingRectCached == null || rect.Overlaps(listing.BoundingRectCached.Value))
        {
            TextFieldNumericLabeled(rect, label, ref val, ref buffer, min, max);

            if (tooltip != null)
                TooltipHandler.TipRegion(rect, tooltip);
        }

        listing.Gap(listing.verticalSpacing);
    }

    public static void TextFieldNumericLabeled<T>(Rect rect, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
    {
        Widgets.Label(rect.LeftHalf().Rounded(), label);
        Widgets.TextFieldNumeric(rect.RightHalf().Rounded(), ref val, ref buffer, min, max);
    }
}