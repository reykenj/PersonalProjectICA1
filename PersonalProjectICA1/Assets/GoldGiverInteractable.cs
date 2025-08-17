using UnityEngine;

public class GoldGiverInteractable : MonoBehaviour, IInteractable
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
        GameFlowManager.instance.Gold += 50;
    }

    public float ReturnScreenDist()
    {
        return ScreenDist;
    }
}