using System;
using UnityEngine;
using UnityEngine.UI;

public static class NoteFactory
{
    /// <summary>
    /// 음표를 구성 요소별로 조립하여 하나의 note-wrap 오브젝트로 생성
    /// </summary>
    public static GameObject CreateNoteWrap(
        Transform parent,                   // 부모 UI 오브젝트
        GameObject headPrefab,              // 음표 머리 프리팹 (온음표, 2분음표 등)
        GameObject stemPrefab = null,       // 스템 (기둥) 프리팹 (선택)
        GameObject flagPrefab = null,       // 깃발 프리팹 (선택)
        GameObject dotPrefab = null,        // 점음표용 점 프리팹 (선택)
        bool stemDown = false,              // 스템 방향 (false면 위, true면 아래)
        Vector2 position = default,         // note-wrap 위치
        float noteScale = 1f,               // 전체 스케일
        float spacing = 36f                 // 오선 간격 (줄 간격)
    )
    {
        // 🎼 note-wrap 오브젝트 생성
        GameObject wrap = new GameObject("note-wrap", typeof(RectTransform));
        var wrapRt = wrap.GetComponent<RectTransform>();
        wrapRt.SetParent(parent, false);
        wrapRt.anchorMin = wrapRt.anchorMax = new Vector2(0.5f, 0.5f);
        wrapRt.pivot = new Vector2(0.5f, 0.5f);
        wrapRt.localScale = Vector3.one * noteScale;
        wrapRt.anchoredPosition = position;

        float verticalCorrection = spacing * -1.0f; // 🎯 시각적 중심 보정값 (도->솔 현상 해결)

        // 🎵 Head (음표 머리)
        if (headPrefab)
        {
            var head = UnityEngine.Object.Instantiate(headPrefab, wrap.transform);
            var rt = head.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            rt.sizeDelta = headPrefab.name switch
            {
                string name when name.Contains("1") => new Vector2(spacing * 0.75f, spacing * 0.75f),
                string name when name.Contains("2") => new Vector2(spacing * 0.6f, spacing * 0.6f),
                string name when name.Contains("4") => new Vector2(spacing * 0.6f, spacing * 0.6f),
                _ => new Vector2(spacing * 0.8f, spacing * 0.8f),
            };

            // 🎯 음표 머리를 위로 올려 실제 줄과 맞추기
            rt.anchoredPosition = new Vector2(0, verticalCorrection);
        }

        // 🎵 Stem (기둥)
        if (stemPrefab)
        {
            var stem = UnityEngine.Object.Instantiate(stemPrefab, wrap.transform);
            var rt = stem.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            rt.sizeDelta = new Vector2(spacing * 0.2f, spacing * 2.0f);

            float offsetY = spacing;
            rt.anchoredPosition = stemDown
            ? new Vector2(0.3f * spacing, offsetY + verticalCorrection)
            : new Vector2(-0.3f * spacing, -offsetY + verticalCorrection);

            rt.localRotation = stemDown ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
        }

        // 🎏 Flag (깃발)
        if (flagPrefab)
        {
            var flag = UnityEngine.Object.Instantiate(flagPrefab, wrap.transform);
            var rt = flag.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            rt.sizeDelta = flagPrefab.name switch
            {
                string name when name.Contains("8") => new Vector2(spacing * 2.0f, spacing * 2.0f),
                string name when name.Contains("16") => new Vector2(spacing * 2.0f, spacing * 2.0f),
                _ => new Vector2(spacing * 1.2f, spacing * 1.2f),
            };

            // ✅ offset 방향 유지 (오른쪽 위 or 아래)
            Vector2 offset = stemDown
                ? new Vector2(0.55f * spacing, 1.1f * spacing + verticalCorrection)
                : new Vector2(-0.05f * spacing, -1.4f * spacing + verticalCorrection);

            rt.anchoredPosition = offset;

            // ✅ 반전 조건 반대로 수정: stemDown == false → -1 (도~라)
            rt.localScale = new Vector3(1f, stemDown ? 1f : -1f, 1f);
        }


        // 🎯 Dot (점음표)
        if (dotPrefab)
        {
            var dot = UnityEngine.Object.Instantiate(dotPrefab, wrap.transform);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            rt.sizeDelta = new Vector2(spacing * 0.25f, spacing * 0.25f);
            rt.anchoredPosition = new Vector2(spacing * 0.5f, 0f);
        }

        return wrap;
    }
}