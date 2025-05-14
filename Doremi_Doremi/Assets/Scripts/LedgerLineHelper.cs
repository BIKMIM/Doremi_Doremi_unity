using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// LedgerLineHelper 클래스는 오선 밖에 위치한 음표에 대해
/// 보조선(덧줄)을 생성하는 헬퍼 기능을 제공합니다.
/// </summary>
public class LedgerLineHelper
{
    private readonly GameObject _ledgerLinePrefab;
    private readonly RectTransform _container;
    private readonly float _yOffsetRatio;

    public LedgerLineHelper(GameObject ledgerLinePrefab, RectTransform container, float yOffsetRatio = -2.1f)
    {
        _ledgerLinePrefab = ledgerLinePrefab;
        _container = container;
        _yOffsetRatio = yOffsetRatio;
    }

    public void GenerateLedgerLines(float noteIndex, float spacing, float x, float baseY, float width)
    {
        // 오선 범위: -2.0f ~ 4.0f (5줄)
        // 덧줄은 오선 밖에 있는 음표에만 필요
        if (noteIndex < -2.0f)
        {
            // 아래 덧줄 (C4와 그보다 낮은 음)
            float lineCount = Mathf.Ceil(Mathf.Abs(noteIndex + 2.0f));
            for (int i = 0; i < lineCount; i++)
            {
                float y = baseY + (-2.0f - i) * spacing;
                CreateLedgerLine(x, y, width);
            }
        }
        else if (noteIndex > 4.0f)
        {
            // 위 덧줄
            float lineCount = Mathf.Ceil(noteIndex - 4.0f);
            for (int i = 0; i < lineCount; i++)
            {
                float y = baseY + (4.0f + i) * spacing;
                CreateLedgerLine(x, y, width);
            }
        }
    }

    private void CreateLedgerLine(float x, float y, float width)
    {
        var line = UnityEngine.Object.Instantiate(_ledgerLinePrefab, _container);
        var rt = line.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(width, 2f);
    }
}
