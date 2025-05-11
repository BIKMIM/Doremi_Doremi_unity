using System;
using UnityEngine;

/// <summary>
/// ClefRenderer는 악보의 음자리표(높은음자리표, 낮은음자리표)를 생성합니다.
/// </summary>
public class ClefRenderer
{
    public void Spawn(
        string clefType,
        GameObject trebleClefPrefab,
        GameObject bassClefPrefab,
        RectTransform parent,
        Vector2 treblePosition,
        Vector2 trebleSize,
        Vector2 bassPosition,
        Vector2 bassSize)
    {
        GameObject prefab = clefType == "Bass" ? bassClefPrefab : trebleClefPrefab;

        if (prefab == null)
        {
            Debug.LogWarning("[ClefRenderer] Clef 프리팹이 비어 있음: " + clefType);
            return;
        }

        var clef = Object.Instantiate(prefab, parent);
        var rt = clef.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        if (clefType == "Bass")
        {
            rt.anchoredPosition = bassPosition;
            rt.sizeDelta = bassSize;
        }
        else
        {
            rt.anchoredPosition = treblePosition;
            rt.sizeDelta = trebleSize;
        }
    }
}
