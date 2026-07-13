using UnityEngine;

public class SpellScriptableObject : ScriptableObject
{
    public virtual void UpdateSpell(Spellbook spellbook) {}
    
    //public virtual void InstantiateSpell() {}
    
    public virtual void DestroySpell(GameObject spellToDestroy) {}
}
