using System.Collections;
using TMPro;
using UnityEngine;

public class TextAnimOnAppear : MonoBehaviour
{
    public TMP_Text textMesh;
    public string TextToAppear;
    public float TimePerCharacter;
    [SerializeField] bool RunOnAwake;

    Coroutine TextAnim;
    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (RunOnAwake)
            DoAnimation();
    }

    private void OnDisable()
    {
        if(TextAnim != null)
        {
            StopCoroutine(TextAnim);
        }
    }
    void Update()
    {
        
    }

    IEnumerator TextAppear()
    {
        textMesh.text = "";
        for (int i = 0; i < TextToAppear.Length; i++)
        {
            textMesh.text += TextToAppear[i];
            //Debug.Log("This is the character that would appear: " + TextToAppear[i]);
            yield return new WaitForSecondsRealtime(TimePerCharacter);
        }
        TextAnim = null;
    }

    public void DoAnimation()
    {
        textMesh.text = "";
        if (TextAnim != null)
        {
            StopCoroutine(TextAnim);
        }
        TextAnim = StartCoroutine(TextAppear());
    }
}
