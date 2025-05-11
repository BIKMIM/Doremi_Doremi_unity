using System;

public class TimeSignatureRenderer
{
    private readonly RectTransform parent;
    private readonly GameObject time2_4;
    private readonly GameObject time3_4;
    private readonly GameObject time4_4;
    private readonly GameObject time3_8;
    private readonly GameObject time4_8;
    private readonly GameObject time6_8;

    public TimeSignatureRenderer(RectTransform parent, TimeSignaturePrefabs prefabs)
    {
        this.parent = parent;
        this.time2_4 = prefabs.time2_4Prefab;
        this.time3_4 = prefabs.time3_4Prefab;
        this.time4_4 = prefabs.time4_4Prefab;
        this.time3_8 = prefabs.time3_8Prefab;
        this.time4_8 = prefabs.time4_8Prefab;
        this.time6_8 = prefabs.time6_8Prefab;
    }

    public void Spawn(string time)
    {
        GameObject prefab = time switch
        {
            "2/4" => time2_4,
            "3/4" => time3_4,
            "4/4" => time4_4,
            "3/8" => time3_8,
            "4/8" => time4_8,
            "6/8" => time6_8,
            _ => null
        };

        if (prefab == null)
        {
            Debug.LogWarning($"[TimeSignatureRenderer] ❗ 등록되지 않은 박자표: {time}");
            return;
        }

        var obj = Object.Instantiate(prefab, parent);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = GetPosition(time);
        rt.sizeDelta = GetSize(time);
    }

    private Vector2 GetPosition(string time) => new Vector2(100f, 0f);

    private Vector2 GetSize(string time) => time switch
    {
        "2/4" => new Vector2(40f, 90f),
        "3/4" => new Vector2(42f, 90f),
        "4/4" => new Vector2(44f, 90f),
        "3/8" => new Vector2(38f, 80f),
        "4/8" => new Vector2(40f, 80f),
        "6/8" => new Vector2(46f, 90f),
        _ => new Vector2(40f, 90f)
    };
}
