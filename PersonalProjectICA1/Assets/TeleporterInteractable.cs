using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleporterInteractable : MonoBehaviour, IInteractable
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
        StartCoroutine(GameFlowManager.instance.MoveScene("MainScene"));
    }
}
