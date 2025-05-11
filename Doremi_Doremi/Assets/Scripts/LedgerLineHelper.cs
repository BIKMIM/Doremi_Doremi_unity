using System;  // 기본 .NET 네임스페이스
using UnityEngine;  // Unity 엔진 핵심 기능 제공
using Object = UnityEngine.Object;  // DestroyImmediate 등을 위한 명시적 참조

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
        this.ledgerLinePrefab = ledgerLinePrefab;  // 멤버 변수에 프리팹 할당
        this.parent = parent;                     // 멤버 변수에 부모 Transform 할당
    }

    /// <summary>
    /// 음표 높이(index)에 따라 필요한 보조선을 생성합니다.
    /// </summary>
    /// <param name="index">음표의 lineIndex (상대적 위치)</param>
    /// <param name="baseY">기준선(Y0) 위치</param>
    /// <param name="spacing">오선 간격 (Y 간격)
    /// <param name="posX">보조선의 X 위치</param>
    /// <param name="yOffset">추가 Y 오프셋 조정값</param>
    public void GenerateLedgerLines(float index, float baseY, float spacing, float posX, float yOffset)
    {

        // 오선 범위: E4(-3) ~ F5(1)
        float minStaffLine = -0.5f;  // E4 라인 인덱스
        float maxStaffLine = 3.5f;   // F5 라인 인덱스

        // 아래 보조선: D4(–1.0) 혹은 그 아래
        if (index <= -1.5f)
        {
            // 첫 번째 보조선(E4) at index -1.0
            CreateLedgerLine(posX, baseY + (-1.5f * spacing) + yOffset);
            // 두 번째 보조선(C4) at index -2.0
            if (index <= -2.5f)
                CreateLedgerLine(posX, baseY + (-2.5f * spacing) + yOffset);
            // 두 번째 보조선(C4) at index -2.0
            if (index <= -3.5f)
                CreateLedgerLine(posX, baseY + (-3.5f * spacing) + yOffset);
        }

        // 위 보조선: G5(3.5) 혹은 그 위
        if (index >= 4.5f)
        {
            // 첫 번째 보조선(F5) at index 3.0
            CreateLedgerLine(posX, baseY + (4.5f * spacing) + yOffset);
            // (필요하다면 G5 위에 두 번째 보조선도)
            // 두 번째 보조선(C4) at index -2.0
            if (index >= 5.5f)
                CreateLedgerLine(posX, baseY + (5.5f * spacing) + yOffset);
            // 세 번째 보조선(C4) at index -2.0
            if (index >= 6.5f)
                CreateLedgerLine(posX, baseY + (6.5f * spacing) + yOffset);
        }

    }

    /// <summary>
    /// 보조선 프리팹을 인스턴스화하고, RectTransform 위치를 설정합니다.
    /// </summary>
    /// <param name="x">보조선의 X 좌표</param>
    /// <param name="y">보조선의 Y 좌표</param>
    private void CreateLedgerLine(float x, float y)
    {
        // 보조선 게임오브젝트 인스턴스화
        GameObject ledgerLine = Object.Instantiate(ledgerLinePrefab, parent);

        // RectTransform 컴포넌트를 가져와 위치 설정
        RectTransform rt = ledgerLine.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, y);
    }
}
