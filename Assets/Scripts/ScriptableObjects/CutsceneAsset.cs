using System;
using System.Collections.Generic;
//using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

[CreateAssetMenu(fileName = "CutsceneAsset", menuName = "Scriptable Objects/CutsceneAsset")]
public class CutsceneAsset : ScriptableObject
{
    [Header("Cutscene Lines")]
    [Tooltip("Dialog lines in the cutscene sequence, if in Localization format, use a Dollar Sign ($) followed by the key to reference the localized text.")]
    public List<String> dialogLines;
    [Header("Character Sprites")]
    [Tooltip("The image to be shown for the character speaking the dialog line, corresponding to the same index of the Cutscene Lines.")]
    public List<Sprite> characterSprites;
    [Header("Character Names")]
    [Tooltip("The person(s) speaking the dialog line, corresponding to the same index of the Cutscene Lines.")]
    public List<String> characterNames;
}
