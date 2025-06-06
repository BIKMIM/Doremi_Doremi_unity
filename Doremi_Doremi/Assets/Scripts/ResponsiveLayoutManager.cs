using UnityEngine;
using UnityEngine.UI;

// ResponsiveLayoutManager.cs - 해상도 독립적 반응형 레이아웃 관리자 (작업용 모드 추가)
public class ResponsiveLayoutManager : MonoBehaviour
{
    [Header("레이아웃 설정")]
    [Range(0.5f, 2.0f)]
    public float globalScaleMultiplier = 1.3f; // 오선지가 더 잘 보이도록 증가
    
    [Header("오선지 위치 설정")]
    [Range(0.01f, 0.2f)]
    public float staffTopMarginRatio = 0.05f; // 상단 여백 줄임
    
    [Range(0.2f, 0.6f)]
    public float staffHeightRatio = 0.45f; // ✅ 오선지 높이 조정
    
    [Header("오선지-피아노 간격")]
    [Range(0.02f, 0.15f)]
    public float staffPianoGapRatio = 0.08f; // ✅ 오선지와 피아노 사이 간격
    
    [Header("피아노 영역 설정")]
    [Range(0.2f, 0.5f)]
    public float pianoHeightRatio = 0.35f; // 피아노 높이 줄임
    
    [Header("🎼 작업 모드")]
    public bool workMode = true; // 작업 모드 토글
    
    [Header("참조 컴포넌트")]
    public RectTransform staffPanel;
    public RectTransform pianoPanel;
    public Canvas mainCanvas;
    
    [Header("자동 조정")]
    public bool autoAdjustOnStart = true;
    public bool autoAdjustOnResize = true;
    
    [Header("디버그")]
    public bool showDebugInfo = true;

    private Vector2 lastScreenSize;

    void Start()
    {
        if (autoAdjustOnStart)
        {
            ApplyResponsiveLayout();
        }
        
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        if (autoAdjustOnResize)
        {
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (lastScreenSize != currentScreenSize)
            {
                if (showDebugInfo)
                    Debug.Log($"🔄 화면 크기 변경 감지: {lastScreenSize} → {currentScreenSize}");
                
                ApplyResponsiveLayout();
                lastScreenSize = currentScreenSize;
            }
        }
    }

    // 반응형 레이아웃 적용
    public void ApplyResponsiveLayout()
    {
        if (showDebugInfo)
            Debug.Log($"🎯 === 반응형 레이아웃 적용 시작 (작업모드: {workMode}) ===");

        // 작업 모드에 따라 다른 설정 적용
        if (workMode)
        {
            ApplyWorkModeLayout();
        }
        else
        {
            ApplyGameModeLayout();
        }
        
        // 1. 오선지 레이아웃 조정
        AdjustStaffLayout();
        
        // 2. 피아노 레이아웃 조정
        AdjustPianoLayout();
        
        // 3. 전체 스케일 적용
        ApplyGlobalScale();
        
        // 4. 음악 설정 업데이트
        UpdateMusicLayoutSettings();
        
        if (showDebugInfo)
        {
            Debug.Log("✅ === 반응형 레이아웃 적용 완료 ===");
            PrintLayoutInfo();
        }
    }

    // 🎼 작업 모드 레이아웃 (오선지 크게, 피아노 작게)
    private void ApplyWorkModeLayout()
    {
        staffTopMarginRatio = 0.08f;     // 상단 여백 8%
        staffHeightRatio = 0.5f;         // ✅ 오선지 50% (적당히 크게)
        staffPianoGapRatio = 0.08f;      // ✅ 오선지-피아노 간격 8%
        pianoHeightRatio = 0.28f;        // ✅ 피아노 28% (작게)
        globalScaleMultiplier = 1.2f;    // 전체적으로 적당히 크게
        
        if (showDebugInfo)
            Debug.Log("🎼 작업 모드 레이아웃 적용: 오선지 우선, 간격 확보");
    }

    // 🎮 게임 모드 레이아웃 (균형잡힌 크기)
    private void ApplyGameModeLayout()
    {
        staffTopMarginRatio = 0.1f;      // 상단 여백 10%
        staffHeightRatio = 0.4f;         // ✅ 오선지 40%
        staffPianoGapRatio = 0.1f;       // ✅ 오선지-피아노 간격 10%
        pianoHeightRatio = 0.35f;        // ✅ 피아노 35%
        globalScaleMultiplier = 1.0f;    // 기본 크기
        
        if (showDebugInfo)
            Debug.Log("🎮 게임 모드 레이아웃 적용: 균형잡힌 크기, 적절한 간격");
    }

    // ✅ 오선지 레이아웃 조정 (간격 고려)
    private void AdjustStaffLayout()
    {
        if (staffPanel == null) return;

        // 오선지는 상단에서 시작하여 설정된 높이만큼 차지
        // 피아노와의 간격은 별도로 계산됨
        float staffBottomY = 1f - staffTopMarginRatio - staffHeightRatio;

        staffPanel.anchorMin = new Vector2(0.05f, staffBottomY);
        staffPanel.anchorMax = new Vector2(0.95f, 1f - staffTopMarginRatio);
        
        // 오프셋 초기화 (앵커로 위치 결정)
        staffPanel.offsetMin = Vector2.zero;
        staffPanel.offsetMax = Vector2.zero;

        if (showDebugInfo)
        {
            Debug.Log($"🎼 오선지 레이아웃 조정:");
            Debug.Log($"   상단여백={staffTopMarginRatio:P0}, 높이={staffHeightRatio:P0}");
            Debug.Log($"   앵커: ({staffPanel.anchorMin.x:F2}, {staffPanel.anchorMin.y:F2}) ~ ({staffPanel.anchorMax.x:F2}, {staffPanel.anchorMax.y:F2})");
            Debug.Log($"   오선지 하단 Y: {staffBottomY:F2}");
        }
    }

    // ✅ 피아노 레이아웃 조정 (간격 고려)
    private void AdjustPianoLayout()
    {
        if (pianoPanel == null) return;

        // 피아노는 화면 하단에서 시작하여 설정된 높이만큼 차지
        pianoPanel.anchorMin = new Vector2(0f, 0f);
        pianoPanel.anchorMax = new Vector2(1f, pianoHeightRatio);
        
        pianoPanel.offsetMin = Vector2.zero;
        pianoPanel.offsetMax = Vector2.zero;

        if (showDebugInfo)
        {
            Debug.Log($"🎹 피아노 레이아웃 조정:");
            Debug.Log($"   높이={pianoHeightRatio:P0}");
            Debug.Log($"   앵커: ({pianoPanel.anchorMin.x:F2}, {pianoPanel.anchorMin.y:F2}) ~ ({pianoPanel.anchorMax.x:F2}, {pianoPanel.anchorMax.y:F2})");
            
            // ✅ 간격 확인 로그
            float staffBottom = 1f - staffTopMarginRatio - staffHeightRatio;
            float actualGap = staffBottom - pianoHeightRatio;
            Debug.Log($"   실제 오선지-피아노 간격: {actualGap:P1} (설정값: {staffPianoGapRatio:P1})");
        }
    }

    // 전체 스케일 적용
    private void ApplyGlobalScale()
    {
        float scaleFactor = MusicLayoutConfig.GetScreenScaleFactor() * globalScaleMultiplier;
        
        if (staffPanel != null)
        {
            staffPanel.localScale = Vector3.one * scaleFactor;
        }
        
        if (pianoPanel != null)
        {
            pianoPanel.localScale = Vector3.one * scaleFactor;
        }

        if (showDebugInfo)
        {
            Debug.Log($"⚖️ 전체 스케일 적용: {scaleFactor:F2} (기본: {MusicLayoutConfig.GetScreenScaleFactor():F2}, 배수: {globalScaleMultiplier:F2})");
        }
    }

    // 음악 레이아웃 설정 업데이트
    private void UpdateMusicLayoutSettings()
    {
        if (staffPanel != null)
        {
            // 다른 음악 관련 컴포넌트들에게 레이아웃 변경 알림
            var staffLineDrawer = FindObjectOfType<StaffLineDrawer>();
            if (staffLineDrawer != null)
            {
                staffLineDrawer.RecalculateLayout();
            }

            var tupletLayoutHandler = FindObjectOfType<TupletLayoutHandler>();
            if (tupletLayoutHandler != null)
            {
                tupletLayoutHandler.Initialize(staffPanel);
            }
        }
    }

    // ✅ 현재 레이아웃 정보 출력 (간격 정보 추가)
    public void PrintLayoutInfo()
    {
        Debug.Log("📊 === 현재 레이아웃 정보 ===");
        Debug.Log($"   모드: {(workMode ? "🎼 작업모드" : "🎮 게임모드")}");
        Debug.Log($"   화면 해상도: {Screen.width}x{Screen.height}");
        Debug.Log($"   스케일 팩터: {MusicLayoutConfig.GetScreenScaleFactor():F2}");
        Debug.Log($"   글로벌 배수: {globalScaleMultiplier:F2}");
        Debug.Log($"   오선지: 상단{staffTopMarginRatio:P0} 여백, 높이{staffHeightRatio:P0}");
        Debug.Log($"   피아노: 높이{pianoHeightRatio:P0}");
        Debug.Log($"   설정 간격: {staffPianoGapRatio:P0}");
        
        // ✅ 실제 간격 계산
        float staffBottom = 1f - staffTopMarginRatio - staffHeightRatio;
        float actualGap = staffBottom - pianoHeightRatio;
        Debug.Log($"   실제 간격: {actualGap:P1}");
        
        if (staffPanel != null)
        {
            float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
            Debug.Log($"   현재 spacing: {spacing:F1}");
        }
    }

    // 🎼 작업 모드 토글
    [ContextMenu("🎼 작업 모드 ON")]
    public void EnableWorkMode()
    {
        workMode = true;
        ApplyResponsiveLayout();
        Debug.Log("🎼 작업 모드 활성화: 오선지가 크게 표시됩니다");
    }

    [ContextMenu("🎮 게임 모드 ON")]
    public void EnableGameMode()
    {
        workMode = false;
        ApplyResponsiveLayout();
        Debug.Log("🎮 게임 모드 활성화: 균형잡힌 레이아웃입니다");
    }

    // 피아노 토글
    [ContextMenu("🎹 피아노 ON/OFF")]
    public void TogglePiano()
    {
        if (pianoPanel != null)
        {
            bool isActive = pianoPanel.gameObject.activeSelf;
            pianoPanel.gameObject.SetActive(!isActive);
            Debug.Log($"🎹 피아노 {(!isActive ? "활성화" : "비활성화")}");
            
            // 피아노 상태에 따라 레이아웃 재조정
            if (!isActive)
            {
                // 피아노 활성화 시 게임 모드로
                EnableGameMode();
            }
            else
            {
                // 피아노 비활성화 시 작업 모드로
                EnableWorkMode();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ 피아노 패널을 찾을 수 없습니다. 참조를 설정해주세요.");
        }
    }

    // ✅ 프리셋 적용 (간격 고려)
    [ContextMenu("📱 모바일 레이아웃")]
    public void ApplyMobileLayout()
    {
        staffTopMarginRatio = 0.06f;
        staffHeightRatio = 0.35f;        // ✅ 작게
        staffPianoGapRatio = 0.1f;       // ✅ 간격 넉넉히
        pianoHeightRatio = 0.45f;        // ✅ 피아노 크게 (터치용)
        globalScaleMultiplier = 1.3f;
        
        ApplyResponsiveLayout();
        Debug.Log("📱 모바일 레이아웃 적용됨 (피아노 우선)");
    }

    [ContextMenu("📱 태블릿 레이아웃")]
    public void ApplyTabletLayout()
    {
        staffTopMarginRatio = 0.08f;
        staffHeightRatio = 0.42f;        // ✅ 균형
        staffPianoGapRatio = 0.08f;      // ✅ 적당한 간격
        pianoHeightRatio = 0.38f;        // ✅ 균형
        globalScaleMultiplier = 1.1f;
        
        ApplyResponsiveLayout();
        Debug.Log("📱 태블릿 레이아웃 적용됨 (균형잡힌)");
    }

    [ContextMenu("🖥️ 데스크톱 레이아웃")]
    public void ApplyDesktopLayout()
    {
        staffTopMarginRatio = 0.1f;
        staffHeightRatio = 0.5f;         // ✅ 오선지 크게
        staffPianoGapRatio = 0.06f;      // ✅ 간격 좁게
        pianoHeightRatio = 0.28f;        // ✅ 피아노 작게
        globalScaleMultiplier = 0.9f;
        
        ApplyResponsiveLayout();
        Debug.Log("🖥️ 데스크톱 레이아웃 적용됨 (오선지 우선)");
    }

    // ✅ 간격 미세 조정 메뉴
    [ContextMenu("🔧 간격 좁히기")]
    public void DecreaseGap()
    {
        staffPianoGapRatio = Mathf.Max(0.02f, staffPianoGapRatio - 0.02f);
        ApplyResponsiveLayout();
        Debug.Log($"🔧 간격 좁히기: {staffPianoGapRatio:P0}");
    }

    [ContextMenu("🔧 간격 넓히기")]
    public void IncreaseGap()
    {
        staffPianoGapRatio = Mathf.Min(0.15f, staffPianoGapRatio + 0.02f);
        ApplyResponsiveLayout();
        Debug.Log($"🔧 간격 넓히기: {staffPianoGapRatio:P0}");
    }

    [ContextMenu("🔄 레이아웃 재계산")]
    public void RecalculateLayout()
    {
        ApplyResponsiveLayout();
    }

    [ContextMenu("⚙️ 설정 리셋")]
    public void ResetToDefault()
    {
        workMode = true; // 기본적으로 작업 모드
        ApplyResponsiveLayout();
        Debug.Log("🔄 기본 설정으로 리셋됨 (작업 모드)");
    }

    // 화면 비율에 따른 자동 레이아웃 선택
    public void AutoSelectLayout()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        
        if (aspectRatio < 1.3f) // 세로형 (모바일)
        {
            ApplyMobileLayout();
        }
        else if (aspectRatio < 1.7f) // 정사각형에 가까운 (태블릿)
        {
            ApplyTabletLayout();
        }
        else // 가로형 (데스크톱)
        {
            ApplyDesktopLayout();
        }
        
        Debug.Log($"📐 화면 비율 {aspectRatio:F2}에 따른 자동 레이아웃 선택됨");
    }

    // 컴포넌트 참조 자동 할당
    [ContextMenu("🔗 참조 자동 할당")]
    public void AutoAssignReferences()
    {
        if (mainCanvas == null)
            mainCanvas = FindObjectOfType<Canvas>();
        
        if (staffPanel == null)
        {
            GameObject staffObj = GameObject.Find("Staff_Panel");
            if (staffObj != null)
                staffPanel = staffObj.GetComponent<RectTransform>();
        }
        
        if (pianoPanel == null)
        {
            GameObject pianoObj = GameObject.Find("Panel_Piano");
            if (pianoObj != null)
                pianoPanel = pianoObj.GetComponent<RectTransform>();
        }
        
        Debug.Log($"🔗 참조 자동 할당: Canvas={mainCanvas != null}, Staff={staffPanel != null}, Piano={pianoPanel != null}");
    }

    // Inspector에서 값 변경 시 자동 적용
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyResponsiveLayout();
        }
    }
}
