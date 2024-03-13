using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FilthCreatesTrash;

public static class WidgetsExtended
{
    public static void CheckboxLabeledMulti(Rect rect, string label, ref MultiCheckboxState checkStatus, bool disabled = false, bool placeCheckboxNearText = false)
    {
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        if (placeCheckboxNearText)
            rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);

        var labelRect = rect;
        labelRect.xMax -= 24f;
        Widgets.Label(labelRect, label);

        if (!disabled && Widgets.ButtonInvisible(rect))
        {
            checkStatus = (MultiCheckboxState)(((byte)checkStatus + 1) % 3);

            switch (checkStatus)
            {
                case MultiCheckboxState.On:
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                    break;
                case MultiCheckboxState.Off:
                case MultiCheckboxState.Partial:
                default:
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                    break;
            }
        }

        var color = GUI.color;
        if (disabled)
            GUI.color = Widgets.InactiveColor;

        var checkboxRect = new Rect(
            rect.x + rect.width - 24f,
            rect.y + (rect.height - 24f) / 2f,
            24f,
            24f);
        var checkboxTexture = checkStatus switch
        {
            MultiCheckboxState.On => Widgets.CheckboxOnTex,
            MultiCheckboxState.Off => Widgets.CheckboxOffTex,
            _ => Widgets.CheckboxPartialTex,
        };

        GUI.DrawTexture(checkboxRect, checkboxTexture);

        if (disabled)
            GUI.color = color;
        Text.Anchor = anchor;
    }
}