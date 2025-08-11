using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBadge : MonoBehaviour
{
    private static Dictionary<GameObject, SpellBadge> cache = new Dictionary<GameObject, SpellBadge>();

    [SerializeField] Image image;
    private void Awake()
    {
        cache[gameObject] = this;
    }
    private void OnDestroy()
    {
        cache.Remove(gameObject);
    }

    public static bool TryGetSpellBadge(GameObject obj, out SpellBadge spellBadge)
    {
        return cache.TryGetValue(obj, out spellBadge);
    }

    public void SetImageSpell(Spell spell)
    {
        if (image != null)
        {
            image.sprite = spell.RuneIcon;
        }
    }
}
