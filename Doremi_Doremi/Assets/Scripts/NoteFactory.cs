using System;
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
        if (headPrefab)
        {
            var head = UnityEngine.Object.Instantiate(headPrefab, wrap.transform);
            var rt = head.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            // 🎯 프리팹 이름에 따라 크기 지정
            Vector2 headSize = headPrefab.name switch
            {
                string name when name.Contains("1") => new Vector2(34, 34),
                string name when name.Contains("2") => new Vector2(22, 22),
                string name when name.Contains("4") => new Vector2(23, 23),
                _ => new Vector2(30, 30),
            };

            rt.sizeDelta = headSize;

            // 🎯 위치 오프셋도 종류별로 조정 가능
            Vector2 headOffset = headPrefab.name switch
            {
                string name when name.Contains("1") => new Vector2(0f, 28f),
                string name when name.Contains("2") => new Vector2(0f, -26f),
                string name when name.Contains("4") => new Vector2(0f, -26f),
                _ => Vector2.zero,
            };

            rt.anchoredPosition = headOffset;
        }



        // 3. stem (선택)
        if (stemPrefab)
        {
            var stem = UnityEngine.Object.Instantiate(stemPrefab, wrap.transform);
            var rt = stem.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(70, 70); // 💡 스템 크기

            // 🎯 기준선보다 약간 띄우기: 음표 머리 중앙에서 spacing 절반 만큼 위로 조정
            float offsetY = -36f;  // 이 값을 줄 간격 절반 정도로 조절

            rt.anchoredPosition = stemDown
                ? new Vector2(-10f, -26f + offsetY)
                : new Vector2(10f, -26f - offsetY);

            rt.localRotation = stemDown ? Quaternion.Euler(0, 0, 180f) : Quaternion.identity;
        }

        // 🎏 4. Flag (깃발 파트 - 8분음표, 16분음표 등에 사용)
        if (flagPrefab)
        {
            // 깃발 프리팹을 note-wrap의 자식으로 생성하여 계층 구조에 포함
            var flag = UnityEngine.Object.Instantiate(flagPrefab, wrap.transform);

            // RectTransform 컴포넌트를 가져와 위치, 크기, 피벗 등을 조절
            var rt = flag.GetComponent<RectTransform>();

            // 앵커와 피벗을 정중앙으로 설정 (부모 기준 정가운데 배치되도록)
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // 🎯 깃발 이름에 따라 크기 설정
            Vector2 flagSize = flagPrefab.name switch
            {
                // 이름에 "8"이 포함되면 8분음표용 크기
                string name when name.Contains("8") => new Vector2(55, 55),
                // 이름에 "16"이 포함되면 16분음표용 크기 (약간 더 세로로 김)
                string name when name.Contains("16") => new Vector2(70, 80),
                // 위 조건에 해당하지 않으면 기본 크기
                _ => new Vector2(40, 40),
            };

            // 🎯 스템이 위일 때의 깃발 위치 오프셋 설정
            Vector2 flagOffsetUp = flagPrefab.name switch
            {
                // 8분음표 깃발은 약간 오른쪽 위
                string name when name.Contains("8") => new Vector2(18f, 20f),
                // 16분음표 깃발은 좀 더 위로
                string name when name.Contains("16") => new Vector2(20f, 20f),
                // 그 외 기본 위치
                _ => new Vector2(20f, 20f),
            };

            // 🎯 스템이 아래로 내려갈 때의 깃발 위치 오프셋 설정
            Vector2 flagOffsetDown = flagPrefab.name switch
            {
                // 8분음표 깃발: 아래쪽 위치이지만 오프셋 값은 양수로 설정 (아래에서 위로 적용)
                string name when name.Contains("8") => new Vector2(2f, 70f),
                // 16분음표 깃발
                string name when name.Contains("16") => new Vector2(0f, 77f),
                // 기본값
                _ => new Vector2(20f, 20f),
            };

            // 위에서 계산한 크기 적용
            rt.sizeDelta = flagSize;

            // 스템 방향에 따라 위치 조정
            // 스템이 아래로 향하면 위치도 반대로 설정 (깃발을 아래로 달아야 하므로 -값 적용)
            rt.anchoredPosition = stemDown
                ? new Vector2(-flagOffsetDown.x, -flagOffsetDown.y)
                : new Vector2(flagOffsetUp.x, flagOffsetUp.y);

            // 🔄 스템이 아래 방향이면 상하 반전 (Y 스케일 -1)
            float scaleY = stemDown ? -1f : 1f;
            rt.localScale = new Vector3(1f, scaleY, 1f);
        }





        // 5. dot (선택)
        if (dotPrefab)
        {
            var dot = UnityEngine.Object.Instantiate(dotPrefab, wrap.transform);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(8, 8); // 💡 점 크기
            rt.anchoredPosition = new Vector2(18f, 0f);
        }

        return wrap;
    }
}
