using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSymbol : MonoBehaviour
{
    [SerializeField] FadeAnim blackFade;
    [SerializeField] AnimationCurve ScaleCurve;
    [SerializeField] Transform TargetTransform;
    [SerializeField] InstructionPanel IPanel;
    [SerializeField] float timer = 0;
    [SerializeField] float Maxtimer = 1;

    private void Start()
    {
        transform.localScale = new Vector3(ScaleCurve.Evaluate(0), ScaleCurve.Evaluate(0), 0);
        //GameFlowManager.instance.Player.GetComponent<Humanoid>().OnDeath += OnPlayerDeath;
        //gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //GameFlowManager.instance.Player.GetComponent<Humanoid>().OnDeath += OnPlayerDeath;
    }


    private void OnDestroy()
    {
        if (GameFlowManager.instance != null && GameFlowManager.instance.Player != null)
        {
            var humanoid = GameFlowManager.instance.Player.GetComponent<Humanoid>();
            if (humanoid != null)
                humanoid.OnDeath -= OnPlayerDeath;
        }
    }

    public void OnPlayerDeath()
    {
        //gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(DeathSymbolAnimation());
    }

    IEnumerator DeathSymbolAnimation()
    {
        // Scale animation
        timer = 0;
        while (timer < Maxtimer)
        {
            timer += Time.deltaTime;
            transform.localScale = new Vector3(ScaleCurve.Evaluate(timer), ScaleCurve.Evaluate(timer), 0);
            yield return null;
        }

        // Fade in background
        timer = 0;
        Vector3 OldPos = transform.position;
        StartCoroutine(blackFade.Fade(1.0f, 1.0f));
        while (timer < 1.0f)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(OldPos, TargetTransform.position, timer);
            transform.Rotate(0, 0, 1000 * Time.deltaTime);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0, 0, -45);
        IPanel.DescriptionTextToAppear = "KILLS: " + GameFlowManager.instance.TotalMonKills.ToString() + "\n" +
                                 "SPELLS BOUGHT: " + GameFlowManager.instance.TotalSpellsBought.ToString() + "\n" +
                                 "TOTAL GOLD GAINED: " + GameFlowManager.instance.TotalGoldCollected.ToString() + "\n" +
                                 "DIED ON LEVEL: " + GameFlowManager.instance.Round.ToString() + "\n";
        IPanel.gameObject.SetActive(true);
    }
}
