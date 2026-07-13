using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New UI Theme", menuName = "VFX Apprentice/Development/New UI Theme")]
public class UIThemeScriptableObject : SpellScriptableObject
{
    [Header("UI Assets")] 
    [SerializeField] internal Sprite _slashIcon;
    [SerializeField] internal Sprite _slash;
    [SerializeField] internal Sprite _slashActive;
    [SerializeField] internal Sprite _castIcon;
    [SerializeField] internal Sprite _cast;
    [SerializeField] internal Sprite _castActive;
    [SerializeField] internal Sprite _summonIcon;
    [SerializeField] internal Sprite _summon;
    [SerializeField] internal Sprite _summonActive;
}
