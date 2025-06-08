using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 간소화된 계이름 맞추기 게임 컨트롤러
/// - 모듈 기반 설계로 간결함
/// - 핵심 기능에만 집중
/// </summary>
public class SongGameController : MonoBehaviour
{
    [Header("필수 참조")]
    [SerializeField] private JsonLoader jsonLoader;
    [SerializeField] private NoteManager noteManager;
    [SerializeField] private NoteColorManager colorManager;
    [SerializeField] private GameState gameState;
    
    [Header("UI 참조")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Text songTitleText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text scoreText;
    
    [Header("게임 설정")]
    [SerializeField] private float delayBetweenNotes = 0.5f;
    [SerializeField] private bool autoFindComponents = true;
    
    // 현재 게임 데이터
    private JsonLoader.SongData currentSong;
    private string expectedNote = "";
    private int expectedOctave = 4;
    
    private void Start()
    {
        InitializeComponents();
        SetupUI();
        LoadFirstSong();
    }
    
    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        if (autoFindComponents)
        {
            FindMissingComponents();
        }
        
        // GameState 초기화
        if (gameState == null)
        {
            gameState = new GameState();
        }
        
        SetupEventListeners();
        Debug.Log("🎮 게임 컨트롤러 초기화 완료");
    }
    
    /// <summary>
    /// 누락된 컴포넌트 자동 검색
    /// </summary>
    private void FindMissingComponents()
    {
        if (jsonLoader == null)
            jsonLoader = FindObjectOfType<JsonLoader>();
        
        if (noteManager == null)
            noteManager = FindObjectOfType<NoteManager>();
        
        if (colorManager == null)
            colorManager = FindObjectOfType<NoteColorManager>();
    }
    
    /// <summary>
    /// 이벤트 리스너 설정
    /// </summary>
    private void SetupEventListeners()
    {
        if (gameState != null)
        {
            gameState.OnGameStart += OnGameStart;
            gameState.OnGameComplete += OnGameComplete;
            gameState.OnProgressChanged += OnProgressChanged;
        }
    }
    
    /// <summary>
    /// UI 설정
    /// </summary>
    private void SetupUI()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        UpdateUI();
    }
    
    /// <summary>
    /// 첫 번째 곡 로드
    /// </summary>
    private void LoadFirstSong()
    {
        if (jsonLoader == null) return;
        
        var songList = jsonLoader.LoadSongs();
        if (songList != null && songList.songs.Count > 0)
        {
            SetCurrentSong(songList.songs[0]);
        }
    }
    
    /// <summary>
    /// 현재 곡 설정
    /// </summary>
    private void SetCurrentSong(JsonLoader.SongData song)
    {
        currentSong = song;
        
        if (noteManager != null)
        {
            noteManager.SetupNotes(song.notes);
        }
        
        if (colorManager != null)
        {
            colorManager.RestoreAllNoteColors();
        }
        
        UpdateUI();
        Debug.Log($"🎵 곡 설정: {song.title}");
    }
    
    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        if (currentSong == null || noteManager == null || noteManager.NoteCount == 0)
        {
            Debug.LogWarning("⚠️ 게임을 시작할 수 없습니다");
            return;
        }
        
        gameState.StartGame(noteManager.NoteCount);
        Debug.Log($"🎮 게임 시작: {currentSong.title}");
    }
    
    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        if (colorManager != null)
        {
            colorManager.RestoreAllNoteColors();
        }
        
        StartGame();
        Debug.Log("🔄 게임 재시작");
    }
    
    /// <summary>
    /// 키 입력 처리 (피아노에서 호출)
    /// </summary>
    public void OnKeyPressed(string pressedNote)
    {
        if (!gameState.isActive || !gameState.isWaitingForInput)
            return;
        
        bool isCorrect = pressedNote.ToUpper() == expectedNote.ToUpper();
        int currentIndex = gameState.currentNoteIndex;
        
        if (isCorrect)
        {
            gameState.ProcessCorrectAnswer();
            SetNoteColor(currentIndex, NoteColorType.Correct);
        }
        else
        {
            gameState.ProcessWrongAnswer();
            SetNoteColor(currentIndex, NoteColorType.Incorrect);
        }
        
        UpdateUI();
        StartCoroutine(DelayedNextNote());
        
        Debug.Log($"🎹 입력: {pressedNote} {(isCorrect ? "✅" : "❌")}");
    }
    
    /// <summary>
    /// 다음 음표 처리 (딜레이)
    /// </summary>
    private IEnumerator DelayedNextNote()
    {
        yield return new WaitForSeconds(delayBetweenNotes);
        ProcessNextNote();
    }
    
    /// <summary>
    /// 다음 음표 처리
    /// </summary>
    private void ProcessNextNote()
    {
        if (!gameState.isActive) return;
        
        int currentIndex = gameState.currentNoteIndex;
        
        if (currentIndex >= noteManager.NoteCount)
        {
            return; // 게임 완료
        }
        
        // 현재 음표 데이터 파싱
        string noteData = noteManager.GetMusicNote(currentIndex);
        ParseNoteData(noteData);
        
        // 현재 음표 강조
        SetNoteColor(currentIndex, NoteColorType.Current);
        
        // 입력 대기 상태로 변경
        gameState.SetWaitingForInput();
        gameState.MoveToNextNote();
        
        UpdateUI();
        Debug.Log($"🎵 음표 {currentIndex + 1}: {expectedNote}");
    }
    
    /// <summary>
    /// 음표 데이터 파싱
    /// </summary>
    private void ParseNoteData(string noteData)
    {
        if (string.IsNullOrEmpty(noteData))
        {
            expectedNote = "";
            expectedOctave = 4;
            return;
        }
        
        // Utils의 NoteDataParser 사용
        var (noteName, octave) = NoteDataParser.ParseNoteData(noteData);
        expectedNote = noteName;
        expectedOctave = octave;
    }
    
    /// <summary>
    /// 음표 색상 변경
    /// </summary>
    private void SetNoteColor(int noteIndex, NoteColorType colorType)
    {
        if (colorManager == null || noteManager == null)
            return;
        
        GameObject noteGroup = noteManager.GetNoteGroup(noteIndex);
        if (noteGroup != null)
        {
            colorManager.ChangeNoteColor(noteGroup, colorType);
        }
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        // 곡 제목
        if (songTitleText != null && currentSong != null)
        {
            songTitleText.text = currentSong.title;
        }
        
        // 상태 텍스트
        if (statusText != null)
        {
            UpdateStatusText();
        }
        
        // 점수 텍스트
        if (scoreText != null && gameState != null)
        {
            scoreText.text = gameState.GetGameSummary();
        }
    }
    
    /// <summary>
    /// 상태 텍스트 업데이트
    /// </summary>
    private void UpdateStatusText()
    {
        if (gameState == null) return;
        
        if (!gameState.isActive)
        {
            statusText.text = "🎮 게임 시작 버튼을 누르세요!";
        }
        else if (gameState.isCompleted)
        {
            statusText.text = $"🎉 게임 완료!\n{gameState.GetGameSummary()}";
        }
        else if (gameState.isWaitingForInput)
        {
            string noteDisplay = GetNoteDisplayName(expectedNote);
            statusText.text = $"🎹 {noteDisplay} 건반을 누르세요!\n({gameState.currentNoteIndex + 1}/{gameState.totalNotes})";
        }
        else
        {
            statusText.text = "게임 진행중...";
        }
    }
    
    /// <summary>
    /// 음표 표시명 반환
    /// </summary>
    private string GetNoteDisplayName(string noteName)
    {
        return noteName.ToUpper() switch
        {
            "C" => "도(C)",
            "C#" => "도#(C#)",
            "D" => "레(D)",
            "D#" => "레#(D#)",
            "E" => "미(E)",
            "F" => "파(F)",
            "F#" => "파#(F#)",
            "G" => "솔(G)",
            "G#" => "솔#(G#)",
            "A" => "라(A)",
            "A#" => "라#(A#)",
            "B" => "시(B)",
            _ => noteName
        };
    }
    
    // === 이벤트 핸들러들 ===
    
    private void OnGameStart()
    {
        ProcessNextNote();
    }
    
    private void OnGameComplete()
    {
        UpdateUI();
        Debug.Log("🎉 게임 완료!");
    }
    
    private void OnProgressChanged(float progress)
    {
        // 진행도 UI 업데이트 (필요시)
    }
    
    // === 디버그 메서드들 ===
    
    [ContextMenu("정답 시뮬레이션")]
    public void SimulateCorrectAnswer()
    {
        if (gameState.isWaitingForInput)
        {
            OnKeyPressed(expectedNote);
        }
    }
    
    [ContextMenu("오답 시뮬레이션")]
    public void SimulateWrongAnswer()
    {
        if (gameState.isWaitingForInput)
        {
            string wrongNote = expectedNote == "C" ? "D" : "C";
            OnKeyPressed(wrongNote);
        }
    }
    
    // === 키보드 단축키 (개발용) ===
    
    private void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (gameState.isActive)
                    gameState.StopGame();
                else
                    StartGame();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SimulateCorrectAnswer();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SimulateWrongAnswer();
            }
        }
    }
    
    // === 공개 API ===
    
    public bool IsGameActive => gameState?.isActive ?? false;
    public string GetExpectedNote() => expectedNote;
    public int GetCurrentNoteIndex() => gameState?.currentNoteIndex ?? 0;
    public float GetGameProgress() => gameState?.GetProgress() ?? 0f;
}
