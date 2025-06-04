using UnityEngine; // Unity 핵심 기능
using UnityEngine.UI; // UI 관련 기능 (Image, RectTransform 등)
using System.Collections.Generic; // List<T> 사용을 위함

// 참고: System.Diagnostics; 및 System.Net.Mime.MediaTypeNames; System; 는 이 스크립트에서 필요하지 않으며
// 이름 충돌 오류를 유발할 수 있으므로 반드시 제거해야 합니다.

// 잇단음표 시각적 요소 생성 및 조립을 담당하는 클래스 - 코드 기반 beam 생성
public class TupletAssembler : MonoBehaviour
{
    [Header("오선 패널")]
    public RectTransform staffPanel;

    [Header("숫자 프리팹 (num0 ~ num9)")]
    public GameObject num0Prefab;
    public GameObject num1Prefab;
    public GameObject num2Prefab;
    public GameObject num3Prefab; // ⭐ 셋잇단음표용
    public GameObject num4Prefab; // ⭐ 넷잇단음표용  
    public GameObject num5Prefab; // ⭐ 다섯잇단음표용
    public GameObject num6Prefab;
    public GameObject num7Prefab;
    public GameObject num8Prefab;
    public GameObject num9Prefab;

    [Header("잇단음표 설정 - 반응형")]
    [Range(1.0f, 4.0f)]
    public float numberSizeRatio = 2.0f; // 숫자 크기 비율 (spacing 대비)

    [Range(0.05f, 0.6f)] // beam 두께 비율 범위 확대
    public float beamThicknessRatio = 0.5f; // beam 두께 비율 (spacing 대비)

    [Range(0.8f, 2.0f)]
    public float numberHeightOffset = 1.5f; // 숫자 높이 오프셋 (spacing 배수)

    // ⭐ 추가: Beam의 Y축 미세 조정을 위한 public 변수 (Inspector에서 조절 가능)
    [Header("Beam Y 미세 조정")]
    [Range(-0.5f, 0.5f)] // 적절한 범위로 설정, 필요에 따라 더 넓힐 수 있습니다.
    public float beamYAdjustmentRatio = 0f; // 기본값 0

    [Header("Beam X 미세 조정")] // ⭐ 추가: Beam의 X축 미세 조정을 위한 public 변수
    [Range(0.0f, 0.5f)] // 0.0f이면 스템 중앙, 클수록 스템 바깥쪽
    public float beamXAdjustmentRatio = 0.2f; // 기본값 0.2f (spacing 대비)


    [Header("Beam 색상")]
    public Color beamColor = Color.black;

    [Header("디버그")]
    public bool showDebugInfo = true;

    // 내부에서 사용할 배열 (자동 생성)
    private GameObject[] numberPrefabs;

    void Awake()
    {
        // 개별 프리팹들을 배열로 구성
        numberPrefabs = new GameObject[10]
        {
            num0Prefab, num1Prefab, num2Prefab, num3Prefab, num4Prefab,
            num5Prefab, num6Prefab, num7Prefab, num8Prefab, num9Prefab
        };

        Debug.Log("🎼 TupletAssembler 초기화 완료 (코드 기반 beam)");
        ValidatePrefabs();
    }

    // 잇단음표 숫자 생성 - 반응형
    public GameObject CreateTupletNumber(int number, Vector2 position, float spacing)
    {
        if (showDebugInfo) Debug.Log($"🔢 잇단음표 숫자 생성 시도: {number}, 위치=({position.x:F1}, {position.y:F1}), spacing={spacing:F1}");

        if (number < 0 || number >= numberPrefabs.Length)
        {
            Debug.LogError($"❌ 지원되지 않는 숫자: {number} (0~9만 가능)");
            return null;
        }

        GameObject prefab = numberPrefabs[number];
        if (prefab == null)
        {
            Debug.LogError($"❌ 숫자 {number} 프리팹이 할당되지 않았습니다!");
            return null;
        }

        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 null입니다!");
            return null;
        }

        if (showDebugInfo) Debug.Log($"📦 프리팹 {prefab.name}을 사용하여 숫자 {number} 생성");

        GameObject numberObj = Instantiate(prefab, staffPanel);
        RectTransform rt = numberObj.GetComponent<RectTransform>();

        if (rt == null)
        {
            Debug.LogError($"❌ 숫자 프리팹에 RectTransform이 없습니다: {prefab.name}");
            DestroyImmediate(numberObj);
            return null;
        }

        // 앵커 및 피벗 설정
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // 위치 설정
        rt.anchoredPosition = position;

        // 크기 설정 - 반응형 (spacing 기반)
        float numberSize = spacing * numberSizeRatio;
        rt.sizeDelta = new Vector2(numberSize, numberSize);
        rt.localScale = Vector3.one;

        // 이름 설정 (디버그용)
        numberObj.name = $"TupletNumber_{number}";

        if (showDebugInfo) Debug.Log($"✅ 잇단음표 숫자 {number} 생성 완료: 위치=({position.x:F1}, {position.y:F1}), 크기={numberSize:F1}");

        return numberObj;
    }

    // 코드로 beam 생성 - Image 컴포넌트 활용
    public GameObject CreateBeamWithCode(Vector2 startPos, Vector2 endPos, float thickness)
    {
        if (showDebugInfo)
        {
            Debug.Log($"🌉 코드 기반 beam 생성: ({startPos.x:F1}, {startPos.y:F1}) → ({endPos.x:F1}, {endPos.y:F1}), 두께={thickness:F2}");
            Debug.Log($"🌉 TA: Received startPos=({startPos.x:F1}, {startPos.y:F1}), endPos=({endPos.x:F1}, {endPos.y:F1}), 두께={thickness:F2}");
        }

        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 null입니다!");
            return null;
        }

        // beam GameObject 생성
        GameObject beamObj = new GameObject("TupletBeam_Code");
        beamObj.transform.SetParent(staffPanel, false);

        // RectTransform 추가
        RectTransform rt = beamObj.AddComponent<RectTransform>();

        // CanvasRenderer 추가 (UI 렌더링용)
        beamObj.AddComponent<CanvasRenderer>();

        // Image 컴포넌트 추가
        Image beamImage = beamObj.AddComponent<Image>();
        beamImage.color = beamColor;

        // beam의 길이와 각도 계산
        Vector2 direction = endPos - startPos;
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 앵커 및 피벗 설정
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f); // 왼쪽 중앙 기준

        // 위치, 크기, 회전 설정
        rt.anchoredPosition = startPos;
        rt.sizeDelta = new Vector2(length, thickness);
        rt.rotation = Quaternion.Euler(0, 0, angle);
        rt.localScale = Vector3.one;

        if (showDebugInfo)
        {
            Debug.Log($"✅ 코드 기반 beam 생성 완료: 길이={length:F1}, 각도={angle:F1}°, 두께={thickness:F2}");
            Debug.Log($"   시작점=({startPos.x:F1}, {startPos.y:F1}), 끝점=({endPos.x:F1}, {endPos.y:F1})");
            Debug.Log($"🌉 TA: Calculated length={length:F1}, angle={angle:F1}°");
        }
        return beamObj;
    }

    // 잇단음표 전체 조립 (숫자 + 코드 기반 beam)
    public TupletVisualGroup AssembleTupletGroup(TupletData tupletData, List<GameObject> noteHeads, List<GameObject> stems, float spacing)
    {
        if (showDebugInfo) Debug.Log($"🎼 === 잇단음표 그룹 조립 시작: {tupletData.GetTupletTypeName()} ===");
        if (showDebugInfo) Debug.Log($"   noteHeads: {noteHeads.Count}개, stems: {stems.Count}개, spacing: {spacing:F1}");

        if (!tupletData.IsComplete())
        {
            Debug.LogError("❌ 잇단음표 그룹이 완성되지 않았습니다.");
            return null;
        }

        TupletVisualGroup visualGroup = new TupletVisualGroup(tupletData);

        try
        {
            // 1. stem들에서 flag 제거 (잇단음표는 flag 대신 beam 사용)
            if (showDebugInfo) Debug.Log("🚫 stem에서 flag 제거 중...");
            RemoveFlagsFromStems(stems);

            // 2. stem 끝점들 정확히 찾기
            List<Vector2> stemEndPoints = GetAccurateStemEndPoints(stems, spacing);

            if (stemEndPoints.Count >= 2)
            {
                if (showDebugInfo) Debug.Log("🌉 코드 기반 beam 생성 중...");

                Vector2 firstStemEnd = stemEndPoints[0];
                Vector2 lastStemEnd = stemEndPoints[stemEndPoints.Count - 1];
                float beamThickness = spacing * beamThicknessRatio;

                if (showDebugInfo) Debug.Log($"   실제 stem 끝점들: 첫번째=({firstStemEnd.x:F1}, {firstStemEnd.y:F1}), 마지막=({lastStemEnd.x:F1}, {lastStemEnd.y:F1})");

                GameObject beamObj = CreateBeamWithCode(firstStemEnd, lastStemEnd, beamThickness);
                visualGroup.beamObject = beamObj;

                if (beamObj != null)
                {
                    if (showDebugInfo) Debug.Log("✅ 코드 기반 beam 생성 성공");
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
            if (showDebugInfo) Debug.Log($"🔢 숫자 위치 계산 완료: ({numberPos.x:F1}, {numberPos.y:F1})");

            GameObject numberObj = CreateTupletNumber(tupletData.noteCount, numberPos, spacing);
            visualGroup.numberObject = numberObj;

            if (numberObj != null)
            {
                if (showDebugInfo) Debug.Log($"✅ 숫자 생성 성공: {tupletData.noteCount}");
            }
            else
            {
                Debug.LogError("❌ 숫자 생성 실패");
            }

            // 4. 시각적 그룹 정보 저장
            visualGroup.noteObjects = noteHeads;
            visualGroup.stemObjects = stems;

            if (showDebugInfo) Debug.Log($"✅ === 잇단음표 시각적 그룹 조립 완료: {tupletData.GetTupletTypeName()} ===");
            if (showDebugInfo) Debug.Log($"   숫자: {(numberObj != null ? "생성됨" : "실패")}");
            if (showDebugInfo) Debug.Log($"   beam: {(visualGroup.beamObject != null ? "생성됨" : "실패")}");
            if (showDebugInfo) Debug.Log($"   음표: {noteHeads.Count}개, stem: {stems.Count}개");

            return visualGroup;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 잇단음표 조립 오류: {e.Message}");
            Debug.LogError($"   StackTrace: {e.StackTrace}");

            // 실패 시 생성된 오브젝트들 정리
            if (visualGroup.numberObject != null)
                // Object.DestroyImmediate(visualGroup.numberObject); // Object.DestroyImmediate -> DestroyImmediate (using Unity.Object)
                DestroyImmediate(visualGroup.numberObject);
            if (visualGroup.beamObject != null)
                // Object.DestroyImmediate(visualGroup.beamObject); // Object.DestroyImmediate -> DestroyImmediate
                DestroyImmediate(visualGroup.beamObject);

            return null;
        }
    }

    // 정확한 stem 끝점들 계산
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
            Vector2 stemBottomLeft = staffPanel.InverseTransformPoint(stemCorners[0]);
            Vector2 stemTopRight = staffPanel.InverseTransformPoint(stemCorners[2]);

            // stem 방향 결정
            bool stemUp = IsStemPointingUp(stem);

            Vector2 endPoint;
            // ⭐ Y축 조정값 계산 (공용 변수 사용)
            float yAdjustment = spacing * beamYAdjustmentRatio; // 이 변수가 여기서 사용됩니다.

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

            // ⭐ X 좌표 미세 조정 적용 (공용 변수 사용)
            float horizontalOffset = spacing * beamXAdjustmentRatio; // 이 변수가 여기서 사용됩니다.

            if (stem == stems[0]) // 첫 번째 stem (가장 왼쪽)
            {
                endPoint.x -= horizontalOffset; // 왼쪽으로 이동하여 beam을 길게
            }
            else if (stem == stems[stems.Count - 1]) // 마지막 stem (가장 오른쪽)
            {
                endPoint.x += horizontalOffset; // 오른쪽으로 이동하여 beam을 길게
            }
            // 그 외 중간 stem들은 필요에 따라 조정 (보통 변경하지 않음)

            // ⭐ 최종 Y 좌표에 조정 값 적용
            endPoint.y += yAdjustment; // << 이 부분을 추가/수정

            endPoints.Add(endPoint);

            if (showDebugInfo) Debug.Log($"🎯 정확한 stem 끝점: stem 위치=({stemRT.anchoredPosition.x:F1}, {stemRT.anchoredPosition.y:F1}), 월드끝점=({endPoint.x:F1}, {endPoint.y:F1}), 위쪽={stemUp}");
        }

        return endPoints;
    }

    // stem 방향 결정 (간단한 방법)
    private bool IsStemPointingUp(GameObject stem)
    {
        // stem의 부모인 note head의 위치로 판단
        Transform noteHead = stem.transform.parent;
        if (noteHead == null) return true;

        Vector2 notePosition = noteHead.GetComponent<RectTransform>().anchoredPosition;

        // Y=0 기준으로 stem 방향 결정 (낮은 음표는 stem 위로)
        return notePosition.y < 0;
    }

    // stem들에서 flag 제거
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
                    if (showDebugInfo) Debug.Log($"🚫 flag 제거: {child.name}");
                    DestroyImmediate(child.gameObject);
                    removedFlags++;
                }
            }
        }

        if (showDebugInfo) Debug.Log($"🚫 총 {removedFlags}개의 flag 제거됨");
    }

    // 숫자 위치 계산 - 반응형
    private Vector2 CalculateNumberPosition(TupletData tupletData, float spacing)
    {
        float x = tupletData.centerX;

        // beam 위쪽에 배치하되 spacing에 비례하여 높이 설정
        float y = tupletData.maxNoteY + spacing * numberHeightOffset;

        // 최소 높이 보장 (spacing에 비례)
        float minY = spacing * 2.5f;
        y = Mathf.Max(y, minY);

        if (showDebugInfo) Debug.Log($"🔢 숫자 위치 계산: x={x:F1}, y={y:F1} (maxNoteY={tupletData.maxNoteY:F1}, spacing={spacing:F1})");

        return new Vector2(x, y);
    }

    // 프리팹 할당 확인
    public bool ValidatePrefabs()
    {
        bool isValid = true;

        // 숫자 프리팹 확인
        string[] prefabNames = { "num0", "num1", "num2", "num3", "num4", "num5", "num6", "num7", "num8", "num9" };
        GameObject[] prefabs = { num0Prefab, num1Prefab, num2Prefab, num3Prefab, num4Prefab, num5Prefab, num6Prefab, num7Prefab, num8Prefab, num9Prefab };

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] == null)
            {
                Debug.LogWarning($"⚠️ {prefabNames[i]} 프리팹이 할당되지 않았습니다.");
                isValid = false;
            }
            else if (showDebugInfo)
            {
                Debug.Log($"✅ {prefabNames[i]} 프리팹 할당됨: {prefabs[i].name}");
            }
        }

        // staffPanel 확인
        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 할당되지 않았습니다!");
            isValid = false;
        }
        else if (showDebugInfo)
        {
            Debug.Log($"✅ staffPanel 할당됨: {staffPanel.name}");
        }

        if (isValid && showDebugInfo)
        {
            Debug.Log("✅ 모든 잇단음표 프리팹이 올바르게 할당되었습니다.");
        }
        else if (!isValid)
        {
            Debug.LogError("❌ 일부 프리팹이 할당되지 않았습니다!");
        }

        return isValid;
    }

    // 테스트용 메뉴
    [ContextMenu("프리팹 할당 상태 확인")]
    public void CheckPrefabAssignment()
    {
        Debug.Log("🔍 === TupletAssembler 프리팹 할당 상태 (코드 기반 beam) ===");

        string[] prefabNames = { "num0", "num1", "num2", "num3", "num4", "num5", "num6", "num7", "num8", "num9" };
        GameObject[] prefabs = { num0Prefab, num1Prefab, num2Prefab, num3Prefab, num4Prefab, num5Prefab, num6Prefab, num7Prefab, num8Prefab, num9Prefab };

        for (int i = 0; i < prefabs.Length; i++)
        {
            string status = prefabs[i] != null ? "✅ 할당됨" : "❌ 미할당";
            Debug.Log($"   {prefabNames[i]}: {status}");
        }

        Debug.Log($"전체 검증 결과: {(ValidatePrefabs() ? "✅ 성공" : "❌ 실패")}");

        // 현재 설정값 출력
        if (staffPanel != null)
        {
            float currentSpacing = MusicLayoutConfig.GetSpacing(staffPanel);
            Debug.Log($"📏 현재 spacing: {currentSpacing:F1}");
            Debug.Log($"🔢 숫자 크기: {currentSpacing * numberSizeRatio:F1} (비율: {numberSizeRatio})");
            Debug.Log($"🌉 beam 두께: {currentSpacing * beamThicknessRatio:F1} (비율: {beamThicknessRatio})");
        }
    }
}

// 잇단음표 시각적 그룹 정보를 담는 클래스
[System.Serializable]
public class TupletVisualGroup
{
    public TupletData tupletData;
    public GameObject numberObject;
    public GameObject beamObject;
    public List<GameObject> noteObjects;
    public List<GameObject> stemObjects;

    public TupletVisualGroup(TupletData data)
    {
        tupletData = data;
        noteObjects = new List<GameObject>();
        stemObjects = new List<GameObject>();
    }

    public void DestroyAll()
    {
        if (numberObject != null)
            Object.DestroyImmediate(numberObject);
        if (beamObject != null)
            Object.DestroyImmediate(beamObject);

        // note와 stem은 다른 곳에서 관리되므로 여기서는 제거하지 않음
        noteObjects.Clear();
        stemObjects.Clear();
    }
}