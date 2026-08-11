using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Current Spell", menuName = "VFX Apprentice/Development/New Current Spell")]
public class CurrentSpellScriptableObject : ScriptableObject
{
    [Header("Category Data")]
    [SerializeField] internal string spellTypeName;

    [Header("Spell Data")]
    [SerializeField]internal string spellName;
    
    [Header("ID Data")]
    [SerializeField]internal int spellTypeID;
    [SerializeField]internal int spellID;
    [SerializeField]internal int spellMaxID;
}
