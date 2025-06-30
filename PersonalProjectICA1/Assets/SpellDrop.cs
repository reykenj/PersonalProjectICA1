using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpellDrop : MonoBehaviour
{
    [SerializeField] GameObject ActiveSpellEffect;
    [SerializeField] GameObject ModifierSpellEffect;
    [SerializeField] GameObject SpecialSpellEffect;
    [SerializeField] GameObject TeleportEffect;
    [SerializeField] Spell spell;
    private static Dictionary<GameObject, SpellDrop> cache = new Dictionary<GameObject, SpellDrop>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public static bool TryGetSpellDrop(GameObject obj, out SpellDrop spellDrop)
    {
        return cache.TryGetValue(obj, out spellDrop);
    }
    private void Awake()
    {
        cache[gameObject] = this;
        GameFlowManager.instance.ShopChoiceChosen += OnShopChoiceChosen;
    }
    private void OnDestroy()
    {
        cache.Remove(gameObject);
    }
    private void OnEnable()
    {
        TeleportEffect.SetActive(true);
        ActiveSpellEffect.SetActive(false);
        ModifierSpellEffect.SetActive(false);
        SpecialSpellEffect.SetActive(false);
    }
    public void Initialise(Spell spell)
    {
        this.spell = spell;
        if (spell.UseTurn) // change this later
        {
            ActiveSpellEffect.SetActive(true);
        }
        else
        {
            ModifierSpellEffect.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameFlowManager.instance.ShopChoiceChosen.Invoke();
            //might need to turn off the collider if it repeat invokes

            for (int i = 0; i < GameFlowManager.instance.Inventory.SpellArray.Count; i++)
            {
                if (GameFlowManager.instance.Inventory.SpellArray[i].spell == null)
                {
                    SpellContainer SC = GameFlowManager.instance.Inventory.SpellArray[i];
                    SC.spell = spell;
                    if (SC.spell != null)
                    {
                        SC.TempProjInfo = SC.spell.OGProjectileInformation;
                    }
                    else
                    {
                        SC.TempProjInfo = new ProjectileInformation();
                    }
                    GameFlowManager.instance.Inventory.SpellArray[i] = SC;

                    //Debug.Log("[SPELLPICKUP] Found and implemented");
                    break;
                }
            }
        }
    }

    void OnShopChoiceChosen()
    {
        ObjectPool.ReturnObj(gameObject);
    }
}
