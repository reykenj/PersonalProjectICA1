using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InstructionPanel : MonoBehaviour
{
    [SerializeField] TextAnimOnAppear Title;
    [SerializeField] TextAnimOnAppear Description;

    public string TitleTextToAppear;
    public string DescriptionTextToAppear;

    [SerializeField] RectTransform Center;
    [SerializeField] RectTransform panelRect;


    [SerializeField] float moveDuration = 1f;

    private Vector2 startPos;
    private Vector2 endPos;
    private float moveTimer;


    bool isMoving = true;
    private Vector2 targetAnchorPos;

    public RectTransform layoutRoot;

    private void Start()
    {
        startPos = new Vector2(0, Screen.height * 1.1f);
        targetAnchorPos = new Vector2(Screen.width / 2, Screen.height);
    }

    private void OnEnable()
    {
        StartCoroutine(PrepLayoutBeforeAnimation());
    }

    private void OnDisable()
    {
        panelRect.anchoredPosition = startPos;
    }
    void Update()
    {
        if (!isMoving) return;

        moveTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(moveTimer / moveDuration);
        panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        if (t >= 1f)
        {
            isMoving = false;
            panelRect.anchoredPosition = endPos;
            Title.DoAnimation();
            Description.DoAnimation();
        }
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
