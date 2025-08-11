using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellStats : MonoBehaviour
{
    [SerializeField] GameObject BadgePrefab;
    [SerializeField] GameObject BadgesContainer;
    [SerializeField] TextMeshProUGUI SpellName;
    [SerializeField] List<RectTransform> Layoutroots;
    public SpellSlotUI currentSpellSlotUI;
    public static SpellStats instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        if (currentSpellSlotUI != null && currentSpellSlotUI.spell != null)
        {
            StartCoroutine(RefreshCorout());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    IEnumerator RefreshCorout()
    {
        int OGchildcount = BadgesContainer.transform.childCount;
        if (OGchildcount > 0)
        {
            for (int i = OGchildcount - 1; i >= 0; i--)
            {
                GameObject child = BadgesContainer.transform.GetChild(i).gameObject;
                ObjectPool.ReturnObj(child);
                child.transform.localScale = Vector3.one;
            }
        }

        for (int i = 0; i < currentSpellSlotUI.SpellsAffectingIt.Count; i++)
        {
            GameObject BadgeUIOBJ = ObjectPool.GetObj(BadgePrefab.name);
            BadgeUIOBJ.transform.SetParent(BadgesContainer.transform, false);

            SpellBadge.TryGetSpellBadge(BadgeUIOBJ, out SpellBadge spellBadge);
            spellBadge.SetImageSpell(currentSpellSlotUI.SpellsAffectingIt[i]);
        }
        SpellName.text = currentSpellSlotUI.spell.name;
        yield return null;
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < Layoutroots.Count; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Layoutroots[i]);
        }
    }

}
