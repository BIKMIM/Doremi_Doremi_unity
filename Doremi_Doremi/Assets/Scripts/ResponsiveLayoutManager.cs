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
    
    [Range(0.4f, 0.8f)]
    public float staffHeightRatio = 0.6f; // 오선지 높이 증가
    
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
        staffTopMarginRatio = 0.05f;     // 상단 여백 5%
        staffHeightRatio = 0.65f;        // 오선지 65% (크게!)
        pianoHeightRatio = 0.3f;         // 피아노 30% (작게)
        globalScaleMultiplier = 1.4f;    // 전체적으로 크게
        
        if (showDebugInfo)
            Debug.Log("🎼 작업 모드 레이아웃 적용: 오선지 우선");
    }

    // 🎮 게임 모드 레이아웃 (균형잡힌 크기)
    private void ApplyGameModeLayout()
    {
        staffTopMarginRatio = 0.1f;      // 상단 여백 10%
        staffHeightRatio = 0.45f;        // 오선지 45%
        pianoHeightRatio = 0.45f;        // 피아노 45%
        globalScaleMultiplier = 1.0f;    // 기본 크기
        
        if (showDebugInfo)
            Debug.Log("🎮 게임 모드 레이아웃 적용: 균형잡힌 크기");
    }

    // 오선지 레이아웃 조정
    private void AdjustStaffLayout()
    {
        if (staffPanel == null) return;

        // 화면 상단에서 설정된 비율만큼 떨어진 위치에 배치
        staffPanel.anchorMin = new Vector2(0.05f, 1f - staffTopMarginRatio - staffHeightRatio);
        staffPanel.anchorMax = new Vector2(0.95f, 1f - staffTopMarginRatio);
        
        // 오프셋 초기화 (앵커로 위치 결정)
        staffPanel.offsetMin = Vector2.zero;
        staffPanel.offsetMax = Vector2.zero;

        if (showDebugInfo)
        {
            Debug.Log($"🎼 오선지 레이아웃 조정: 상단여백={staffTopMarginRatio:P0}, 높이={staffHeightRatio:P0}");
            Debug.Log($"   앵커: ({staffPanel.anchorMin.x:F2}, {staffPanel.anchorMin.y:F2}) ~ ({staffPanel.anchorMax.x:F2}, {staffPanel.anchorMax.y:F2})");
        }
    }

    // 피아노 레이아웃 조정
    private void AdjustPianoLayout()
    {
        if (pianoPanel == null) return;

        // 화면 하단에 피아노 배치
        pianoPanel.anchorMin = new Vector2(0f, 0f);
        pianoPanel.anchorMax = new Vector2(1f, pianoHeightRatio);
        
        pianoPanel.offsetMin = Vector2.zero;
        pianoPanel.offsetMax = Vector2.zero;

        if (showDebugInfo)
        {
            Debug.Log($"🎹 피아노 레이아웃 조정: 높이={pianoHeightRatio:P0}");
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

    // 현재 레이아웃 정보 출력
    public void PrintLayoutInfo()
    {
        Debug.Log("📊 === 현재 레이아웃 정보 ===");
        Debug.Log($"   모드: {(workMode ? "🎼 작업모드" : "🎮 게임모드")}");
        Debug.Log($"   화면 해상도: {Screen.width}x{Screen.height}");
        Debug.Log($"   스케일 팩터: {MusicLayoutConfig.GetScreenScaleFactor():F2}");
        Debug.Log($"   글로벌 배수: {globalScaleMultiplier:F2}");
        Debug.Log($"   오선지 위치: 상단{staffTopMarginRatio:P0} 여백, 높이{staffHeightRatio:P0}");
        Debug.Log($"   피아노 높이: {pianoHeightRatio:P0}");
        
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

    // 프리셋 적용
    [ContextMenu("📱 모바일 레이아웃")]
    public void ApplyMobileLayout()
    {
        staffTopMarginRatio = 0.08f;
        staffHeightRatio = 0.35f;
        pianoHeightRatio = 0.55f;
        globalScaleMultiplier = 1.2f;
        
        ApplyResponsiveLayout();
        Debug.Log("📱 모바일 레이아웃 적용됨");
    }

    [ContextMenu("📱 태블릿 레이아웃")]
    public void ApplyTabletLayout()
    {
        staffTopMarginRatio = 0.1f;
        staffHeightRatio = 0.4f;
        pianoHeightRatio = 0.5f;
        globalScaleMultiplier = 1.0f;
        
        ApplyResponsiveLayout();
        Debug.Log("📱 태블릿 레이아웃 적용됨");
    }

    [ContextMenu("🖥️ 데스크톱 레이아웃")]
    public void ApplyDesktopLayout()
    {
        staffTopMarginRatio = 0.12f;
        staffHeightRatio = 0.45f;
        pianoHeightRatio = 0.4f;
        globalScaleMultiplier = 0.9f;
        
        ApplyResponsiveLayout();
        Debug.Log("🖥️ 데스크톱 레이아웃 적용됨");
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

    // Inspector에서 workMode 변경 시 자동 적용
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyResponsiveLayout();
        }
    }
}