using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object; // ✅ 이 줄 추가!

public class BeamRenderer
{
    private GameObject beamPrefab;
    private RectTransform parent;

    public BeamRenderer(GameObject beamPrefab, RectTransform parent)
    {
        this.beamPrefab = beamPrefab;
        this.parent = parent;
    }

    public void RenderBeam(List<RectTransform> stems, bool isAbove)
    {
        if (stems == null || stems.Count < 2 || beamPrefab == null) return;

        for (int i = 0; i < stems.Count - 1; i++)
        {
            var start = stems[i];
            var end = stems[i + 1];

            GameObject beam = Object.Instantiate(beamPrefab, parent); // 🔄 모호성 해결됨
            RectTransform rt = beam.GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0.5f);

            Vector2 startPos = start.anchoredPosition;
            Vector2 endPos = end.anchoredPosition;
            Vector2 dir = endPos - startPos;

            rt.anchoredPosition = startPos;
            rt.sizeDelta = new Vector2(dir.magnitude, 6f);
            rt.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        }
    }
}
