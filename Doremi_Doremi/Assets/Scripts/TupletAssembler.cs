using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 잇단음표 시각적 요소 생성 및 조립을 담당하는 클래스
/// - 숫자 프리팹을 이용한 잇단음표 번호 생성
/// - 코드 기반 beam 생성
/// - 모듈화된 구조로 색상 관리 분리
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

    // 내부에서 사용할 배열
    private GameObject[] numberPrefabs;
    
    // 컴포넌트 참조들
    private NoteColorManager colorManager;
    private TupletAssemblyLogic assemblyLogic;

    void Awake()
    {
        InitializePrefabs();
        InitializeComponents();
        Debug.Log("🎼 TupletAssembler 초기화 완료 (모듈화된 버전)");
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
    /// 필요한 컴포넌트들 초기화
    /// </summary>
    private void InitializeComponents()
    {
        // NoteColorManager 찾기 (잣단음표도 음표 색상 관리자 사용)
        colorManager = FindObjectOfType<NoteColorManager>();
        if (colorManager == null)
        {
            GameObject colorManagerObj = new GameObject("NoteColorManager");
            colorManager = colorManagerObj.AddComponent<NoteColorManager>();
            Debug.Log("🎨 NoteColorManager 자동 생성됨");
        }

        // TupletAssemblyLogic 찾기 또는 생성
        assemblyLogic = GetComponent<TupletAssemblyLogic>();
        if (assemblyLogic == null)
        {
            assemblyLogic = gameObject.AddComponent<TupletAssemblyLogic>();
            Debug.Log("🔧 TupletAssemblyLogic 자동 생성됨");
        }
    }

    /// <summary>
    /// 잇단음표 숫자 생성 - 반응형
    /// </summary>
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

    /// <summary>
    /// 코드로 beam 생성 - Image 컴포넌트 활용
    /// </summary>
    public GameObject CreateBeamWithCode(Vector2 startPos, Vector2 endPos, float thickness)
    {
        if (showDebugInfo)
        {
            Debug.Log($"🌉 코드 기반 beam 생성: ({startPos.x:F1}, {startPos.y:F1}) → ({endPos.x:F1}, {endPos.y:F1}), 두께={thickness:F2}");
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
        }
        return beamObj;
    }

    /// <summary>
    /// 잇단음표 전체 조립 (TupletAssemblyLogic에 위임)
    /// </summary>
    public TupletVisualGroup AssembleTupletGroup(TupletData tupletData, List<GameObject> noteHeads, List<GameObject> stems, float spacing)
    {
        if (assemblyLogic != null)
        {
            return assemblyLogic.AssembleTupletGroup(tupletData, noteHeads, stems, spacing);
        }
        else
        {
            Debug.LogError("❌ TupletAssemblyLogic이 없습니다!");
            return null;
        }
    }

    // === 색상 관리 관련 메서드들 (NoteColorManager로 위임) ===

    /// <summary>
    /// 잇단음표 색상 변경
    /// </summary>
    public void ChangeTupletColor(TupletVisualGroup visualGroup, Color color)
    {
        if (colorManager != null && visualGroup != null)
        {
            // 잇단음표의 모든 구성요소 색상 변경
            if (visualGroup.numberObject != null)
                colorManager.ChangeNoteColor(visualGroup.numberObject, color);
            
            if (visualGroup.beamObject != null)
                colorManager.ChangeNoteColor(visualGroup.beamObject, color);
            
            // 각 음표들도 색상 변경
            foreach (var noteHead in visualGroup.noteHeads)
            {
                if (noteHead != null)
                    colorManager.ChangeNoteColor(noteHead, color);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ NoteColorManager가 없습니다!");
        }
    }

    /// <summary>
    /// 잇단음표 색상 복원
    /// </summary>
    public void RestoreTupletColor(TupletVisualGroup visualGroup)
    {
        if (colorManager != null && visualGroup != null)
        {
            // 잇단음표의 모든 구성요소 색상 복원
            if (visualGroup.numberObject != null)
                colorManager.RestoreNoteColor(visualGroup.numberObject);
            
            if (visualGroup.beamObject != null)
                colorManager.RestoreNoteColor(visualGroup.beamObject);
            
            // 각 음표들도 색상 복원
            foreach (var noteHead in visualGroup.noteHeads)
            {
                if (noteHead != null)
                    colorManager.RestoreNoteColor(noteHead);
            }
        }
    }

    // 편의 메서드들
    public void SetCorrectColor(TupletVisualGroup visualGroup) 
    {
        if (colorManager != null)
            ChangeTupletColor(visualGroup, colorManager.correctColor);
    }
    
    public void SetIncorrectColor(TupletVisualGroup visualGroup)
    {
        if (colorManager != null)
            ChangeTupletColor(visualGroup, colorManager.incorrectColor);
    }
    
    public void SetCurrentColor(TupletVisualGroup visualGroup)
    {
        if (colorManager != null)
            ChangeTupletColor(visualGroup, colorManager.currentColor);
    }
    
    public void SetHighlightColor(TupletVisualGroup visualGroup)
    {
        if (colorManager != null)
            ChangeTupletColor(visualGroup, colorManager.highlightColor);
    }
    
    public void SetDefaultColor(TupletVisualGroup visualGroup)
    {
        if (colorManager != null)
            ChangeTupletColor(visualGroup, colorManager.defaultColor);
    }

    // === 유틸리티 메서드들 ===

    /// <summary>
    /// 프리팹 유효성 검사
    /// </summary>
    public bool ValidatePrefabs()
    {
        bool isValid = true;

        // 숫자 프리팹 확인
        string[] prefabNames = { "num0", "num1", "num2", "num3", "num4", "num5", "num6", "num7", "num8", "num9" };

        for (int i = 0; i < numberPrefabs.Length; i++)
        {
            if (numberPrefabs[i] == null)
            {
                Debug.LogWarning($"⚠️ {prefabNames[i]} 프리팹이 할당되지 않았습니다.");
                isValid = false;
            }
        }

        // staffPanel 확인
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
        Debug.Log("🔍 === TupletAssembler 프리팹 할당 상태 (모듈화된 버전) ===");

        string[] prefabNames = { "num0", "num1", "num2", "num3", "num4", "num5", "num6", "num7", "num8", "num9" };

        for (int i = 0; i < numberPrefabs.Length; i++)
        {
            string status = numberPrefabs[i] != null ? "✅ 할당됨" : "❌ 미할당";
            Debug.Log($"   {prefabNames[i]}: {status}");
        }

        Debug.Log($"전체 검증 결과: {(ValidatePrefabs() ? "✅ 성공" : "❌ 실패")}");
        Debug.Log($"NoteColorManager: {(colorManager != null ? "✅ 할당됨" : "❌ 없음")}");
        Debug.Log($"TupletAssemblyLogic: {(assemblyLogic != null ? "✅ 할당됨" : "❌ 없음")}");
    }

    [ContextMenu("색상 테스트")]
    public void TestColors()
    {
        if (colorManager != null)
        {
            Debug.Log("🎨 잇단음표 색상 테스트 - 실제 잇단음표가 있을 때만 동작합니다.");
        }
        else
        {
            Debug.LogWarning("⚠️ NoteColorManager가 없어서 색상 테스트를 할 수 없습니다.");
        }
    }

    [ContextMenu("컴포넌트 재초기화")]
    public void ReinitializeComponents()
    {
        InitializeComponents();
        Debug.Log("🔄 TupletAssembler 컴포넌트 재초기화 완료");
    }
}
