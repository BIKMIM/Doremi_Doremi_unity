using System.Collections.Generic;

public class KeySignatureRenderer
{
    private readonly Dictionary<int, Vector2[]> sharpPositions = new()
    {
        [1] = new[] { new Vector2(0, 0) },
        [2] = new[] { new Vector2(0, 0), new Vector2(10, 20) },
        [3] = new[] { new Vector2(0, 0), new Vector2(10, 20), new Vector2(20, -10) },
    };

    private readonly Dictionary<int, Vector2[]> flatPositions = new()
    {
        [1] = new[] { new Vector2(0, 0) },
        [2] = new[] { new Vector2(0, 0), new Vector2(10, -20) },
        [3] = new[] { new Vector2(0, 0), new Vector2(10, -20), new Vector2(20, 10) },
    };

    private readonly RectTransform parent;
    private readonly GameObject sharpPrefab;
    private readonly GameObject flatPrefab;
    private readonly Vector2 basePosition;
    private readonly float staffHeight;

    public KeySignatureRenderer(
        RectTransform parent,
        GameObject sharpPrefab,
        GameObject flatPrefab,
        Vector2 basePosition,
        float staffHeight)
    {
        this.parent = parent;
        this.sharpPrefab = sharpPrefab;
        this.flatPrefab = flatPrefab;
        this.basePosition = basePosition;
        this.staffHeight = staffHeight;
    }

    public void Spawn(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        GameObject prefab = null;
        Dictionary<int, Vector2[]> positionMap = null;

        if (key.StartsWith("sharp"))
        {
            prefab = sharpPrefab;
            positionMap = sharpPositions;
        }
        else if (key.StartsWith("flat"))
        {
            prefab = flatPrefab;
            positionMap = flatPositions;
        }
        else
        {
            Debug.LogWarning($"[KeySignature] 알 수 없는 key signature: {key}");
            return;
        }

        if (!int.TryParse(key.Substring(key.IndexOfAny("0123456789".ToCharArray())), out int count))
        {
            Debug.LogWarning($"[KeySignature] 조표 개수 파싱 실패: {key}");
            return;
        }

        if (prefab == null || !positionMap.ContainsKey(count)) return;

        float spacing = staffHeight / 4f;

        foreach (var offset in positionMap[count])
        {
            var go = UnityEngine.Object.Instantiate(prefab, parent);
            var rt = go.GetComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = basePosition + offset;
            rt.sizeDelta = new Vector2(30f, 60f);
        }
    }
}
