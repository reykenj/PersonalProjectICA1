using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HandsInstructionPanel : MonoBehaviour
{
    public void OnEnable()
    {
        GameFlowManager.instance.ActivateInstructionPanel("Exit Instructions", "Press E again to exit");
    }

    public void OnDisable()
    {
        GameFlowManager.instance.DeactivateInstructionPanel();
    }


}
