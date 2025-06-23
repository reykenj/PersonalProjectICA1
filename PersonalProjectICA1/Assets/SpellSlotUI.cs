using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellSlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private static Dictionary<GameObject, SpellSlotUI> cache = new Dictionary<GameObject, SpellSlotUI>();
    public Spell spell;
    [SerializeField] Image slotimage;

    public bool isBeingHeld = false;
    public int Index;
    public static SpellSlotUI DragIconChosen;
    public Sprite EmptySpellSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        cache[gameObject] = this;
    }
    private void OnDestroy()
    {
        cache.Remove(gameObject);
    }

    public static bool TryGetSpellSlotUI(GameObject obj, out SpellSlotUI spellSlotUI)
    {
        return cache.TryGetValue(obj, out spellSlotUI);
    }

    private void Start()
    {
        if (DragIconChosen == null)
        {
            GameObject dragicon =  ObjectPool.GetObj("SpellSlotUI");
            TryGetSpellSlotUI(dragicon, out SpellSlotUI spellSlotUI);
            DragIconChosen = spellSlotUI;

            Transform parentDrag = transform.parent;
            while (parentDrag.name != "Canvas")
            {
                parentDrag = parentDrag.parent;
            }
            dragicon.transform.SetParent(parentDrag, false);
            dragicon.SetActive(false);
        }
    }

    public void SetSpell(Spell spell, int index)
    {
        this.spell = spell;
        if (spell != null)
        {
            slotimage.sprite = spell.RuneIcon;
            Index = index;
        }
        else{
            slotimage.sprite = EmptySpellSlot;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isBeingHeld = true;
        DragIconChosen.gameObject.SetActive(true);
        DragIconChosen.SetSpell(spell, Index);
        slotimage.sprite = EmptySpellSlot;
        Debug.Log("Slider is being held");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isBeingHeld = false;
        DragIconChosen.gameObject.SetActive(false);

        // Change Children

        //DragIconChosen.SetSpell(spell);
        Debug.Log("Slider released");
    }
}
