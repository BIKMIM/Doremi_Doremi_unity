using System;
using UnityEngine;
using Object = UnityEngine.Object;



/// <summary>
/// 음표의 보조선(덧줄)을 생성하는 헬퍼 클래스
/// </summary>
public class LedgerLineHelper
{
    private GameObject ledgerLinePrefab;
    private Transform parent;

    public LedgerLineHelper(GameObject ledgerLinePrefab, Transform parent)
    {
        this.ledgerLinePrefab = ledgerLinePrefab;
        this.parent = parent;
    }

    /// <summary>
    /// 음표 위치에 따라 필요한 보조선을 생성합니다
    /// </summary>
    /// <param name="index">음표의 높이 인덱스</param>
    /// <param name="baseY">기준 Y 위치</param>
    /// <param name="spacing">선 간격</param>
    /// <param name="posX">X 위치</param>
    /// <param name="yOffset">미세 Y 위치 조정값</param>
    public void GenerateLedgerLines(float index, float baseY, float spacing, float posX, float yOffset)
    {
        // 오선 범위 설정: 오선은 -3 ~ 1 범위 (E4 ~ F5)
        float minStaffLine = -3.0f;  // E4 (첫 번째 오선)
        float maxStaffLine = 1.0f;   // F5 (다섯 번째 오선)

        // C4, D4는 아래 보조선 필요
        if (index <= -3.5f) // C4, D4
        {
            // 보조선 1 (C4, D4 모두)
            CreateLedgerLine(posX, baseY + (-3.0f * spacing) + yOffset);

            // 보조선 2 (C4만)
            if (index <= -4.0f)
            {
                CreateLedgerLine(posX, baseY + (-4.0f * spacing) + yOffset);
            }
        }

        // G5 이상은 위 보조선 필요
        if (index >= 1.5f) // G5 이상
        {
            CreateLedgerLine(posX, baseY + (2.0f * spacing) + yOffset);
        }
    }

    private void CreateLedgerLine(float x, float y)
    {

        // ✅ 수정 코드
        GameObject ledgerLine = UnityEngine.Object.Instantiate(ledgerLinePrefab, parent);

        RectTransform rt = ledgerLine.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, y);
    }
}