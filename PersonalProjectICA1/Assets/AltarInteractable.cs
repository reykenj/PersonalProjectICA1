using UnityEngine;

public class AltarInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] string Title;
    [SerializeField] string Description;
    
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
        if (GameFlowManager.instance.SpellEditor != null && !GameFlowManager.instance.SpellEditor.activeSelf)
        {
            GameFlowManager.instance.SpellEditor.SetActive(true);
        }
    }
}
