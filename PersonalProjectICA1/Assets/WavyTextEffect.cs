using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class WavyTextEffect : MonoBehaviour
{
    TMP_Text textMesh;
    Mesh mesh;
    Vector3[] vertices;
    public float amplitude = 5f;
    public float frequency = 2f;
    public float speed = 2f;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        textMesh.ForceMeshUpdate();
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            vertices = textInfo.meshInfo[materialIndex].vertices;
            Vector3 offset = new Vector3(0, Mathf.Sin(Time.unscaledTime * speed + i * frequency) * amplitude, 0);
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
