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

    // [Header("Game References")] 아래 또는 적절한 위치에 추가
    [SerializeField] private NotePlacementHandler notePlacementHandler; // 이 줄을 추가하세요.



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
        // 이 부분을 추가하거나 수정하세요.
        if (notePlacementHandler == null)
            notePlacementHandler = FindObjectOfType<NotePlacementHandler>();
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

        ExtractMusicNotesOnly();

        // FindNoteObjects() 대신 NotePlacementHandler에서 가져오기
        if (notePlacementHandler != null)
        {
            noteObjects = new List<GameObject>(notePlacementHandler.spawnedNoteHeadsInOrder);
            // 필요하다면 여기서 musicNotesOnly와 noteObjects의 갯수/순서 일치 여부를 검증
            Debug.Log($"🎼 NotePlacementHandler에서 음표 오브젝트 {noteObjects.Count}개 가져옴");
        }
        else
        {
            Debug.LogWarning("⚠️ NotePlacementHandler가 할당되지 않았습니다. 음표 오브젝트를 가져올 수 없습니다.");
            FindNoteObjects(); // 백업 로직으로 기존 FindNoteObjects() 호출
        }

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

    // ExtractMusicNotesOnly 수정 (TupletData도 고려)
    private void ExtractMusicNotesOnly()
    {
        musicNotesOnly.Clear();
        // NoteSpawner에서 사용하는 SplitIntoMeasures 로직을 참고하여
        // JSON 파싱 후 NoteData와 TupletData를 구분하는 로직을 가져와야 합니다.
        // 현재 SongGameController는 `currentSong.notes`의 string 형태를 직접 파싱하므로 한계가 있습니다.

        // 임시 방편으로, NoteSpawner의 SplitIntoMeasures에서 사용하는 TupletParser.ParseWithTuplets
        // 또는 유사한 로직을 SongGameController에서도 사용해야 정확한 음표 목록을 얻을 수 있습니다.

        // 현재 SongGameController의 제한적인 파싱으로는 Tuplet의 개별 음표를 추출하기 어렵습니다.
        // NoteSpawner의 parsedElements 리스트에 접근하여 정확한 NoteData를 가져오는 것이 이상적입니다.
        // 만약 그것이 어렵다면, 각 string을 NoteData로 파싱하고 쉼표와 마디선을 걸러내야 합니다.

        foreach (string noteData in currentSong.notes)
        {
            // NoteParser.Parse(noteData)와 같이 NoteData 객체로 먼저 파싱을 시도하는 것이 좋습니다.
            // 현재 프로젝트에 NoteParser 클래스가 있다면 사용하세요.
            // 만약 TUPLET 형태라면, 해당 TUPLET 내의 첫 번째 음표를 추출하거나, TUPLET 자체를 건너뛰는 선택 필요.

            if (noteData.ToUpper().StartsWith("REST")) // 쉼표는 건너뛰기
            {
                continue;
            }
            if (noteData.Trim() == "|") // 마디선 건너뛰기
            {
                continue;
            }
            // TUPLET은 복합적인 구조이므로, 단순히 string으로 처리하기 어렵습니다.
            // TUPLET이 string으로 되어있다면 "TUPLET:3of2:C4:D4:E4" 형태일 것입니다.
            // 이 경우 TUPLET 내부의 음표들을 하나씩 musicNotesOnly에 추가해야 합니다.
            if (noteData.ToUpper().StartsWith("TUPLET"))
            {
                // TUPLET 파싱 로직 필요 (NoteSpawner의 TupletParser와 유사)
                // 예: "TUPLET:3of2:C4:D4:E4" -> C4, D4, E4를 추출하여 추가
                string[] tupletParts = noteData.Split(':');
                if (tupletParts.Length > 3) // TUPLET:count:duration:note1:note2:...
                {
                    for (int i = 3; i < tupletParts.Length; i++)
                    {
                        // 잇단음표 내부의 각 음표도 쉼표나 바라인이 아닌지 다시 검사
                        string innerNote = tupletParts[i].Trim();
                        if (!IsRest(innerNote) && !IsBarLine(innerNote)) // 잇단음표 내부의 쉼표/바라인도 처리
                        {
                            musicNotesOnly.Add(innerNote);
                        }
                    }
                }
            }
            else // 일반 음표 (DOUBLE 등 다른 특수 표기법도 여기에 포함되어야 함)
            {
                // 일반 음표는 그대로 추가
                musicNotesOnly.Add(noteData);
            }
        }
    }




    private bool IsRest(string noteData)
    {
        return noteData.StartsWith("REST") || noteData.StartsWith("rest");
    }

    private bool IsBarLine(string noteData)
    {
        // JSON에서 "|"로 명확히 마디선이 표시된다면 이것만 체크
        return noteData.Trim() == "|";
    }

    private void FindNoteObjects()
    {
        noteObjects.Clear();

        if (staffPanel == null)
        {
            Debug.LogWarning("⚠️ Staff Panel을 찾을 수 없습니다.");
            return;
        }

        // 먼저 모든 유효한 음표 오브젝트를 찾아서 Dictionary에 저장 (이름: GameObject)
        Dictionary<string, GameObject> allNotesOnStaff = new Dictionary<string, GameObject>();
        for (int i = 0; i < staffPanel.childCount; i++)
        {
            Transform child = staffPanel.GetChild(i);
            // 'NoteHead' 태그가 있다면 가장 확실 (ModularNoteAssembler에서 부여 가정)
            if (child.CompareTag("NoteHead"))
            {
                allNotesOnStaff[child.name] = child.gameObject; // 예: "C4NoteHead"
            }
            // 또는 이름에 'Note'가 포함되고 Image 컴포넌트가 있는 경우 (덜 정확)
            else if (child.name.Contains("Note") && child.GetComponent<Image>() != null)
            {
                // NoteHead_C4, Note_C4 등 실제 음표의 머리 부분 오브젝트만 선별 필요
                // 여기서는 간단히 이름에 "Note"가 있고, "stem"이나 "flag"가 없는 것으로 가정
                if (!child.name.ToLower().Contains("stem") && !child.name.ToLower().Contains("flag"))
                {
                    allNotesOnStaff[child.name] = child.gameObject;
                }
            }
        }
        Debug.Log($"🎼 오선지에서 유효한 음표 후보 오브젝트 {allNotesOnStaff.Count}개 찾음.");

        // musicNotesOnly 순서에 맞춰 실제 음표 오브젝트 리스트 구성
        // 이 부분이 중요합니다. `NoteSpawner`가 음표를 배치하는 순서와 일치해야 합니다.
        foreach (string noteData in musicNotesOnly)
        {
            string expectedNoteNameWithOctave = GetNoteNameForMatching(noteData); // "C4", "D#5" 등
            GameObject foundNoteObject = null;

            // 정확한 매칭 시도 (예: "NoteHead_C4" 또는 "Note_C4")
            foreach (var kvp in allNotesOnStaff)
            {
                // 오브젝트 이름에서 'C4', 'D#5' 등을 추출하여 매칭
                if (kvp.Key.Contains(expectedNoteNameWithOctave))
                {
                    foundNoteObject = kvp.Value;
                    break;
                }
            }

            if (foundNoteObject != null)
            {
                noteObjects.Add(foundNoteObject);
                Debug.Log($"  매칭 성공: {noteData} -> {foundNoteObject.name}");
            }
            else
            {
                noteObjects.Add(null); // 매칭 실패 시 null 추가 (인덱스 유지를 위해)
                Debug.LogWarning($"  매칭 실패: {noteData} 에 해당하는 음표 오브젝트를 찾을 수 없습니다.");
            }
        }

        // 매칭을 위한 헬퍼 함수
        string GetNoteNameForMatching(string noteData)
        {
            string notePart = noteData.Split(':')[0];
            string normalizedNote = NormalizeNoteName(notePart);
            // 옥타브가 명시된 경우 "C4" 형식으로 반환, 아니면 "C" 형식으로 반환 (매칭에 따라 다름)
            if (char.IsDigit(notePart[notePart.Length - 1]))
            {
                return normalizedNote + notePart[notePart.Length - 1];
            }
            return normalizedNote;
        }

        Debug.Log($"🎼 최종적으로 매칭된 음표 오브젝트 {noteObjects.Count}개 (null 포함)");
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

        noteObjects.Clear(); // 중요: 이전 게임의 음표 오브젝트 초기화

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

    // SetNoteColor 함수를 수정하여 모든 자식 Image 컴포넌트의 색상 변경
    private void SetNoteColor(int noteIndex, Color color)
    {
        if (noteIndex < 0 || noteIndex >= noteObjects.Count || noteObjects[noteIndex] == null)
        {
            Debug.LogWarning($"⚠️ 음표 인덱스 범위 초과 또는 오브젝트 없음: {noteIndex}");
            return;
        }

        GameObject noteObj = noteObjects[noteIndex];

        // 1. 음표 오브젝트 자체의 Image 컴포넌트 색상 변경 (음표 머리)
        Image noteImage = noteObj.GetComponent<Image>();
        if (noteImage != null)
        {
            noteImage.color = color;
        }
        else
        {
            // Debug.LogWarning($"⚠️ 음표 오브젝트에 Image 컴포넌트가 없습니다: {noteObj.name}");
        }

        // 2. 모든 자식 Image 컴포넌트의 색상 변경 (stem, flag 등)
        Image[] childImages = noteObj.GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            // 이미 부모에서 처리했거나, 특정 제외 대상이 있다면 건너뛰기
            if (img == noteImage) continue; // 부모 Image와 중복 처리 방지

            // 잇단음표의 Beam은 LineRenderer일 수 있으므로 Image가 아님.
            // Image 컴포넌트를 가진 자식들만 색상 변경
            img.color = color;
        }

        // 3. (선택 사항) LineRenderer 색상 변경 (잇단음표의 Beam)
        LineRenderer lineRenderer = noteObj.GetComponentInChildren<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        Debug.Log($"🎼 음표 {noteIndex + 1} ({noteObj.name}) 색상 변경: {color}");
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

    // SongGameController.cs (추가)
    // 이 함수는 NoteSpawner에서 호출되어 음표 오브젝트를 순서대로 추가합니다.
    public void AddNoteObject(GameObject noteObj)
    {
        // 중복 추가 방지 (선택 사항)
        if (!noteObjects.Contains(noteObj))
        {
            noteObjects.Add(noteObj);
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
