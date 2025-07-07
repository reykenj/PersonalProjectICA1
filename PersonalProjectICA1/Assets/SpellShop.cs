using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpellShop : MonoBehaviour
{
    [SerializeField] List<SpellDrop> spelldrops;
    [SerializeField] List<Spell> SpellShopPool;

    private void Start()
    {
        RefreshShop();
    }
    public void RefreshShop()
    {
        for (int i = 0; i < spelldrops.Count; i++)
        {
            spelldrops[i].gameObject.SetActive(false);
            spelldrops[i].gameObject.SetActive(true);
            spelldrops[i].Initialise(SpellShopPool[Random.Range(0, SpellShopPool.Count)]);
        }
    }
}
