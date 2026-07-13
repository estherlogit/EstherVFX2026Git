using UnityEngine;

public class ImpactTrigger : MonoBehaviour
{
    public Spellbook spellbook;
    private GameObject impactGO;
    
    [Tooltip("How strongly the dummy wiggles on impact. ~5000 is standard strength")] public float impactStrength = 5000.0f;
    bool canTrigger = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            if (canTrigger)
            {
                canTrigger = false;

                if (impactGO == null)
                {
                    switch (spellbook._currentSpellData.spellTypeID)
                    {
                        // TODO: Refactor, handle NaN
                        case 1:
                            impactGO = Instantiate(spellbook.slashSpells[spellbook._currentSpellData.spellID].burstPrefab);

                            break;
                        case 2:
                            impactGO = Instantiate(spellbook.castSpells[spellbook._currentSpellData.spellID].impactPrefab);

                            break;
                        case 3:
                            //impactGO = Instantiate(spellbook.summonSpells[spellbook._currentSpellData.spellID].impactPrefab);
                            break;
                        default:
                            Debug.Log("Invalid category.");
                            break;
                    }
            
                    impactGO.transform.position = spellbook._dummyTransform.position;
                    impactGO.GetComponent<ParticleSystem>().Play();
                    ParticleSystem particleSystem = impactGO.GetComponent<ParticleSystem>();
                    var main = particleSystem.main;
                    float spellDuration = main.duration; // + main.startLifetimeMultiplier;
                    particleSystem.Play();
            
                    Destroy (impactGO, spellDuration);
            
                    Invoke("ResetTrigger", 0.75f);
                }
            }
        }
    }

    private void ResetTrigger() { canTrigger = true; }
}
