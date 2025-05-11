using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 음표를 구성 요소별로 조립해주는 유틸리티 함수
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
        // 1. note-wrap 생성
        GameObject wrap = new GameObject("note-wrap", typeof(RectTransform));
        var wrapRt = wrap.GetComponent<RectTransform>();
        wrapRt.SetParent(parent, false);
        wrapRt.anchorMin = wrapRt.anchorMax = new Vector2(0.5f, 0.5f);
        wrapRt.pivot = new Vector2(0.5f, 0.5f);
        wrapRt.localScale = Vector3.one * noteScale;
        wrapRt.anchoredPosition = position;

        // 2. note-head (필수)
        var head = GameObject.Instantiate(headPrefab, wrap.transform);
        var headRt = head.GetComponent<RectTransform>();
        headRt.anchoredPosition = Vector2.zero;

        // 3. stem (선택)
        if (stemPrefab != null)
        {
            var stem = GameObject.Instantiate(stemPrefab, wrap.transform);
            var stemRt = stem.GetComponent<RectTransform>();

            // 기본 offset (좌/우 위치 및 회전 방향)
            float x = stemDown ? -20f : 20f;
            float y = 0f;
            float scaleY = stemDown ? -1f : 1f;

            stemRt.anchoredPosition = new Vector2(x, y);
            stemRt.localScale = new Vector3(1f, scaleY, 1f);
        }

        // 4. flag (선택, 보통 스템 위쪽에 붙임)
        if (flagPrefab != null)
        {
            var flag = GameObject.Instantiate(flagPrefab, wrap.transform);
            var flagRt = flag.GetComponent<RectTransform>();

            // flag는 스템 끝 쪽에서 튀어나오므로 상대 위치 조정
            float x = stemDown ? -24f : 24f;
            float y = stemDown ? -40f : 40f;
            float scaleY = stemDown ? -1f : 1f;

            flagRt.anchoredPosition = new Vector2(x, y);
            flagRt.localScale = new Vector3(1f, scaleY, 1f);
        }

        // 5. dot (점음표일 때만)
        if (dotPrefab != null)
        {
            var dot = GameObject.Instantiate(dotPrefab, wrap.transform);
            var dotRt = dot.GetComponent<RectTransform>();

            // 점은 머리 오른쪽에 살짝
            dotRt.anchoredPosition = new Vector2(40f, 0f);
        }

        return wrap;
    }
}
