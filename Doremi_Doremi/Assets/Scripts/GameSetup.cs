using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 계이름 맞추기 게임을 위한 간단한 연결 및 테스트 스크립트
/// - 게임 컴포넌트들을 자동으로 연결
/// - 피아노 키 이벤트 설정
/// - 색상 변경 테스트 기능
/// </summary>
public class GameSetup : MonoBehaviour
{
    [Header("컴포넌트 참조")]
    [SerializeField] private SongGameController gameController;
    [SerializeField] private NoteColorManager noteColorManager;
    
    [Header("UI 참조")]
    [SerializeField] private Transform staffPanel;
    [SerializeField] private Transform pianoPanel;
    
    [Header("테스트 설정")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private bool enableTestMode = true;
    [SerializeField] private bool showDebugUI = false;
    
    // 현재 테스트 상태
    private int currentNoteIndex = 0;
    private string[] testNotes = {"D4", "E4", "C5", "C4", "D4", "E4"};
    private bool isTestActive = false;
    private GameObject[] testNoteObjects;
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupReferences();
            SetupPianoKeys();
            if (enableTestMode)
            {
                Invoke(nameof(StartTestMode), 1f);
            }
        }
    }
    
    /// <summary>
    /// 필요한 컴포넌트들을 자동으로 찾아서 연결
    /// </summary>
    private void SetupReferences()
    {
        Debug.Log("🔧 GameSetup: 컴포넌트 참조 설정 중...");
        
        // SongGameController 찾기
        if (gameController == null)
        {
            gameController = FindObjectOfType<SongGameController>();
            if (gameController == null)
            {
                Debug.LogError("❌ SongGameController를 찾을 수 없습니다!");
                return;
            }
        }
        
        // NoteColorManager 찾기 또는 생성
        if (noteColorManager == null)
        {
            noteColorManager = FindObjectOfType<NoteColorManager>();
            if (noteColorManager == null)
            {
                GameObject colorManagerObj = new GameObject("NoteColorManager");
                noteColorManager = colorManagerObj.AddComponent<NoteColorManager>();
                Debug.Log("🎨 NoteColorManager 자동 생성됨");
            }
        }
        
        // Staff Panel 찾기
        if (staffPanel == null)
        {
            GameObject staffPanelObj = GameObject.Find("Staff_Panel");
            if (staffPanelObj != null)
                staffPanel = staffPanelObj.transform;
        }
        
        // Piano Panel 찾기
        if (pianoPanel == null)
        {
            GameObject pianoPanelObj = GameObject.Find("Panel_Piano");
            if (pianoPanelObj != null)
                pianoPanel = pianoPanelObj.transform;
        }
        
        Debug.Log($"✅ 참조 설정 완료:");
        Debug.Log($"   SongGameController: {(gameController != null ? "✅" : "❌")}");
        Debug.Log($"   NoteColorManager: {(noteColorManager != null ? "✅" : "❌")}");
        Debug.Log($"   StaffPanel: {(staffPanel != null ? "✅" : "❌")}");
        Debug.Log($"   PianoPanel: {(pianoPanel != null ? "✅" : "❌")}");
    }
    
    /// <summary>
    /// 피아노 키들에 이벤트 연결
    /// </summary>
    private void SetupPianoKeys()
    {
        if (pianoPanel == null)
        {
            Debug.LogWarning("⚠️ Piano Panel을 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log("🎹 피아노 키 설정 중...");
        
        // 피아노 패널의 모든 자식을 검사
        for (int i = 0; i < pianoPanel.childCount; i++)
        {
            Transform child = pianoPanel.GetChild(i);
            Button button = child.GetComponent<Button>();
            
            if (button != null)
            {
                string noteName = child.name;
                
                // 기존 리스너 제거
                button.onClick.RemoveAllListeners();
                
                // 새로운 리스너 추가
                button.onClick.AddListener(() => OnPianoKeyPressed(noteName));
                
                Debug.Log($"🎹 피아노 키 연결: {noteName}");
            }
        }
        
        Debug.Log("✅ 피아노 키 설정 완료");
    }
    
    /// <summary>
    /// 피아노 키가 눌렸을 때 호출되는 메서드
    /// </summary>
    private void OnPianoKeyPressed(string noteName)
    {
        Debug.Log($"🎹 피아노 키 눌림: {noteName}");
        
        if (enableTestMode && isTestActive)
        {
            TestNoteColorChange(noteName);
        }
        
        // SongGameController에 알림
        if (gameController != null)
        {
            try
            {
                gameController.OnKeyPressed(noteName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ SongGameController.OnKeyPressed 호출 실패: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 테스트 모드 시작
    /// </summary>
    private void StartTestMode()
    {
        if (!enableTestMode || staffPanel == null) return;
        
        Debug.Log("🧪 테스트 모드 시작");
        
        // Staff Panel에서 음표들 찾기
        FindAndMarkTestNotes();
        
        isTestActive = true;
        currentNoteIndex = 0;
        
        // 첫 번째 음표를 파란색으로 표시
        if (currentNoteIndex < testNotes.Length && testNoteObjects != null && testNoteObjects.Length > 0)
        {
            HighlightCurrentNote();
        }
    }
    
    /// <summary>
    /// 테스트용 음표들 찾기 및 표시
    /// </summary>
    private void FindAndMarkTestNotes()
    {
        // Staff Panel의 모든 Image 컴포넌트 찾기
        Image[] allImages = staffPanel.GetComponentsInChildren<Image>();
        
        Debug.Log($"🔍 Staff Panel에서 {allImages.Length}개의 Image 발견");
        
        // 테스트용 음표 배열 생성
        testNoteObjects = new GameObject[Mathf.Min(testNotes.Length, allImages.Length)];
        
        // 처음 몇 개를 테스트용으로 사용
        for (int i = 0; i < testNoteObjects.Length; i++)
        {
            testNoteObjects[i] = allImages[i].gameObject;
            testNoteObjects[i].name = $"TestNote_{i + 1}";
            Debug.Log($"🎼 테스트 음표 {i + 1}: {testNoteObjects[i].name}");
        }
    }
    
    /// <summary>
    /// 현재 음표를 파란색으로 강조
    /// </summary>
    private void HighlightCurrentNote()
    {
        if (currentNoteIndex >= testNotes.Length || testNoteObjects == null || currentNoteIndex >= testNoteObjects.Length) 
            return;
        
        GameObject currentNote = testNoteObjects[currentNoteIndex];
        if (currentNote != null && noteColorManager != null)
        {
            noteColorManager.ChangeNoteColor(currentNote, NoteColorType.Current);
            Debug.Log($"🔵 현재 음표 강조: {currentNote.name}");
        }
    }
    
    /// <summary>
    /// 음표 색상 변경 테스트
    /// </summary>
    private void TestNoteColorChange(string pressedNote)
    {
        if (currentNoteIndex >= testNotes.Length || testNoteObjects == null || currentNoteIndex >= testNoteObjects.Length)
        {
            Debug.Log("🎉 테스트 완료!");
            isTestActive = false;
            return;
        }
        
        string expectedNote = testNotes[currentNoteIndex];
        GameObject currentNote = testNoteObjects[currentNoteIndex];
        
        if (currentNote != null && noteColorManager != null)
        {
            // 정답 체크 (간단하게 이름 비교)
            bool isCorrect = pressedNote.Contains(expectedNote.Substring(0, expectedNote.Length - 1));
            
            if (isCorrect)
            {
                // 정답: 녹색
                noteColorManager.ChangeNoteColor(currentNote, NoteColorType.Correct);
                Debug.Log($"✅ 정답! {pressedNote} == {expectedNote}");
            }
            else
            {
                // 오답: 빨간색
                noteColorManager.ChangeNoteColor(currentNote, NoteColorType.Incorrect);
                Debug.Log($"❌ 오답! {pressedNote} != {expectedNote}");
            }
            
            // 다음 음표로 진행
            currentNoteIndex++;
            
            // 잠시 후 다음 음표 강조
            if (currentNoteIndex < testNotes.Length)
            {
                Invoke(nameof(HighlightCurrentNote), 0.5f);
            }
        }
    }
    
    // === 수동 제어 메서드들 ===
    
    [ContextMenu("참조 설정")]
    public void ManualSetupReferences()
    {
        SetupReferences();
    }
    
    [ContextMenu("피아노 키 설정")]
    public void ManualSetupPianoKeys()
    {
        SetupPianoKeys();
    }
    
    [ContextMenu("테스트 시작")]
    public void ManualStartTest()
    {
        StartTestMode();
    }
    
    [ContextMenu("모든 색상 복원")]
    public void RestoreAllColors()
    {
        if (noteColorManager != null)
        {
            noteColorManager.RestoreAllColors();
            Debug.Log("🎨 모든 색상 복원됨");
        }
    }
    
    [ContextMenu("첫 번째 음표 색상 테스트")]
    public void TestFirstNoteColor()
    {
        if (testNoteObjects != null && testNoteObjects.Length > 0 && testNoteObjects[0] != null)
        {
            if (noteColorManager != null)
                noteColorManager.ChangeNoteColor(testNoteObjects[0], NoteColorType.Correct);
            Debug.Log("🧪 첫 번째 음표 색상 테스트");
        }
    }
    
    [ContextMenu("디버그 UI 토글")]
    public void ToggleDebugUI()
    {
        showDebugUI = !showDebugUI;
        Debug.Log($"🖥️ 디버그 UI: {(showDebugUI ? "활성화" : "비활성화")}");
    }
    
    // === 키보드 단축키 ===
    
    private void Update()
    {
        // 테스트용 키보드 단축키
        if (Input.GetKeyDown(KeyCode.T))
        {
            ManualStartTest();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestoreAllColors();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestFirstNoteColor();
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleDebugUI();
        }
        
        // 피아노 키 시뮬레이션
        if (Input.GetKeyDown(KeyCode.Z))
        {
            OnPianoKeyPressed("C4");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            OnPianoKeyPressed("D4");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            OnPianoKeyPressed("E4");
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            OnPianoKeyPressed("F4");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            OnPianoKeyPressed("G4");
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            OnPianoKeyPressed("A4");
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnPianoKeyPressed("B4");
        }
    }
    
    // === 디버그 UI ===
    
    private void OnGUI()
    {
        if (!showDebugUI) 
        {
            // 작은 토글 버튼만 표시
            if (GUI.Button(new Rect(10, 10, 80, 30), "UI 토글"))
            {
                ToggleDebugUI();
            }
            return;
        }
        
        // 오른쪽 상단에 컴팩트한 UI 표시
        float panelWidth = 300f;
        float panelHeight = 200f;
        float margin = 10f;
        
        Rect panelRect = new Rect(Screen.width - panelWidth - margin, margin, panelWidth, panelHeight);
        
        GUI.Box(panelRect, "");
        
        GUILayout.BeginArea(panelRect);
        
        GUILayout.Label("🎵 계이름 맞추기 테스트", GUI.skin.box);
        
        if (isTestActive)
        {
            GUILayout.Label($"현재: {(currentNoteIndex < testNotes.Length ? testNotes[currentNoteIndex] : "완료")}");
            GUILayout.Label($"진행: {currentNoteIndex}/{testNotes.Length}");
        }
        else
        {
            GUILayout.Label("테스트 대기 중");
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("테스트 시작 (T)"))
        {
            ManualStartTest();
        }
        
        if (GUILayout.Button("색상 복원 (R)"))
        {
            RestoreAllColors();
        }
        
        if (GUILayout.Button("첫 음표 테스트 (1)"))
        {
            TestFirstNoteColor();
        }
        
        GUILayout.Space(5);
        GUILayout.Label("키보드: Z-M (C4-B4), U (UI토글)");
        
        if (GUILayout.Button("UI 숨기기"))
        {
            showDebugUI = false;
        }
        
        GUILayout.EndArea();
    }
}
