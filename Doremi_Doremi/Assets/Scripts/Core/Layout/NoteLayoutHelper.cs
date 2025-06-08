// NoteLayoutHelper.cs (최종 수정본)
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Image, Color 사용

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
            // Debug.Log($"🎼 GetLedgerPositions: 오선 아래 음표, noteIndex={noteIndex}");
            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0); // 짝수 인덱스 = 줄 위, 홀수 인덱스 = 줄 사이

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 해당 덧줄부터 오선 바로 아래 덧줄(-6f)까지
                for (float ledgerPos = noteIndex; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 음표보다 위에 있는 덧줄부터 오선 바로 아래 덧줄(-6f)까지
                float upperLedger = Mathf.Ceil(noteIndex / 2f) * 2f; // 가장 가까운 짝수(위쪽 덧줄)
                for (float ledgerPos = upperLedger; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
        }
        else if (noteIndex > 4f) // 오선 위
        {
            // Debug.Log($"🎼 GetLedgerPositions: 오선 위 음표, noteIndex={noteIndex}");
            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0); // 짝수 인덱스 = 줄 위, 홀수 인덱스 = 줄 사이

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 오선 바로 위 덧줄(6f)부터 해당 덧줄까지
                for (float ledgerPos = 6f; ledgerPos <= noteIndex; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 오선 바로 위 덧줄(6f)부터 음표보다 아래에 있는 덧줄까지
                float lowerLedger = Mathf.Floor(noteIndex / 2f) * 2f; // 가장 가까운 짝수(아래쪽 덧줄)
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
        // Object.Instantiate가 null을 반환할 수 있으므로, 인스턴스화 성공 여부 확인
        GameObject ledgerLine = Object.Instantiate(ledgerLinePrefab, staffPanel);
        if (ledgerLine == null)
        {
            Debug.LogError("⚠️ 덧줄 오브젝트 인스턴스화 실패! ledgerLinePrefab이 올바른지 확인하세요.");
            return;
        }

        RectTransform ledgerRT = ledgerLine.GetComponent<RectTransform>();
        if (ledgerRT == null)
        {
            Debug.LogError("⚠️ 덧줄 오브젝트에 RectTransform 컴포넌트가 없습니다!");
            Object.Destroy(ledgerLine); // 컴포넌트가 없으면 삭제
            return;
        }

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

    // 🎼 마디선 생성 함수 (이것은 그대로 두세요)
    public static void CreateBarLine(float xPosition, RectTransform staffPanel, GameObject linePrefab, float staffSpacing)
    {
        if (staffPanel == null || linePrefab == null)
        {
            Debug.LogError("StaffPanel 또는 LinePrefab이 설정되지 않았습니다! 마디선을 그릴 수 없습니다.");
            return;
        }

        float topStaffLineY = 4f * staffSpacing * 0.5f;
        float bottomStaffLineY = -4f * staffSpacing * 0.5f;
        float staffTotalHeight = topStaffLineY - bottomStaffLineY;

        float thickness = MusicLayoutConfig.GetLineThickness(staffPanel);

        GameObject barLine = Object.Instantiate(linePrefab, staffPanel);
        if (barLine == null)
        {
            Debug.LogError("⚠️ 마디선 오브젝트 인스턴스화 실패! linePrefab이 올바른지 확인하세요.");
            return;
        }

        RectTransform barLineRT = barLine.GetComponent<RectTransform>();
        if (barLineRT == null)
        {
            Debug.LogError("⚠️ 마디선 오브젝트에 RectTransform 컴포넌트가 없습니다!");
            Object.Destroy(barLine);
            return;
        }

        barLineRT.sizeDelta = new Vector2(thickness, staffTotalHeight);
        barLineRT.anchorMin = new Vector2(0.5f, 0.5f);
        barLineRT.anchorMax = new Vector2(0.5f, 0.5f);
        barLineRT.pivot = new Vector2(0.5f, 0.5f);
        barLineRT.anchoredPosition = new Vector2(xPosition, 0);

        Image barLineImage = barLine.GetComponent<Image>();
        if (barLineImage != null)
        {
            barLineImage.color = Color.black;
        }

        Debug.Log($"🎼 마디선 생성: X={xPosition:F1}, 높이={staffTotalHeight:F1}, 두께={thickness:F1}");
    }
}