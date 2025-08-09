using UnityEngine;

public class AltarInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] string Title;
    [SerializeField] string Description;
    [SerializeField] float ScreenDist = 500.0f;
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

    public float ReturnScreenDist()
    {
        return ScreenDist;
    }
}
