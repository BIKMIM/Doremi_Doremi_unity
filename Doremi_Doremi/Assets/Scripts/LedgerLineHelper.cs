using UnityEngine;
using Object = UnityEngine.Object; // Added to resolve Object.Instantiate

public class LedgerLineHelper
{
    private GameObject ledgerLinePrefab;
    private Transform parent;

    public LedgerLineHelper(GameObject ledgerLinePrefab, Transform parent)
    {
        this.ledgerLinePrefab = ledgerLinePrefab;
        this.parent = parent;
    }

    public void GenerateLedgerLines(float currentNoteIndex, float spacing, float posX, float baseY_param, float verticalCorrection_param)
    {
        Debug.Log($"📌 덧줄 검사 진입: currentNoteIndex={currentNoteIndex}, spacing={spacing}, posX={posX}, baseY_param={baseY_param}, verticalCorrection_param={verticalCorrection_param}");

        // NoteMapper의 인덱스 기준:
        // C4 = -0.5f (첫 번째 덧줄 아래)
        // A3 = -1.5f (두 번째 덧줄 아래)
        // F3 = -2.5f (세 번째 덧줄 아래)
        // D3 = -3.5f (네 번째 덧줄 아래)

        // A5 = 5.5f (첫 번째 덧줄 위)
        // C6 = 6.5f (두 번째 덧줄 위)
        // E6 = 7.5f (세 번째 덧줄 위) - NoteMapper에 있다면
        // G6 = 8.5f (네 번째 덧줄 위) - NoteMapper에 있다면

        // 아래 덧줄 (Lower ledger lines)
        // C4 덧줄 (음표 인덱스 -0.5f)
        if (currentNoteIndex <= -0.5f)
        {
            CreateLedgerLine(posX, baseY_param + (-0.5f * spacing) + verticalCorrection_param);
            // Debug.Log($"🧾 C4 (-0.5f) 덧줄 생성 for note index={currentNoteIndex}");
        }
        // A3 덧줄 (음표 인덱스 -1.5f)
        if (currentNoteIndex <= -1.5f)
        {
            CreateLedgerLine(posX, baseY_param + (-1.5f * spacing) + verticalCorrection_param);
            // Debug.Log($"🧾 A3 (-1.5f) 덧줄 생성 for note index={currentNoteIndex}");
        }
        // F3 덧줄 (음표 인덱스 -2.5f)
        if (currentNoteIndex <= -2.5f)
        {
            CreateLedgerLine(posX, baseY_param + (-2.5f * spacing) + verticalCorrection_param);
            // Debug.Log($"🧾 F3 (-2.5f) 덧줄 생성 for note index={currentNoteIndex}");
        }
        // D3 덧줄 (음표 인덱스 -3.5f)
        if (currentNoteIndex <= -3.5f)
        {
            CreateLedgerLine(posX, baseY_param + (-3.5f * spacing) + verticalCorrection_param);
            // Debug.Log($"🧾 D3 (-3.5f) 덧줄 생성 for note index={currentNoteIndex}");
        }
        // (필요시 더 낮은 음에 대한 덧줄 추가)

        // 위 덧줄 (Upper ledger lines)
        // A5 덧줄 (음표 인덱스 5.5f)
        if (currentNoteIndex >= 5.5f)
        {
            CreateLedgerLine(posX, baseY_param + (5.5f * spacing) + verticalCorrection_param);
            // Debug.Log($"🧾 A5 (5.5f) 덧줄 생성 for note index={currentNoteIndex}");
        }
        // C6 덧줄 (음표 인덱스 6.5f)
        if (currentNoteIndex >= 6.5f)
        {
            CreateLedgerLine(posX, baseY_param + (6.5f * spacing) + verticalCorrection_param);
            // Debug.Log($"🧾 C6 (6.5f) 덧줄 생성 for note index={currentNoteIndex}");
        }
        // (필요시 E6(7.5f), G6(8.5f) 등 더 높은 음에 대한 덧줄 추가)
    }

    private void CreateLedgerLine(float x, float y)
    {
        GameObject ledgerLine = Object.Instantiate(ledgerLinePrefab, parent, false);
        RectTransform rt = ledgerLine.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, Mathf.Round(y));
        // rt.sizeDelta = new Vector2(30f, rt.sizeDelta.y); // 필요시 덧줄 너비 조정
    }
}