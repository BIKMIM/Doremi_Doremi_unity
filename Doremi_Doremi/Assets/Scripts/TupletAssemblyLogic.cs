using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 잇단음표 조립 로직을 담당하는 클래스
/// TupletAssembler의 복잡한 조립 로직을 분리
/// </summary>
public class TupletAssemblyLogic : MonoBehaviour
{
    private TupletAssembler assembler;
    
    private void Awake()
    {
        assembler = GetComponent<TupletAssembler>();
        if (assembler == null)
        {
            assembler = FindObjectOfType<TupletAssembler>();
        }
    }

    /// <summary>
    /// 잇단음표 전체 조립 (숫자 + 코드 기반 beam)
    /// </summary>
    public TupletVisualGroup AssembleTupletGroup(TupletData tupletData, List<GameObject> noteHeads, List<GameObject> stems, float spacing)
    {
        if (assembler.showDebugInfo) Debug.Log($"🎼 === 잇단음표 그룹 조립 시작: {tupletData.GetTupletTypeName()} ===");
        if (assembler.showDebugInfo) Debug.Log($"   noteHeads: {noteHeads.Count}개, stems: {stems.Count}개, spacing: {spacing:F1}");

        if (!tupletData.IsComplete())
        {
            Debug.LogError("❌ 잇단음표 그룹이 완성되지 않았습니다.");
            return null;
        }

        TupletVisualGroup visualGroup = new TupletVisualGroup(tupletData);

        try
        {
            // 1. stem들에서 flag 제거 (잇단음표는 flag 대신 beam 사용)
            if (assembler.showDebugInfo) Debug.Log("🚫 stem에서 flag 제거 중...");
            RemoveFlagsFromStems(stems);

            // 2. stem 끝점들 정확히 찾기
            List<Vector2> stemEndPoints = GetAccurateStemEndPoints(stems, spacing);

            if (stemEndPoints.Count >= 2)
            {
                if (assembler.showDebugInfo) Debug.Log("🌉 코드 기반 beam 생성 중...");

                Vector2 firstStemEnd = stemEndPoints[0];
                Vector2 lastStemEnd = stemEndPoints[stemEndPoints.Count - 1];
                float beamThickness = spacing * assembler.beamThicknessRatio;

                if (assembler.showDebugInfo) Debug.Log($"   실제 stem 끝점들: 첫번째=({firstStemEnd.x:F1}, {firstStemEnd.y:F1}), 마지막=({lastStemEnd.x:F1}, {lastStemEnd.y:F1})");

                GameObject beamObj = assembler.CreateBeamWithCode(firstStemEnd, lastStemEnd, beamThickness);
                visualGroup.beamObject = beamObj;

                if (beamObj != null)
                {
                    if (assembler.showDebugInfo) Debug.Log("✅ 코드 기반 beam 생성 성공");
                }
                else
                {
                    Debug.LogError("❌ 코드 기반 beam 생성 실패");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ stem 끝점이 부족합니다 ({stemEndPoints.Count}개). beam 생성 건너뜀");
            }

            // 3. 잇단음표 숫자 생성 (beam 위에 배치)
            Vector2 numberPos = CalculateNumberPosition(tupletData, spacing);
            if (assembler.showDebugInfo) Debug.Log($"🔢 숫자 위치 계산 완료: ({numberPos.x:F1}, {numberPos.y:F1})");

            GameObject numberObj = assembler.CreateTupletNumber(tupletData.noteCount, numberPos, spacing);
            visualGroup.numberObject = numberObj;

            if (numberObj != null)
            {
                if (assembler.showDebugInfo) Debug.Log($"✅ 숫자 생성 성공: {tupletData.noteCount}");
            }
            else
            {
                Debug.LogError("❌ 숫자 생성 실패");
            }

            // 4. 시각적 그룹 정보 저장
            visualGroup.noteObjects = noteHeads;
            visualGroup.stemObjects = stems;

            if (assembler.showDebugInfo) Debug.Log($"✅ === 잇단음표 시각적 그룹 조립 완료: {tupletData.GetTupletTypeName()} ===");
            if (assembler.showDebugInfo) Debug.Log($"   숫자: {(numberObj != null ? "생성됨" : "실패")}");
            if (assembler.showDebugInfo) Debug.Log($"   beam: {(visualGroup.beamObject != null ? "생성됨" : "실패")}");
            if (assembler.showDebugInfo) Debug.Log($"   음표: {noteHeads.Count}개, stem: {stems.Count}개");

            return visualGroup;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 잇단음표 조립 오류: {e.Message}");
            Debug.LogError($"   StackTrace: {e.StackTrace}");

            // 실패 시 생성된 오브젝트들 정리
            if (visualGroup.numberObject != null)
                DestroyImmediate(visualGroup.numberObject);
            if (visualGroup.beamObject != null)
                DestroyImmediate(visualGroup.beamObject);

            return null;
        }
    }

    /// <summary>
    /// 정확한 stem 끝점들 계산
    /// </summary>
    private List<Vector2> GetAccurateStemEndPoints(List<GameObject> stems, float spacing)
    {
        List<Vector2> endPoints = new List<Vector2>();

        foreach (GameObject stem in stems)
        {
            if (stem == null) continue;

            RectTransform stemRT = stem.GetComponent<RectTransform>();
            if (stemRT == null) continue;

            // stem의 실제 월드 위치 계산
            Vector3[] stemCorners = new Vector3[4];
            stemRT.GetWorldCorners(stemCorners);

            // staffPanel 기준으로 로컬 좌표로 변환
            Vector2 stemBottomLeft = assembler.staffPanel.InverseTransformPoint(stemCorners[0]);
            Vector2 stemTopRight = assembler.staffPanel.InverseTransformPoint(stemCorners[2]);

            // stem 방향 결정
            bool stemUp = IsStemPointingUp(stem);

            Vector2 endPoint;
            // Y축 조정값 계산
            float yAdjustment = spacing * assembler.beamYAdjustmentRatio;

            if (stemUp)
            {
                // stem이 위로: 상단 중앙점
                endPoint = new Vector2((stemBottomLeft.x + stemTopRight.x) * 0.5f, stemTopRight.y);
            }
            else
            {
                // stem이 아래로: 하단 중앙점
                endPoint = new Vector2((stemBottomLeft.x + stemTopRight.x) * 0.5f, stemBottomLeft.y);
            }

            // X 좌표 미세 조정 적용
            float horizontalOffset = spacing * assembler.beamXAdjustmentRatio;

            if (stem == stems[0]) // 첫 번째 stem (가장 왼쪽)
            {
                endPoint.x -= horizontalOffset; // 왼쪽으로 이동하여 beam을 길게
            }
            else if (stem == stems[stems.Count - 1]) // 마지막 stem (가장 오른쪽)
            {
                endPoint.x += horizontalOffset; // 오른쪽으로 이동하여 beam을 길게
            }

            // 최종 Y 좌표에 조정 값 적용
            endPoint.y += yAdjustment;

            endPoints.Add(endPoint);

            if (assembler.showDebugInfo) Debug.Log($"🎯 정확한 stem 끝점: stem 위치=({stemRT.anchoredPosition.x:F1}, {stemRT.anchoredPosition.y:F1}), 월드끝점=({endPoint.x:F1}, {endPoint.y:F1}), 위쪽={stemUp}");
        }

        return endPoints;
    }

    /// <summary>
    /// stem 방향 결정
    /// </summary>
    private bool IsStemPointingUp(GameObject stem)
    {
        // stem의 부모인 note head의 위치로 판단
        Transform noteHead = stem.transform.parent;
        if (noteHead == null) return true;

        Vector2 notePosition = noteHead.GetComponent<RectTransform>().anchoredPosition;

        // Y=0 기준으로 stem 방향 결정 (낮은 음표는 stem 위로)
        return notePosition.y < 0;
    }

    /// <summary>
    /// stem들에서 flag 제거
    /// </summary>
    private void RemoveFlagsFromStems(List<GameObject> stems)
    {
        int removedFlags = 0;
        foreach (GameObject stem in stems)
        {
            if (stem == null) continue;

            // stem의 자식 중에서 flag 찾아서 제거
            for (int i = stem.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = stem.transform.GetChild(i);

                // flag 프리팹인지 확인 (이름으로 판단)
                if (child.name.ToLower().Contains("flag"))
                {
                    if (assembler.showDebugInfo) Debug.Log($"🚫 flag 제거: {child.name}");
                    DestroyImmediate(child.gameObject);
                    removedFlags++;
                }
            }
        }

        if (assembler.showDebugInfo) Debug.Log($"🚫 총 {removedFlags}개의 flag 제거됨");
    }

    /// <summary>
    /// 숫자 위치 계산 - 반응형
    /// </summary>
    private Vector2 CalculateNumberPosition(TupletData tupletData, float spacing)
    {
        float x = tupletData.centerX;

        // beam 위쪽에 배치하되 spacing에 비례하여 높이 설정
        float y = tupletData.maxNoteY + spacing * assembler.numberHeightOffset;

        // 최소 높이 보장 (spacing에 비례)
        float minY = spacing * 2.5f;
        y = Mathf.Max(y, minY);

        if (assembler.showDebugInfo) Debug.Log($"🔢 숫자 위치 계산: x={x:F1}, y={y:F1} (maxNoteY={tupletData.maxNoteY:F1}, spacing={spacing:F1})");

        return new Vector2(x, y);
    }
}
