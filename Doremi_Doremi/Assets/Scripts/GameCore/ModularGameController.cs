using UnityEngine;
using System.Collections;

/// <summary>
/// 간소화된 모듈형 게임 컨트롤러
/// - 핵심 기능에만 집중
/// - 모듈간 느슨한 결합
/// </summary>
public class ModularGameController : MonoBehaviour
{
    [Header("핵심 모듈")]
    [SerializeField] private JsonLoader jsonLoader;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private NoteFinder noteFinder;
    [SerializeField] private ImprovedNoteColorManager colorManager;
    [SerializeField] private GameUIManager uiManager;
    
    [Header("게임 설정")]
    [SerializeField] private bool autoFindComponents = true;
    [SerializeField] private float delayBetweenNotes = 0.5f;
    
    // 현재 게임 상태
    private JsonLoader.SongData currentSong;
    private System.Collections.Generic.List<string> musicNotes;
    private System.Collections.Generic.List<GameObject> noteObjects;
    private string expectedNote = "";
    
    private void Start()
    {
        InitializeModules();
        LoadSongs();
    }
    
    /// <summary>
    /// 모듈 초기화
    /// </summary>
    private void InitializeModules()
    {
        if (autoFindComponents)
        {
            FindMissingComponents();
        }
        
        SetupEventListeners();
        Debug.Log("🎮 모듈형 게임 컨트롤러 초기화 완료");
    }
    
    /// <summary>
    /// 누락된 컴포넌트 찾기
    /// </summary>
    private void FindMissingComponents()
    {
        if (jsonLoader == null)
            jsonLoader = FindObjectOfType<JsonLoader>();
        
        if (gameStateManager == null)
            gameStateManager = FindObjectOfType<GameStateManager>();
        
        if (noteFinder == null)
            noteFinder = FindObjectOfType<NoteFinder>();
        
        if (colorManager == null)
            colorManager = FindObjectOfType<ImprovedNoteColorManager>();
        
        if (uiManager == null)
            uiManager = FindObjectOfType<GameUIManager>();
    }
    
    /// <summary>
    /// 이벤트 리스너 설정
    /// </summary>
    private void SetupEventListeners()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnGameStart += OnGameStart;
            gameStateManager.OnGameComplete += OnGameComplete;
            gameStateManager.OnCorrectAnswer += OnCorrectAnswer;
            gameStateManager.OnWrongAnswer += OnWrongAnswer;
        }
        
        if (uiManager != null)
        {
            uiManager.OnStartButtonClicked += StartGame;
            uiManager.OnRestartButtonClicked += RestartGame;
        }
    }
    
    /// <summary>
    /// 곡 데이터 로드
    /// </summary>
    private void LoadSongs()
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
        
        // 음표 데이터 추출
        musicNotes = NoteDataParser.ExtractMusicNotesOnly(song.notes);
        
        // 음표 객체 찾기
        if (noteFinder != null)
        {
            noteObjects = noteFinder.GetNoteGroups(musicNotes.Count);
        }
        
        // UI 업데이트
        if (uiManager != null)
        {
            uiManager.UpdateCurrentSongText(song.title);
        }
        
        ResetAllColors();
        Debug.Log($"🎵 곡 설정: {song.title} ({musicNotes.Count}개 음표)");
    }
    
    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        if (currentSong == null || musicNotes == null || musicNotes.Count == 0)
        {
            Debug.LogWarning("⚠️ 재생할 곡이 없습니다!");
            return;
        }
        
        ResetAllColors();
        
        if (gameStateManager != null)
        {
            gameStateManager.StartGame(musicNotes.Count);
        }
        
        Debug.Log($"🎮 게임 시작: {currentSong.title}");
    }
    
    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        ResetAllColors();
        StartGame();
        Debug.Log("🔄 게임 재시작");
    }
    
    /// <summary>
    /// 키 입력 처리
    /// </summary>
    public void OnKeyPressed(string pressedNoteName)
    {
        if (gameStateManager == null || !gameStateManager.IsGameActive || !gameStateManager.IsWaitingForInput)
            return;
        
        bool isCorrect = NoteDataParser.CompareNotes(pressedNoteName, expectedNote, true);
        int currentIndex = gameStateManager.CurrentNoteIndex;
        
        if (isCorrect)
        {
            gameStateManager.ProcessCorrectAnswer();
            SetNoteColor(currentIndex, colorManager.SetCorrectColor);
        }
        else
        {
            gameStateManager.ProcessWrongAnswer();
            SetNoteColor(currentIndex, colorManager.SetIncorrectColor);
        }
        
        UpdateUI();
        StartCoroutine(DelayedNextNote());
        
        Debug.Log($"🎹 입력: {pressedNoteName} {(isCorrect ? "✅" : "❌")}");
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
        if (gameStateManager == null || !gameStateManager.IsGameActive)
            return;
        
        int currentIndex = gameStateManager.CurrentNoteIndex;
        
        if (currentIndex >= musicNotes.Count)
        {
            return; // 게임 완료
        }
        
        // 현재 음표 파싱
        string noteData = musicNotes[currentIndex];
        var (noteName, octave) = NoteDataParser.ParseNoteData(noteData);
        expectedNote = noteName;
        
        // 현재 음표 강조
        SetNoteColor(currentIndex, colorManager.SetCurrentColor);
        
        // 게임 상태 업데이트
        gameStateManager.MoveToNextNote();
        
        UpdateUI();
        Debug.Log($"🎵 음표 {currentIndex + 1}: {expectedNote}");
    }
    
    /// <summary>
    /// 음표 색상 변경
    /// </summary>
    private void SetNoteColor(int noteIndex, System.Action<GameObject> colorSetter)
    {
        if (colorManager == null || noteIndex < 0 || noteIndex >= noteObjects.Count)
            return;
        
        GameObject noteObject = noteObjects[noteIndex];
        if (noteObject != null)
        {
            colorSetter.Invoke(noteObject);
        }
    }
    
    /// <summary>
    /// 모든 색상 초기화
    /// </summary>
    private void ResetAllColors()
    {
        if (colorManager != null)
        {
            colorManager.RestoreAllColors();
        }
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (uiManager != null && gameStateManager != null)
        {
            uiManager.UpdateUIForGameState(gameStateManager, expectedNote);
        }
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
    
    private void OnCorrectAnswer(int score)
    {
        Debug.Log($"✅ 정답! (점수: {score})");
    }
    
    private void OnWrongAnswer(int score)
    {
        Debug.Log($"❌ 오답! (점수: {score})");
    }
    
    // === 디버그 메서드들 ===
    
    [ContextMenu("정답 시뮬레이션")]
    public void SimulateCorrectAnswer()
    {
        if (gameStateManager != null && gameStateManager.IsWaitingForInput)
        {
            OnKeyPressed(expectedNote);
        }
    }
    
    [ContextMenu("오답 시뮬레이션")]
    public void SimulateWrongAnswer()
    {
        if (gameStateManager != null && gameStateManager.IsWaitingForInput)
        {
            string wrongNote = expectedNote == "C" ? "D" : "C";
            OnKeyPressed(wrongNote);
        }
    }
    
    // === 키보드 단축키 ===
    
    private void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (gameStateManager != null && gameStateManager.IsGameActive)
                    gameStateManager.StopGame();
                else
                    StartGame();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
        }
    }
    
    // === 공개 API ===
    
    public bool IsGameActive => gameStateManager?.IsGameActive ?? false;
    public string GetExpectedNote() => expectedNote;
    public int GetCurrentNoteIndex() => gameStateManager?.CurrentNoteIndex ?? 0;
}
