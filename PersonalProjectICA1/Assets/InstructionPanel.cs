using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InstructionPanel : MonoBehaviour
{
    [SerializeField] TextAnimOnAppear Title;
    [SerializeField] TextAnimOnAppear Description;
    

    public string TitleTextToAppear;
    public string DescriptionTextToAppear;
    public Image image;

    [SerializeField] RectTransform Center;
    [SerializeField] RectTransform panelRect;


    [SerializeField] float moveDuration = 1f;

    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 endPos;
    private float moveTimer;


    bool isMoving = true;
    bool isGoingBack = false;
    private Vector2 targetAnchorPos;

    public RectTransform layoutRoot;

    private void Awake()
    {
        startPos = new Vector2(0, Screen.height * 1.1f);
        targetAnchorPos = new Vector2(Screen.width / 2, Screen.height);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        isGoingBack = false;
        StartCoroutine(PrepLayoutBeforeAnimation());
    }

    private void OnDisable()
    {
        panelRect.anchoredPosition = startPos;
    }
    void Update()
    {
        if (!isMoving) return;

        if (isGoingBack)
        {
            moveTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            moveTimer += Time.unscaledDeltaTime;
        }
        float t = Mathf.Clamp01(moveTimer / moveDuration);
        panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        if (t >= 1f && !isGoingBack)
        {
            isMoving = false;
            panelRect.anchoredPosition = endPos;
            Title.DoAnimation();
            Description.DoAnimation();
        }
        else if (t <= 0 && isGoingBack)
        {
            gameObject.SetActive(false);
        }
    }

    public void SendBack()
    {
        isMoving = true;
        isGoingBack = true;
    }

    public void RefreshLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
    }

    IEnumerator PrepLayoutBeforeAnimation()
    {
        Debug.Log("11AAAAAAAAAAAAAA111");
        Title.textMesh.text = TitleTextToAppear;
        Description.textMesh.text = DescriptionTextToAppear;
        Title.TextToAppear = TitleTextToAppear;
        Description.TextToAppear = DescriptionTextToAppear;
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        Title.textMesh.text = " ";
        Description.textMesh.text = " ";
        Debug.Log("1111111BBBBBBBBBBBBBBBBBB111");




        RectTransform parent = panelRect.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, targetAnchorPos, null, out Vector2 localPoint);
        Vector2 offset = (Vector2)panelRect.position - (Vector2)Center.position;
        Vector2 desiredPos = localPoint + offset;

        panelRect.anchoredPosition = startPos;
        endPos = desiredPos;
        moveTimer = 0f;
        isMoving = true;
    }


}
