using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void MoveToScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
