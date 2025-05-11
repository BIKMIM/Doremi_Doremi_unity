using UnityEngine;
using Object = UnityEngine.Object; // Added to resolve Object.Instantiate

/// <summary>
/// LedgerLineHelper 클래스는 오선 밖에 위치한 음표에 대해
/// 보조선(덧줄)을 생성하는 헬퍼 기능을 제공합니다.
/// </summary>
public class LedgerLineHelper
{
    private GameObject ledgerLinePrefab;  // 보조선 프리팹
    private Transform parent;             // 보조선 인스턴스를 부모로 붙일 Transform

    /// <summary>
    /// 생성자: 보조선 프리팹과 부모 Transform을 주입받아 저장합니다.
    /// </summary>
    /// <param name="ledgerLinePrefab">보조선으로 사용할 프리팹</param>
    /// <param name="parent">보조선을 자식으로 붙일 부모 Transform</param>
    public LedgerLineHelper(GameObject ledgerLinePrefab, Transform parent)
    {
        this.ledgerLinePrefab = ledgerLinePrefab;
        this.parent = parent;
    }

    /// <summary>
    /// 음표 높이(index)에 따라 필요한 보조선을 생성합니다.
    /// </summary>
    /// <param name="index">음표의 lineIndex (상대적 위치)</param>
    /// <param name="spacing">오선 간격 (Y 간격)</param>
    /// <param name="posX">보조선의 X 위치</param>
    /// <param name="baseY">기준선(Y0) 위치</param>
    /// <param name="verticalCorrection">추가 Y 오프셋 조정값</param>
    public void GenerateLedgerLines(float index, float spacing, float posX, float baseY, float verticalCorrection)
    {
        Debug.Log($"📌 덧줄 검사 진입: index={index}, spacing={spacing}, posX={posX}, baseY={baseY}, verticalCorrection={verticalCorrection}");

        // NoteMapper의 인덱스 기준:
        // C4 = -0.5f (첫 번째 덧줄 아래)
        // A3 = -1.5f (두 번째 덧줄 아래)
        // F3 = -2.5f (세 번째 덧줄 아래)
        // D3 = -3.5f (네 번째 덧줄 아래)

        // A5 = 5.5f (첫 번째 덧줄 위)
        // C6 = 6.5f (두 번째 덧줄 위)
        // E6 = 7.5f (세 번째 덧줄 위)
        // G6 = 8.5f (네 번째 덧줄 위)

        // Lower ledger lines
        // C4 is index -0.5f. If note is C4 or lower, draw C4 ledger line.
        if (index <= -0.5f)
        {
            CreateLedgerLine(posX, baseY + (-0.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 C4 (-0.5f) 덧줄 생성 for note index={index} → y={baseY + (-0.5f) * spacing + verticalCorrection}");
        }
        // A3 is index -1.5f. If note is A3 or lower, draw A3 ledger line.
        if (index <= -1.5f)
        {
            CreateLedgerLine(posX, baseY + (-1.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 A3 (-1.5f) 덧줄 생성 for note index={index} → y={baseY + (-1.5f) * spacing + verticalCorrection}");
        }
        // F3 is index -2.5f. If note is F3 or lower, draw F3 ledger line.
        if (index <= -2.5f)
        {
            CreateLedgerLine(posX, baseY + (-2.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 F3 (-2.5f) 덧줄 생성 for note index={index} → y={baseY + (-2.5f) * spacing + verticalCorrection}");
        }
        // D3 is index -3.5f. If note is D3 or lower, draw D3 ledger line.
        if (index <= -3.5f)
        {
            CreateLedgerLine(posX, baseY + (-3.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 D3 (-3.5f) 덧줄 생성 for note index={index} → y={baseY + (-3.5f) * spacing + verticalCorrection}");
        }
        // Add more for lower notes if needed (e.g., B2 = -4.5f, G2 = -5.0f)


        // Upper ledger lines
        // A5 is index 5.5f. If note is A5 or higher, draw A5 ledger line.
        if (index >= 5.5f)
        {
            CreateLedgerLine(posX, baseY + (5.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 A5 (5.5f) 덧줄 생성 for note index={index} → y={baseY + (5.5f) * spacing + verticalCorrection}");
        }
        // C6 is index 6.5f. If note is C6 or higher, draw C6 ledger line.
        if (index >= 6.5f)
        {
            CreateLedgerLine(posX, baseY + (6.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 C6 (6.5f) 덧줄 생성 for note index={index} → y={baseY + (6.5f) * spacing + verticalCorrection}");
        }
        // E6 is index 7.5f (if in NoteMapper). If note is E6 or higher, draw E6 ledger line.
        if (index >= 7.5f)
        {
            CreateLedgerLine(posX, baseY + (7.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 E6 (7.5f) 덧줄 생성 for note index={index} → y={baseY + (7.5f) * spacing + verticalCorrection}");
        }
        // G6 is index 8.5f (if in NoteMapper). If note is G6 or higher, draw G6 ledger line.
        if (index >= 8.5f)
        {
            CreateLedgerLine(posX, baseY + (8.5f) * spacing + verticalCorrection);
            Debug.Log($"🧾 G6 (8.5f) 덧줄 생성 for note index={index} → y={baseY + (8.5f) * spacing + verticalCorrection}");
        }
        // Add more for higher notes if needed
    }

    /// <summary>
    /// 보조선 프리팹을 인스턴스화하고, RectTransform 위치를 설정합니다.
    /// </summary>
    /// <param name="x">보조선의 X 좌표</param>
    /// <param name="y">보조선의 Y 좌표</param>
    private void CreateLedgerLine(float x, float y)
    {
        GameObject ledgerLine = Object.Instantiate(ledgerLinePrefab, parent);
        RectTransform rt = ledgerLine.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, Mathf.Round(y)); // Mathf.Round(y) is good for pixel-perfect alignment
        // rt.sizeDelta = new Vector2(30f, rt.sizeDelta.y); // Optional: Adjust width if needed
    }
}