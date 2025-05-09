using UnityEngine;

[ExecuteAlways] // �����Ϳ����� ����ǰ�
public class NotePlacer : MonoBehaviour
{
    public RectTransform staffPanel;
    public RectTransform noteImage;
    public float staffHeight = 150f;

    [Range(-2f, 6f)] // �� ���� ��ġ �ε���
    public float lineIndex = 0f;

    private void Start()
    {
        PlaceNote();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            PlaceNote();
        }
    }
#endif

    public void PlaceNote()
    {
        if (noteImage == null || staffPanel == null) return;

        float spacing = staffHeight / 4f; // �� ����
        float y = lineIndex * spacing;
        noteImage.anchoredPosition = new Vector2(noteImage.anchoredPosition.x, y);
    }
}
