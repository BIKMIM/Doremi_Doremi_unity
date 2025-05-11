using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// LedgerLineHelper 클래스는 오선 밖에 위치한 음표에 대해
/// 보조선(덧줄)을 생성하는 헬퍼 기능을 제공합니다.
/// </summary>
public class LedgerLineHelper
{
    private readonly GameObject ledgerLinePrefab;
    private readonly Transform parent;
    private readonly float yOffsetRatio; // 덧줄 전용 Y 오프셋 비율

    public LedgerLineHelper(GameObject ledgerLinePrefab, Transform parent, float yOffsetRatio = 0f)
    {
        this.ledgerLinePrefab = ledgerLinePrefab;
        this.parent = parent;
        this.yOffsetRatio = yOffsetRatio;
    }

    /// <summary>
    /// index: 음표의 위치 (NoteMapper 기준), spacing: 오선 간격
    /// posX: 덧줄의 X 좌표, baseY: 기준 Y 위치
    /// </summary>
    public void GenerateLedgerLines(float index, float spacing, float posX, float baseY, float verticalCorrection)
    {
        float offsetY = spacing * yOffsetRatio;

        if (index <= -0.5f)
            CreateLedgerLine(posX, baseY + (-0.5f) * spacing + verticalCorrection + offsetY);

        if (index <= -1.5f)
            CreateLedgerLine(posX, baseY + (-1.5f) * spacing + verticalCorrection + offsetY);

        if (index <= -2.5f)
            CreateLedgerLine(posX, baseY + (-2.5f) * spacing + verticalCorrection + offsetY);

        if (index <= -3.5f)
            CreateLedgerLine(posX, baseY + (-3.5f) * spacing + verticalCorrection + offsetY);

        if (index >= 5.5f)
            CreateLedgerLine(posX, baseY + (5.5f) * spacing + verticalCorrection + offsetY);

        if (index >= 6.5f)
            CreateLedgerLine(posX, baseY + (6.5f) * spacing + verticalCorrection + offsetY);

        if (index >= 7.5f)
            CreateLedgerLine(posX, baseY + (7.5f) * spacing + verticalCorrection + offsetY);

        if (index >= 8.5f)
            CreateLedgerLine(posX, baseY + (8.5f) * spacing + verticalCorrection + offsetY);
    }

    private void CreateLedgerLine(float x, float y)
    {
        GameObject ledgerLine = Object.Instantiate(ledgerLinePrefab, parent);
        RectTransform rt = ledgerLine.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, Mathf.Round(y));
    }
}
