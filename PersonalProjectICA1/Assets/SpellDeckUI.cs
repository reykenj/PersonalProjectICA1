using UnityEngine;

public class SpellDeckUI : MonoBehaviour
{
    [SerializeField] AttackHandler attackHandler;
    [SerializeField] GameObject SpellSlotUIPrefab;

    private void Start()
    {
        OnSpellDeckChangeSize();
    }
    private void OnEnable()
    {
        RefreshUISpellDeck();
    }

    public void OnUIChangeConfirmed()
    {
        for (int i = 0; i < attackHandler.SpellArray.Count; i++)
        {
            SpellSlotUI.TryGetSpellSlotUI(transform.GetChild(i).gameObject, out SpellSlotUI spellslotUI);
            if (spellslotUI == null)
            {
                continue;
            }
            SpellContainer SC = attackHandler.SpellArray[i];
            SC.spell = spellslotUI.spell;
            if (SC.spell != null)
            {
                SC.TempProjInfo = SC.spell.OGProjectileInformation;
            }
            else
            {
                SC.TempProjInfo = new ProjectileInformation();
            }
            attackHandler.SpellArray[i] = SC;
        }
        attackHandler.DontCast.Clear();
        attackHandler.Turn = 0;
    }
    void RefreshUISpellDeck()
    {
        for (int i = 0; i < attackHandler.SpellArray.Count; i++)
        {
            SpellSlotUI.TryGetSpellSlotUI(transform.GetChild(i).gameObject, out SpellSlotUI spellslotUI);
            if (spellslotUI == null)
            {
                continue;
            }
            spellslotUI.SetSpell(attackHandler.SpellArray[i].spell, i);
        }
    }
    void OnSpellDeckChangeSize()
    {
        if (transform.childCount == attackHandler.SpellArray.Count)
        {
            return;
        }

        for (int i = 0; i < attackHandler.SpellArray.Count; i++)
        {
            GameObject SpellSlotUIOBJ = ObjectPool.GetObj(SpellSlotUIPrefab.name);
            SpellSlotUIOBJ.transform.SetParent(transform, false);

            SpellSlotUI.TryGetSpellSlotUI(transform.GetChild(i).gameObject, out SpellSlotUI spellslotUI);
            spellslotUI.ParentDeck = this;
        }


        RefreshUISpellDeck();
    }
}
