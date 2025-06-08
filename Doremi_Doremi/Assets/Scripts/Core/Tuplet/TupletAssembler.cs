using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 잇단음표 시각적 요소 생성 및 조립을 담당하는 클래스
/// - 숫자 프리팹을 이용한 잇단음표 번호 생성
/// - 코드 기반 beam 생성
/// - 색상 관리 기능 통합
/// </summary>
public class TupletAssembler : MonoBehaviour
{
    [Header("오선 패널")]
    public RectTransform staffPanel;

    [Header("숫자 프리팹 (num0 ~ num9)")]
    public GameObject num0Prefab;
    public GameObject num1Prefab;
    public GameObject num2Prefab;
    public GameObject num3Prefab;
    public GameObject num4Prefab;
    public GameObject num5Prefab;
    public GameObject num6Prefab;
    public GameObject num7Prefab;
    public GameObject num8Prefab;
    public GameObject num9Prefab;

    [Header("잇단음표 설정 - 반응형")]
    [Range(1.0f, 4.0f)]
    public float numberSizeRatio = 2.0f;

    [Range(0.05f, 0.6f)]
    public float beamThicknessRatio = 0.5f;

    [Range(0.8f, 2.0f)]
    public float numberHeightOffset = 1.5f;

    [Header("Beam 미세 조정")]
    [Range(-0.5f, 0.5f)]
    public float beamYAdjustmentRatio = 0f;

    [Range(0.0f, 0.5f)]
    public float beamXAdjustmentRatio = 0.2f;

    [Header("Beam 색상")]
    public Color beamColor = Color.black;

    [Header("디버그")]
    public bool showDebugInfo = true;

    // 내부 배열
    private GameObject[] numberPrefabs;

    void Awake()
    {
        InitializePrefabs();
        Debug.Log("🎼 TupletAssembler 초기화 완료");
        ValidatePrefabs();
    }

    /// <summary>
    /// 숫자 프리팹 배열 초기화
    /// </summary>
    private void InitializePrefabs()
    {
        numberPrefabs = new GameObject[10]
        {
            num0Prefab, num1Prefab, num2Prefab, num3Prefab, num4Prefab,
            num5Prefab, num6Prefab, num7Prefab, num8Prefab, num9Prefab
        };
    }

    /// <summary>
    /// 잇단음표 숫자 생성 - 반응형
    /// </summary>
    public GameObject CreateTupletNumber(int number, Vector2 position, float spacing)
    {
        if (showDebugInfo) Debug.Log($"🔢 잇단음표 숫자 생성: {number}");

        if (number < 0 || number >= numberPrefabs.Length)
        {
            Debug.LogError($"❌ 지원되지 않는 숫자: {number}");
            return null;
        }

        GameObject prefab = numberPrefabs[number];
        if (prefab == null || staffPanel == null)
        {
            Debug.LogError($"❌ 프리팹 또는 staffPanel이 null");
            return null;
        }

        GameObject numberObj = Instantiate(prefab, staffPanel);
        RectTransform rt = numberObj.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;

        float numberSize = spacing * numberSizeRatio;
        rt.sizeDelta = new Vector2(numberSize, numberSize);
        rt.localScale = Vector3.one;
        numberObj.name = $"TupletNumber_{number}";

        return numberObj;
    }

    /// <summary>
    /// 코드 기반 beam 생성
    /// </summary>
    public GameObject CreateBeamWithCode(Vector2 startPos, Vector2 endPos, float thickness)
    {
        if (showDebugInfo) Debug.Log($"🌉 beam 생성: {startPos} → {endPos}");

        if (staffPanel == null) return null;

        GameObject beamObj = new GameObject("TupletBeam_Code");
        beamObj.transform.SetParent(staffPanel, false);

        RectTransform rt = beamObj.AddComponent<RectTransform>();
        beamObj.AddComponent<CanvasRenderer>();
        Image beamImage = beamObj.AddComponent<Image>();
        beamImage.color = beamColor;

        Vector2 direction = endPos - startPos;
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = startPos;
        rt.sizeDelta = new Vector2(length, thickness);
        rt.rotation = Quaternion.Euler(0, 0, angle);
        rt.localScale = Vector3.one;

        return beamObj;
    }

    /// <summary>
    /// 잇단음표 전체 조립
    /// </summary>
    public TupletVisualGroup AssembleTupletGroup(TupletData tupletData, List<GameObject> noteHeads, List<GameObject> stems, float spacing)
    {
        if (showDebugInfo) Debug.Log($"🎼 잇단음표 그룹 조립: {tupletData.GetTupletTypeName()}");

        if (!tupletData.IsComplete()) return null;

        TupletVisualGroup visualGroup = new TupletVisualGroup(tupletData);

        try
        {
            // Flag 제거
            RemoveFlagsFromStems(stems);

            // Beam 생성
            List<Vector2> stemEndPoints = GetAccurateStemEndPoints(stems, spacing);
            if (stemEndPoints.Count >= 2)
            {
                Vector2 firstStemEnd = stemEndPoints[0];
                Vector2 lastStemEnd = stemEndPoints[stemEndPoints.Count - 1];
                float beamThickness = spacing * beamThicknessRatio;

                GameObject beamObj = CreateBeamWithCode(firstStemEnd, lastStemEnd, beamThickness);
                visualGroup.beamObject = beamObj;
            }

            // 숫자 생성
            Vector2 numberPos = CalculateNumberPosition(tupletData, spacing);
            GameObject numberObj = CreateTupletNumber(tupletData.noteCount, numberPos, spacing);
            visualGroup.numberObject = numberObj;

            // 참조 저장 (noteHeads -> noteObjects 수정)
            visualGroup.noteObjects = noteHeads;
            visualGroup.stemObjects = stems;

            return visualGroup;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 잇단음표 조립 오류: {e.Message}");
            
            // 정리
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

            Vector3[] stemCorners = new Vector3[4];
            stemRT.GetWorldCorners(stemCorners);

            Vector2 stemBottomLeft = staffPanel.InverseTransformPoint(stemCorners[0]);
            Vector2 stemTopRight = staffPanel.InverseTransformPoint(stemCorners[2]);

            bool stemUp = IsStemPointingUp(stem);
            float yAdjustment = spacing * beamYAdjustmentRatio;

            Vector2 endPoint;
            if (stemUp)
            {
                endPoint = new Vector2((stemBottomLeft.x + stemTopRight.x) * 0.5f, stemTopRight.y);
            }
            else
            {
                endPoint = new Vector2((stemBottomLeft.x + stemTopRight.x) * 0.5f, stemBottomLeft.y);
            }

            // X축 조정
            float horizontalOffset = spacing * beamXAdjustmentRatio;
            if (stem == stems[0])
            {
                endPoint.x -= horizontalOffset;
            }
            else if (stem == stems[stems.Count - 1])
            {
                endPoint.x += horizontalOffset;
            }

            endPoint.y += yAdjustment;
            endPoints.Add(endPoint);
        }

        return endPoints;
    }

    /// <summary>
    /// stem 방향 결정
    /// </summary>
    private bool IsStemPointingUp(GameObject stem)
    {
        Transform noteHead = stem.transform.parent;
        if (noteHead == null) return true;

        Vector2 notePosition = noteHead.GetComponent<RectTransform>().anchoredPosition;
        return notePosition.y < 0;
    }

    /// <summary>
    /// stem들에서 flag 제거
    /// </summary>
    private void RemoveFlagsFromStems(List<GameObject> stems)
    {
        foreach (GameObject stem in stems)
        {
            if (stem == null) continue;

            for (int i = stem.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = stem.transform.GetChild(i);
                if (child.name.ToLower().Contains("flag"))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 숫자 위치 계산
    /// </summary>
    private Vector2 CalculateNumberPosition(TupletData tupletData, float spacing)
    {
        float x = tupletData.centerX;
        float y = tupletData.maxNoteY + spacing * numberHeightOffset;
        float minY = spacing * 2.5f;
        y = Mathf.Max(y, minY);

        return new Vector2(x, y);
    }

    /// <summary>
    /// 프리팹 유효성 검사
    /// </summary>
    public bool ValidatePrefabs()
    {
        bool isValid = true;
        string[] prefabNames = { "num0", "num1", "num2", "num3", "num4", "num5", "num6", "num7", "num8", "num9" };

        for (int i = 0; i < numberPrefabs.Length; i++)
        {
            if (numberPrefabs[i] == null)
            {
                Debug.LogWarning($"⚠️ {prefabNames[i]} 프리팹이 할당되지 않았습니다.");
                isValid = false;
            }
        }

        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 할당되지 않았습니다!");
            isValid = false;
        }

        return isValid;
    }

    [ContextMenu("프리팹 할당 상태 확인")]
    public void CheckPrefabAssignment()
    {
        Debug.Log("🔍 === TupletAssembler 프리팹 할당 상태 ===");
        ValidatePrefabs();
    }
}
