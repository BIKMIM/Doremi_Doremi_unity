using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 계이름 맞추기 게임 컨트롤러
/// - 음표 색상 변경으로 피드백 제공
/// - 쉼표 무시하고 순서대로 진행
/// - 올바른 음표 누르면 녹색, 틀리면 빨간색
/// - 끝까지 완주하면 성공/실패 판정
/// </summary>
public class SongGameController : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private JsonLoader jsonLoader;
    [SerializeField] private DynamicPianoMapper pianoMapper;
    [SerializeField] private Transform staffPanel; // 오선지 패널 (음표들이 있는 곳)
    
    [Header("UI References")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private Text currentSongText;
    [SerializeField] private Text gameStatusText;
    [SerializeField] private Text scoreText;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color defaultNoteColor = Color.black;
    [SerializeField] private Color currentNoteColor = Color.blue; // 현재 대기중인 음표 색상
    
    // Game State
    private JsonLoader.SongList songList;
    private JsonLoader.SongData currentSong;
    private int currentSongIndex = 0;
    private int currentMusicNoteIndex = 0; // 실제 음표 순서 (쉼표 제외)
    private bool gameIsActive = false;
    private bool waitingForInput = false;
    private string expectedNote = "";
    private int expectedOctave = 4;
    
    // Score System
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private int totalNotes = 0;
    
    // Note Management
    private List<string> musicNotesOnly = new List<string>(); // 쉼표를 제외한 음표들만
    private List<GameObject> noteObjects = new List<GameObject>(); // 오선지의 음표 오브젝트들
    
    private void Start()
    {
        InitializeGame();
        SetupUI();
        LoadSongs();
    }
    
    private void InitializeGame()
    {
        // 자동으로 컴포넌트 찾기
        if (jsonLoader == null)
            jsonLoader = FindObjectOfType<JsonLoader>();
            
        if (pianoMapper == null)
            pianoMapper = FindObjectOfType<DynamicPianoMapper>();
            
        // Staff Panel 찾기
        if (staffPanel == null)
        {
            GameObject staffPanelObj = GameObject.Find("Staff_Panel");
            if (staffPanelObj != null)
                staffPanel = staffPanelObj.transform;
        }
        
        Debug.Log("=== 🎵 계이름 맞추기 게임 초기화 완료 ===");
        Debug.Log($"StaffPanel 찾기: {(staffPanel != null ? "성공" : "실패")}");
    }
    
    private void SetupUI()
    {
        // UI 버튼 이벤트 연결
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
            
        if (restartGameButton != null)
            restartGameButton.onClick.AddListener(RestartGame);
            
        UpdateUI();
    }
    
    private void LoadSongs()
    {
        if (jsonLoader == null)
        {
            Debug.LogError("❗ JsonLoader가 없습니다!");
            return;
        }
        
        songList = jsonLoader.LoadSongs();
        
        if (songList == null || songList.songs.Count == 0)
        {
            Debug.LogError("❗ 노래를 불러올 수 없습니다!");
            if (gameStatusText != null)
                gameStatusText.text = "노래 파일을 찾을 수 없습니다";
            return;
        }
        
        Debug.Log($"✅ {songList.songs.Count}곡 로드 완료");
        
        // 첫 번째 곡으로 설정 (샘플 곡)
        SetCurrentSong(0);
    }
    
    private void SetCurrentSong(int index)
    {
        if (songList == null || index < 0 || index >= songList.songs.Count)
            return;
            
        currentSongIndex = index;
        currentSong = songList.songs[index];
        currentMusicNoteIndex = 0;
        
        // 쉼표를 제외한 음표들만 추출
        ExtractMusicNotesOnly();
        
        // 오선지의 음표 오브젝트들 찾기
        FindNoteObjects();
        
        Debug.Log($"🎵 현재 곡: {currentSong.title}");
        Debug.Log($"🎶 전체 음표 수: {currentSong.notes.Count}");
        Debug.Log($"🎶 실제 음표 수: {musicNotesOnly.Count} (쉼표 제외)");
        
        totalNotes = musicNotesOnly.Count;
        
        UpdateUI();
        ResetNoteColors();
    }
    
    private void ExtractMusicNotesOnly()
    {
        musicNotesOnly.Clear();
        
        foreach (string noteData in currentSong.notes)
        {
            if (!IsRest(noteData) && !IsBarLine(noteData))
            {
                musicNotesOnly.Add(noteData);
            }
        }
        
        Debug.Log($"🎶 음표 목록 (쉼표 제외): {string.Join(", ", musicNotesOnly)}");
    }
    
    private bool IsRest(string noteData)
    {
        return noteData.StartsWith("REST") || noteData.StartsWith("rest");
    }
    
    private bool IsBarLine(string noteData)
    {
        return noteData == "|" || noteData.Contains("TUPLET") || noteData.Contains("DOUBLE");
    }
    
    private void FindNoteObjects()
    {
        noteObjects.Clear();
        
        if (staffPanel == null)
        {
            Debug.LogWarning("⚠️ Staff Panel을 찾을 수 없습니다.");
            return;
        }
        
        // 오선지에서 실제 음표 오브젝트들을 찾기
        // "Note_" 또는 "note" 태그를 가진 오브젝트들 찾기
        for (int i = 0; i < staffPanel.childCount; i++)
        {
            Transform child = staffPanel.GetChild(i);
            
            // 음표 관련 오브젝트인지 확인
            if (child.name.Contains("Note") || child.name.Contains("note") || 
                child.GetComponent<Image>() != null)
            {
                noteObjects.Add(child.gameObject);
            }
        }
        
        // 음표 오브젝트가 부족한 경우 추가로 찾기
        if (noteObjects.Count < musicNotesOnly.Count)
        {
            Debug.LogWarning($"⚠️ 음표 오브젝트 부족: {noteObjects.Count} < {musicNotesOnly.Count}");
            
            // 더 넓은 범위에서 찾기
            Image[] allImages = FindObjectsOfType<Image>();
            foreach (Image img in allImages)
            {
                if (img.transform.IsChildOf(staffPanel) && !noteObjects.Contains(img.gameObject))
                {
                    noteObjects.Add(img.gameObject);
                    if (noteObjects.Count >= musicNotesOnly.Count) break;
                }
            }
        }
        
        Debug.Log($"🎼 음표 오브젝트 {noteObjects.Count}개 찾음");
    }
    
    private void UpdateUI()
    {
        if (currentSongText != null && currentSong != null)
        {
            currentSongText.text = $"{currentSong.title}";
        }
        
        if (gameStatusText != null)
        {
            if (gameIsActive)
            {
                if (waitingForInput)
                {
                    string noteDisplay = GetNoteDisplayName(expectedNote);
                    gameStatusText.text = $"{noteDisplay} 건반을 누르세요! ({currentMusicNoteIndex + 1}/{totalNotes})";
                }
                else
                {
                    gameStatusText.text = "다음 음표로...";
                }
            }
            else
            {
                gameStatusText.text = "게임 시작 버튼을 누르세요!";
            }
        }
        
        if (scoreText != null)
        {
            int totalAnswered = correctAnswers + wrongAnswers;
            float accuracy = totalAnswered > 0 ? (float)correctAnswers / totalAnswered * 100f : 0f;
            scoreText.text = $"정답: {correctAnswers} / 오답: {wrongAnswers} (정확도: {accuracy:F1}%)";
        }
    }
    
    private string GetNoteDisplayName(string noteName)
    {
        // 음표 이름을 한국어 계이름으로 변환
        switch (noteName.ToUpper())
        {
            case "C": return "도(C)";
            case "C#": return "도#(C#)";
            case "D": return "레(D)";
            case "D#": return "레#(D#)";
            case "E": return "미(E)";
            case "F": return "파(F)";
            case "F#": return "파#(F#)";
            case "G": return "솔(G)";
            case "G#": return "솔#(G#)";
            case "A": return "라(A)";
            case "A#": return "라#(A#)";
            case "B": return "시(B)";
            default: return noteName;
        }
    }
    
    public void StartGame()
    {
        if (currentSong == null || musicNotesOnly.Count == 0)
        {
            Debug.LogWarning("⚠️ 재생할 곡이 없습니다!");
            return;
        }
        
        gameIsActive = true;
        currentMusicNoteIndex = 0;
        waitingForInput = false;
        
        // 점수 초기화
        correctAnswers = 0;
        wrongAnswers = 0;
        
        // 음표 색상 초기화
        ResetNoteColors();
        
        Debug.Log($"🎮 게임 시작: {currentSong.title}");
        
        // 첫 번째 음표부터 시작
        ProcessNextNote();
        
        UpdateUI();
    }
    
    public void RestartGame()
    {
        Debug.Log("🔄 게임 다시시작");
        StartGame();
    }
    
    public void StopGame()
    {
        gameIsActive = false;
        waitingForInput = false;
        
        ResetNoteColors();
        
        Debug.Log("🛑 게임 중지");
        UpdateUI();
    }
    
    private void ProcessNextNote()
    {
        if (!gameIsActive || currentMusicNoteIndex >= musicNotesOnly.Count)
        {
            OnGameComplete();
            return;
        }
        
        // 현재 음표 정보 파싱
        string noteData = musicNotesOnly[currentMusicNoteIndex];
        ParseNoteData(noteData);
        
        Debug.Log($"🎵 음표 {currentMusicNoteIndex + 1}: {noteData} → {expectedNote}{expectedOctave}");
        
        // 현재 음표를 파란색으로 표시 (대기 상태)
        SetNoteColor(currentMusicNoteIndex, currentNoteColor);
        
        // 음표 재생
        PlayCurrentNote();
        
        // 사용자 입력 대기 상태로 전환
        waitingForInput = true;
        UpdateUI();
    }
    
    private void ParseNoteData(string noteData)
    {
        if (string.IsNullOrEmpty(noteData))
        {
            expectedNote = "";
            expectedOctave = 4;
            return;
        }
        
        // 콜론으로 분리 (음표:박자)
        string[] parts = noteData.Split(':');
        string notePart = parts[0];
        
        // 음표와 옥타브 분리
        if (notePart.Length >= 2 && char.IsDigit(notePart[notePart.Length - 1]))
        {
            // 마지막 문자가 숫자인 경우 (C4, D#5 등)
            expectedOctave = int.Parse(notePart[notePart.Length - 1].ToString());
            expectedNote = notePart.Substring(0, notePart.Length - 1);
        }
        else
        {
            // 옥타브가 명시되지 않은 경우 기본값 사용
            expectedNote = notePart;
            expectedOctave = 4; // 기본 옥타브
        }
        
        // 음표 이름 정규화
        expectedNote = NormalizeNoteName(expectedNote);
    }
    
    private string NormalizeNoteName(string note)
    {
        if (string.IsNullOrEmpty(note)) return "";
        
        note = note.ToUpper();
        
        // 다양한 샤프/플랫 표기법 처리
        note = note.Replace("S", "#")    // CS → C#
                  .Replace("SHARP", "#") // CSHARP → C#
                  .Replace("s", "#");    // Cs → C#
        
        return note;
    }
    
    private void PlayCurrentNote()
    {
        // DynamicPianoMapper를 통해 해당 음표의 옥타브 설정 후 재생
        if (pianoMapper != null)
        {
            pianoMapper.UpdateNoteOctave(expectedNote, expectedOctave);
            
            // 해당 건반 찾아서 소리 재생
            PianoKey[] allKeys = FindObjectsOfType<PianoKey>();
            foreach (PianoKey key in allKeys)
            {
                if (key.NoteName.ToUpper() == expectedNote.ToUpper())
                {
                    key.PlaySoundOnly(); // 게임 로직 알림 없이 소리만 재생
                    break;
                }
            }
        }
    }
    
    public void OnKeyPressed(string pressedNoteName)
    {
        if (!gameIsActive || !waitingForInput)
            return;
            
        Debug.Log($"🎹 건반 입력: {pressedNoteName} (기대값: {expectedNote})");
        
        // 정답 체크
        bool isCorrect = pressedNoteName.ToUpper() == expectedNote.ToUpper();
        
        if (isCorrect)
        {
            correctAnswers++;
            SetNoteColor(currentMusicNoteIndex, correctColor);
            Debug.Log("✅ 정답!");
        }
        else
        {
            wrongAnswers++;
            SetNoteColor(currentMusicNoteIndex, incorrectColor);
            Debug.Log($"❌ 오답! 정답은 {expectedNote}");
        }
        
        // 다음 음표로 진행
        waitingForInput = false;
        currentMusicNoteIndex++;
        
        UpdateUI();
        
        // 잠시 대기 후 다음 음표로
        StartCoroutine(DelayedNextNote());
    }
    
    private IEnumerator DelayedNextNote()
    {
        yield return new WaitForSeconds(0.5f); // 0.5초 대기
        
        if (gameIsActive)
        {
            ProcessNextNote();
        }
    }
    
    private void SetNoteColor(int noteIndex, Color color)
    {
        if (noteIndex < 0 || noteIndex >= noteObjects.Count)
        {
            Debug.LogWarning($"⚠️ 음표 인덱스 범위 초과: {noteIndex}");
            return;
        }
        
        GameObject noteObj = noteObjects[noteIndex];
        if (noteObj == null)
        {
            Debug.LogWarning($"⚠️ 음표 오브젝트가 null입니다: {noteIndex}");
            return;
        }
        
        // Image 컴포넌트 찾아서 색상 변경
        Image noteImage = noteObj.GetComponent<Image>();
        if (noteImage != null)
        {
            noteImage.color = color;
            Debug.Log($"🎼 음표 {noteIndex + 1} 색상 변경: {color}");
        }
        else
        {
            Debug.LogWarning($"⚠️ 음표 오브젝트에 Image 컴포넌트가 없습니다: {noteObj.name}");
        }
    }
    
    private void ResetNoteColors()
    {
        // 모든 음표를 기본 색상으로 초기화
        for (int i = 0; i < noteObjects.Count && i < musicNotesOnly.Count; i++)
        {
            SetNoteColor(i, defaultNoteColor);
        }
        
        Debug.Log("🎼 모든 음표 색상 초기화");
    }
    
    private void OnGameComplete()
    {
        gameIsActive = false;
        waitingForInput = false;
        
        float accuracy = totalNotes > 0 ? (float)correctAnswers / totalNotes * 100f : 0f;
        bool gameSuccess = wrongAnswers == 0; // 틀린 음표가 하나도 없어야 성공
        
        Debug.Log("🎉 곡 완료!");
        Debug.Log($"📊 최종 점수: {correctAnswers}/{totalNotes} 정답, {wrongAnswers}개 오답 ({accuracy:F1}%)");
        Debug.Log($"🏆 게임 결과: {(gameSuccess ? "성공!" : "실패")}");
        
        if (gameStatusText != null)
        {
            string resultText = gameSuccess ? "🎉 성공!" : "😅 실패";
            gameStatusText.text = $"{resultText} 정확도: {accuracy:F1}% - 다시시작으로 재도전하세요";
        }
    }
    
    // 디버그 및 테스트 메서드들
    [ContextMenu("테스트: 현재 곡 정보")]
    public void DebugCurrentSong()
    {
        if (currentSong != null)
        {
            Debug.Log($"=== 현재 곡 정보 ===");
            Debug.Log($"제목: {currentSong.title}");
            Debug.Log($"전체 음표: {string.Join(", ", currentSong.notes)}");
            Debug.Log($"실제 음표 (쉼표 제외): {string.Join(", ", musicNotesOnly)}");
            Debug.Log($"음표 오브젝트 수: {noteObjects.Count}");
        }
    }
    
    [ContextMenu("테스트: 첫 번째 음표 재생")]
    public void TestPlayFirstNote()
    {
        if (musicNotesOnly.Count > 0)
        {
            currentMusicNoteIndex = 0;
            ParseNoteData(musicNotesOnly[0]);
            PlayCurrentNote();
        }
    }
    
    [ContextMenu("테스트: 음표 오브젝트 찾기")]
    public void TestFindNoteObjects()
    {
        FindNoteObjects();
        Debug.Log($"찾은 음표 오브젝트들:");
        for (int i = 0; i < noteObjects.Count; i++)
        {
            Debug.Log($"{i}: {(noteObjects[i] != null ? noteObjects[i].name : "NULL")}");
        }
    }
    
    [ContextMenu("테스트: 음표 색상 테스트")]
    public void TestNoteColors()
    {
        for (int i = 0; i < Mathf.Min(3, noteObjects.Count); i++)
        {
            Color testColor = i == 0 ? correctColor : i == 1 ? incorrectColor : currentNoteColor;
            SetNoteColor(i, testColor);
        }
    }
    
    // 키보드 단축키 (디버깅용)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (gameIsActive)
                StopGame();
            else
                StartGame();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
}
