using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 반응형 UI 컨트롤러 - 모든 해상도/기기에서 일정한 비율 유지
/// </summary>
public class ResponsiveUIController : MonoBehaviour
{
    [Header("Reference Resolution")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    
    [Header("UI Elements")]
    [SerializeField] private RectTransform pianoPanel;
    [SerializeField] private RectTransform staffPanel;
    [SerializeField] private RectTransform gameControlPanel;
    [SerializeField] private RectTransform songNavigationPanel;
    [SerializeField] private RectTransform octaveController;
    
    [Header("Layout Settings")]
    [SerializeField] private float pianoHeightRatio = 0.35f; // 화면 높이의 35%
    [SerializeField] private float staffHeightRatio = 0.45f; // 화면 높이의 45%
    // controlPanelWidthRatio 제거 (사용되지 않는 변수)
    
    private CanvasScaler canvasScaler;
    private Canvas mainCanvas;
    
    private void Start()
    {
        InitializeCanvas();
        SetupResponsiveLayout();
    }
    
    private void InitializeCanvas()
    {
        // 메인 캔버스 찾기
        mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
            mainCanvas = FindObjectOfType<Canvas>();
            
        // Canvas Scaler 설정
        canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler == null)
            canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
            
        // Scale With Screen Size 모드로 설정
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f; // 너비와 높이 균형
        
        Debug.Log($"🎨 Canvas Scaler 설정 완료 - 기준 해상도: {referenceResolution}");
    }
    
    private void SetupResponsiveLayout()
    {
        // 자동으로 UI 요소들 찾기
        FindUIElements();
        
        // 각 UI 요소의 앵커와 위치 설정
        SetupPianoPanel();
        SetupStaffPanel();
        SetupGameControlPanel();
        SetupOctaveController();
        SetupSongNavigation();
        
        Debug.Log("🎨 반응형 UI 레이아웃 설정 완료");
    }
    
    private void FindUIElements()
    {
        if (pianoPanel == null)
        {
            GameObject pianoObj = GameObject.Find("Panel_Piano");
            if (pianoObj != null)
                pianoPanel = pianoObj.GetComponent<RectTransform>();
        }
        
        if (staffPanel == null)
        {
            GameObject staffObj = GameObject.Find("Staff_Panel");
            if (staffObj != null)
                staffPanel = staffObj.GetComponent<RectTransform>();
        }
        
        if (gameControlPanel == null)
        {
            GameObject controlObj = GameObject.Find("GameControlPanel");
            if (controlObj != null)
                gameControlPanel = controlObj.GetComponent<RectTransform>();
        }
        
        if (octaveController == null)
        {
            GameObject octaveObj = GameObject.Find("OctaveController");
            if (octaveObj != null)
                octaveController = octaveObj.GetComponent<RectTransform>();
        }
    }
    
    private void SetupPianoPanel()
    {
        if (pianoPanel == null) return;
        
        // 피아노 - 화면 하단에 고정, 너비는 70%
        pianoPanel.anchorMin = new Vector2(0.15f, 0f);      // 왼쪽 15% 지점부터
        pianoPanel.anchorMax = new Vector2(0.75f, pianoHeightRatio); // 오른쪽 75%, 높이는 35%
        pianoPanel.offsetMin = Vector2.zero;
        pianoPanel.offsetMax = Vector2.zero;
        
        Debug.Log("🎹 피아노 패널 - 반응형 설정 완료");
    }
    
    private void SetupStaffPanel()
    {
        if (staffPanel == null) return;
        
        // 오선지 - 화면 상단, 너비는 75%
        staffPanel.anchorMin = new Vector2(0.125f, 1f - staffHeightRatio); // 왼쪽 12.5%, 상단에서 45% 아래
        staffPanel.anchorMax = new Vector2(0.75f, 1f);                     // 오른쪽 75%, 상단
        staffPanel.offsetMin = Vector2.zero;
        staffPanel.offsetMax = Vector2.zero;
        
        Debug.Log("🎼 오선지 패널 - 반응형 설정 완료");
    }
    
    private void SetupGameControlPanel()
    {
        if (gameControlPanel == null) return;
        
        // 게임 컨트롤 패널 - 화면 오른쪽, 피아노 옆 (너비 25% 사용)
        gameControlPanel.anchorMin = new Vector2(0.75f, 0f);               // 오른쪽 25% 영역
        gameControlPanel.anchorMax = new Vector2(1f, 0.8f);                // 전체 오른쪽, 높이 80%
        gameControlPanel.offsetMin = Vector2.zero;
        gameControlPanel.offsetMax = Vector2.zero;
        
        Debug.Log("🎮 게임 컨트롤 패널 - 반응형 설정 완료");
    }
    
    private void SetupOctaveController()
    {
        if (octaveController == null) return;
        
        // 옥타브 컨트롤러 - 화면 왼쪽 하단
        octaveController.anchorMin = new Vector2(0f, 0f);                  // 왼쪽 하단
        octaveController.anchorMax = new Vector2(0.15f, pianoHeightRatio); // 왼쪽 15%, 피아노 높이만큼
        octaveController.offsetMin = Vector2.zero;
        octaveController.offsetMax = Vector2.zero;
        
        Debug.Log("🎵 옥타브 컨트롤러 - 반응형 설정 완료");
    }
    
    private void SetupSongNavigation()
    {
        if (songNavigationPanel == null) return;
        
        // 노래 네비게이션 - 게임 컨트롤 패널 하단
        songNavigationPanel.anchorMin = new Vector2(0.75f, 0.8f);          // 오른쪽 25% 영역 상단
        songNavigationPanel.anchorMax = new Vector2(1f, 1f);               // 화면 우상단
        songNavigationPanel.offsetMin = Vector2.zero;
        songNavigationPanel.offsetMax = Vector2.zero;
        
        Debug.Log("🎶 노래 네비게이션 - 반응형 설정 완료");
    }
    
    // 런타임에서 레이아웃 업데이트
    [ContextMenu("Update Layout")]
    public void UpdateLayout()
    {
        SetupResponsiveLayout();
    }
    
    // 화면 회전이나 크기 변경 감지
    private void Update()
    {
        // 해상도 변경 감지 (모바일 회전 등)
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            OnScreenSizeChanged();
        }
    }
    
    private int lastScreenWidth;
    private int lastScreenHeight;
    
    private void OnScreenSizeChanged()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        
        Debug.Log($"🎨 화면 크기 변경 감지: {Screen.width}x{Screen.height}");
        
        // 잠깐 대기 후 레이아웃 업데이트 (안정화)
        Invoke("UpdateLayout", 0.1f);
    }
    
    private void OnValidate()
    {
        // 에디터에서 값 변경 시 실시간 업데이트
        if (Application.isPlaying)
        {
            UpdateLayout();
        }
    }
}
