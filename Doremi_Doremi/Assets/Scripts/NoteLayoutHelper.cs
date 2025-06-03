using UnityEngine;
using System.Collections.Generic;

// NoteLayoutHelper.cs - 음표 배치와 레이아웃을 담당하는 헬퍼 클래스
public static class NoteLayoutHelper
{
    // 🎼 덧줄이 필요한 음표들 (오선 범위 밖)
    public static bool NeedsLedgerLines(float noteIndex)
    {
        // 오선 범위: E4(-4) ~ F5(4), 즉 -4 ~ 4 사이는 덧줄 불필요
        return noteIndex < -4f || noteIndex > 4f;
    }

    // 🎼 덧줄 위치 계산
    public static List<float> GetLedgerPositions(float noteIndex)
    {
        List<float> ledgerPositions = new List<float>();

        if (noteIndex < -4f) // 오선 아래
        {
            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0);

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 해당 덧줄부터 위쪽 모든 덧줄
                for (float ledgerPos = noteIndex; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 위쪽 덧줄만
                float upperLedger = Mathf.Ceil(noteIndex / 2f) * 2f;
                for (float ledgerPos = upperLedger; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
        }
        else if (noteIndex > 4f) // 오선 위
        {
            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0);

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 6부터 해당 덧줄까지
                for (float ledgerPos = 6f; ledgerPos <= noteIndex; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 아래쪽 덧줄부터
                float lowerLedger = Mathf.Floor(noteIndex / 2f) * 2f;
                for (float ledgerPos = 6f; ledgerPos <= lowerLedger; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
        }

        return ledgerPositions;
    }

    // 🎼 개별 덧줄 생성
    public static void CreateSingleLedgerLine(float x, float ledgerIndex, float staffSpacing, 
        RectTransform staffPanel, GameObject ledgerLinePrefab)
    {
        GameObject ledgerLine = Object.Instantiate(ledgerLinePrefab, staffPanel);
        RectTransform ledgerRT = ledgerLine.GetComponent<RectTransform>();

        float ledgerWidth = staffSpacing * 1.6f;
        float ledgerThickness = MusicLayoutConfig.GetLineThickness(staffPanel);

        ledgerRT.sizeDelta = new Vector2(ledgerWidth, ledgerThickness);

        ledgerRT.anchorMin = new Vector2(0.5f, 0.5f);
        ledgerRT.anchorMax = new Vector2(0.5f, 0.5f);
        ledgerRT.pivot = new Vector2(0.5f, 0.5f);

        float ledgerY = ledgerIndex * staffSpacing * 0.5f;
        ledgerRT.anchoredPosition = new Vector2(x, ledgerY);

        UnityEngine.UI.Image ledgerImage = ledgerLine.GetComponent<UnityEngine.UI.Image>();
        if (ledgerImage != null)
        {
            ledgerImage.color = Color.black;
        }

        Debug.Log($"   → 덧줄: 인덱스={ledgerIndex}, Y={ledgerY:F1}, 크기={ledgerWidth:F1}x{ledgerThickness:F1}");
    }
}