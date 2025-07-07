using UnityEngine;

public class RefreshInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] string Title;
    [SerializeField] string Description;
    [SerializeField] SpellShop SpellShop;

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
        SpellShop.RefreshShop();
    }
}
