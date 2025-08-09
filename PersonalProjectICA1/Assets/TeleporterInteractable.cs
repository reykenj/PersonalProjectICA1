using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleporterInteractable : MonoBehaviour, IInteractable
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
        StartCoroutine(GameFlowManager.instance.MoveScene("MainScene"));
    }

    public float ReturnScreenDist()
    {
        return ScreenDist;
    }
}
