using UnityEngine;

public class RefreshInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] string Title;
    [SerializeField] string Description;
    [SerializeField] SpellShop SpellShop;
    [SerializeField] int GoldCost = 10;
    [SerializeField] float GoldRerollMult = 1.5f;

    public void EnterNear()
    {
        GameFlowManager.instance.ActivateInstructionPanel(Title, Description);

    }

    public void ExitNear()
    {
        GameFlowManager.instance.DeactivateInstructionPanel();
    }

    public void Interact()
    {
        if (GameFlowManager.instance.Gold >= GoldCost)
        {
            GameFlowManager.instance.Gold -= GoldCost;
        }
        else
        {
            return;
        }
        Description = "Restock and Refresh? (Costs " + GoldCost.ToString() + " Gold, Press E)";
        GoldCost = (int)(GoldCost * GoldRerollMult);
        SpellShop.RefreshShop();
    }
}
