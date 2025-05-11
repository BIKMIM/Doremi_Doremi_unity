using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��ǥ�� ���� ��Һ��� �������ִ� ��ƿ��Ƽ �Լ�
/// </summary>
public static class NoteFactory
{
    public static GameObject CreateNoteWrap(
        Transform parent,
        GameObject headPrefab,
        GameObject stemPrefab = null,
        GameObject flagPrefab = null,
        GameObject dotPrefab = null,
        bool stemDown = false,
        Vector2 position = default,
        float noteScale = 1f
    )
    {
        // 1. note-wrap ����
        GameObject wrap = new GameObject("note-wrap", typeof(RectTransform));
        var wrapRt = wrap.GetComponent<RectTransform>();
        wrapRt.SetParent(parent, false);
        wrapRt.anchorMin = wrapRt.anchorMax = new Vector2(0.5f, 0.5f);
        wrapRt.pivot = new Vector2(0.5f, 0.5f);
        wrapRt.localScale = Vector3.one * noteScale;
        wrapRt.anchoredPosition = position;

        // 2. note-head (�ʼ�)
        var head = GameObject.Instantiate(headPrefab, wrap.transform);
        var headRt = head.GetComponent<RectTransform>();
        headRt.anchoredPosition = Vector2.zero;

        // 3. stem (����)
        if (stemPrefab != null)
        {
            var stem = GameObject.Instantiate(stemPrefab, wrap.transform);
            var stemRt = stem.GetComponent<RectTransform>();

            // �⺻ offset (��/�� ��ġ �� ȸ�� ����)
            float x = stemDown ? -20f : 20f;
            float y = 0f;
            float scaleY = stemDown ? -1f : 1f;

            stemRt.anchoredPosition = new Vector2(x, y);
            stemRt.localScale = new Vector3(1f, scaleY, 1f);
        }

        // 4. flag (����, ���� ���� ���ʿ� ����)
        if (flagPrefab != null)
        {
            var flag = GameObject.Instantiate(flagPrefab, wrap.transform);
            var flagRt = flag.GetComponent<RectTransform>();

            // flag�� ���� �� �ʿ��� Ƣ����Ƿ� ��� ��ġ ����
            float x = stemDown ? -24f : 24f;
            float y = stemDown ? -40f : 40f;
            float scaleY = stemDown ? -1f : 1f;

            flagRt.anchoredPosition = new Vector2(x, y);
            flagRt.localScale = new Vector3(1f, scaleY, 1f);
        }

        // 5. dot (����ǥ�� ����)
        if (dotPrefab != null)
        {
            var dot = GameObject.Instantiate(dotPrefab, wrap.transform);
            var dotRt = dot.GetComponent<RectTransform>();

            // ���� �Ӹ� �����ʿ� ��¦
            dotRt.anchoredPosition = new Vector2(40f, 0f);
        }

        return wrap;
    }
}
