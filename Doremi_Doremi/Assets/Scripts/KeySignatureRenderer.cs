using System.Collections.Generic;
using UnityEngine;

public class KeySignatureRenderer
{
    private readonly GameObject sharpPrefab;
    private readonly GameObject flatPrefab;
    private readonly Transform parent;
    private readonly float spacing;
    private readonly float baseY;

    public KeySignatureRenderer(GameObject sharpPrefab, GameObject flatPrefab, Transform parent, float spacing, float baseY)
    {
        this.sharpPrefab = sharpPrefab;
        this.flatPrefab = flatPrefab;
        this.parent = parent;
        this.spacing = spacing;
        this.baseY = baseY;
    }

    public void Render(string key)
    {
        Debug.Log($"[KeySig] INPUT key: '{key}'");
        List<string> accidentals = KeySignatureHelper.GetAccidentals(key);
        Debug.Log($"[KeySig] accidentals.Count = {accidentals.Count}");

        if (accidentals.Count == 0) return;

        for (int i = 0; i < accidentals.Count; i++)
        {
            string acc = accidentals[i];
            string type = acc[^1..];

            GameObject prefab = type == "#" ? sharpPrefab : flatPrefab;
            if (prefab == null) continue;

            var obj = UnityEngine.Object.Instantiate(prefab, parent);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            float clefRightEdge = 70f + 140f; // clef 위치 + clef width
            float keySigSpacing = spacing * 0.7f; // or simply 25~30f
            float x = clefRightEdge + i * keySigSpacing;

            float index = NoteMapper.GetKeySignatureIndex(acc);
            float y = baseY + index * spacing;

            rt.anchoredPosition = new Vector2(x, y);
            rt.localScale = Vector3.one;

            obj.name = $"KeySig_{acc}";
            Debug.Log($"[KeySig] ✅ Created {obj.name} at anchoredY: {y}");

            var img = obj.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = Color.red;
            }

        }
      
    }

}
