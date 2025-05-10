using UnityEngine;

/// <summary>
/// 🎵 음표의 덧줄(ledger line)을 생성하는 보조 클래스
/// </summary>
public class LedgerLineHelper
{
    private GameObject ledgerLinePrefab;   // 덧줄에 사용할 프리팹
    private RectTransform parent;          // 덧줄을 배치할 부모 오브젝트

    public LedgerLineHelper(GameObject prefab, RectTransform container)
    {
        ledgerLinePrefab = prefab;
        parent = container;
    }

    /// <summary>
    /// 덧줄이 필요한 경우(오선 밖의 높은/낮은 음표)에만 생성
    /// </summary>
    public void GenerateLedgerLines(float index, float baseY, float spacing, float x, float offsetY)
    {
        if (index <= -1f)
        {
            for (float ledger = index; ledger <= -1f; ledger += 1f)
                CreateLedgerLine(ledger, baseY, spacing, x, offsetY);
        }
        else if (index >= 4f)
        {
            for (float ledger = index; ledger >= 4f; ledger -= 1f)
                CreateLedgerLine(ledger, baseY, spacing, x, offsetY);
        }
    }

    /// <summary>
    /// 덧줄 1개 생성
    /// </summary>
    private void CreateLedgerLine(float ledger, float baseY, float spacing, float x, float offsetY)
    {
        GameObject line = UnityEngine.Object.Instantiate(ledgerLinePrefab, parent);
        RectTransform rt = line.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0.5f);

        float y = baseY + ledger * spacing + offsetY;
        if (ledger % 1 != 0)
            y += (ledger >= 4f ? -spacing / 2f : spacing / 2f);

        rt.anchoredPosition = new Vector2(x, Mathf.Round(y));
    }
}
