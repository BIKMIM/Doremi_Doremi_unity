using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// NoteSpawner.cs - 단순한 균등 배치 시스템
// 박자 계산 없이 음표 개수별로 균등하게 배치

public class NoteSpawner : MonoBehaviour
{
    [Header("Json 파일 로딩 스크립트가 붙은 오브젝트")]
    public JsonLoader jLoader;

    [Header("노래 번호 - 0번이 첫번째 곡")]
    public int selectedSongIndex = 0;

    [Header("음표 배치 대상 패널")]
    public RectTransform staffPanel;

    // 분리된 스크립트 참조
    [Header("분리된 기능 스크립트")]
    public ScoreSymbolSpawner scoreSymbolSpawner;
    public NotePlacementHandler notePlacementHandler;

    // ModularNoteAssembler로 변경
    public ModularNoteAssembler assembler; // NoteAssembler → ModularNoteAssembler 변경

    // StaffLineDrawer에 있는 linePrefab을 마디선용으로도 재활용
    [Header("오선 프리팹 (마디선용)")]
    public GameObject staffLinePrefabForBarLine;

    [Header("✨ 잇단음표 지원")]
    public bool enableTupletSupport = true;

    [Header("📱 모바일 친화적 레이아웃")]
    [Space(10)]
    [Range(0.8f, 0.95f)]
    [Tooltip("화면 사용 비율 (모바일 최적화)")]
    public float screenUsageRatio = 0.9f;
    
    [Range(0.05f, 0.15f)]
    [Tooltip("마디 내부 여백 비율 (모바일 터치 고려)")]
    public float measurePaddingRatio = 0.08f;

    [Range(0.02f, 0.1f)]
    [Tooltip("화면 가장자리 여백 비율")]
    public float screenMarginRatio = 0.05f;

    [Header("🔧 디버그 설정")]
    [Tooltip("화면 분할 정보 출력")]
    public bool showScreenDivisionDebug = true;

    private MusicLayoutConfig.TimeSignature currentSongTimeSignature;
    private string currentTimeSignatureString;
    private int barLineCount = 0; // 현재 곡의 마디선 개수

    public StaffLineDrawer staffLineDrawer;

    // 🔄 곡 변경 감지를 위한 변수들
    private int lastSelectedSongIndex = -1;
    private bool isInitialized = false;

    void Start()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();

        if (songList == null || selectedSongIndex >= songList.songs.Count)
        {
            Debug.LogError("❌ 유효한 곡이 없습니다.");
            return;
        }

        if (staffLineDrawer == null)
        {
            Debug.LogError("❌ StaffLineDrawer가 할당되지 않았습니다!");
            return;
        }

        JsonLoader.SongData song = songList.songs[selectedSongIndex];
        Debug.Log($"🎵 \"{song.title}\" ({song.timeSignature} 박자) 단순 균등 배치 시작");

        // 박자표 정보 저장
        this.currentTimeSignatureString = song.timeSignature;
        this.currentSongTimeSignature = ParseTimeSignatureFromString(song.timeSignature);

        // 마디선 개수 계산
        this.barLineCount = CountBarLines(song.notes);

        // 분리된 스크립트 초기화
        if (scoreSymbolSpawner == null || notePlacementHandler == null || staffLinePrefabForBarLine == null)
        {
            Debug.LogError("필요한 스크립트 또는 프리팹이 할당되지 않았습니다!");
            return;
        }
        scoreSymbolSpawner.Initialize(staffPanel, currentSongTimeSignature);
        notePlacementHandler.Initialize(staffPanel);
        notePlacementHandler.assembler = this.assembler; // ModularNoteAssembler 할당

        // 단순 균등 레이아웃
        ClearAllAndRedrawStaff();

        // 초기화 완료 표시
        lastSelectedSongIndex = selectedSongIndex;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // selectedSongIndex 변경 감지
        if (selectedSongIndex != lastSelectedSongIndex)
        {
            Debug.Log($"🔄 곡 인덱스 변경 감지: {lastSelectedSongIndex} → {selectedSongIndex}");
            
            JsonLoader.SongList songList = jLoader.LoadSongs();
            if (songList != null && selectedSongIndex >= 0 && selectedSongIndex < songList.songs.Count)
            {
                RefreshCurrentSong();
                lastSelectedSongIndex = selectedSongIndex;
            }
            else
            {
                Debug.LogError($"❌ 잘못된 곡 인덱스: {selectedSongIndex}. 유효 범위: 0-{(songList?.songs.Count ?? 0) - 1}");
                selectedSongIndex = lastSelectedSongIndex;
            }
        }
    }

    /// <summary>
    /// 🎯 마디선 개수 계산
    /// </summary>
    private int CountBarLines(List<string> noteStrings)
    {
        int count = 0;
        foreach (string noteString in noteStrings)
        {
            if (noteString.Trim() == "|")
            {
                count++;
            }
        }
        
        Debug.Log($"📏 마디선 개수: {count}개");
        return count;
    }

    // ✅ 단순한 균등 배치 악보 레이아웃
    private void LayoutCompleteScore(JsonLoader.SongData song)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float panelWidth = staffPanel.rect.width;

        // 🎯 기본 화면 분할
        float leftEdge = -panelWidth * 0.5f;
        float rightEdge = panelWidth * 0.5f;
        float leftMargin = panelWidth * screenMarginRatio;
        float rightMargin = panelWidth * screenMarginRatio;
        
        // 음자리표, 조표, 박자표 생성 (화면 왼쪽 고정)
        float symbolsStartX = leftEdge + leftMargin;
        float currentX = symbolsStartX;
        
        Debug.Log($"🎼 음악 기호 배치 시작: X={symbolsStartX:F1}");
        
        float clefWidth = scoreSymbolSpawner.SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;
        Debug.Log($"   음자리표: 폭={clefWidth:F1}, 다음X={currentX:F1}");

        float keySignatureWidth = scoreSymbolSpawner.SpawnKeySignature(currentX, spacing, song.keySignature, song.clef);
        currentX += keySignatureWidth;
        Debug.Log($"   조표: 폭={keySignatureWidth:F1}, 다음X={currentX:F1}");

        float timeSignatureWidth = scoreSymbolSpawner.SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;
        Debug.Log($"   박자표: 폭={timeSignatureWidth:F1}, 다음X={currentX:F1}");

        // 🔧 수정: 음악 기호 이후의 남은 공간 계산
        float totalSymbolsWidth = currentX - symbolsStartX;
        float noteAreaStartX = currentX + (spacing * 0.5f); // 약간의 간격 추가
        
        // 🔧 수정: 마지막 마디선이 화면 끝에 정확히 오도록 계산
        float noteAreaEndX = rightEdge - rightMargin;
        float totalNoteAreaWidth = noteAreaEndX - noteAreaStartX;
        
        Debug.Log($"   기호 총 폭: {totalSymbolsWidth:F1}");
        Debug.Log($"   음표 영역 시작: X={noteAreaStartX:F1}");
        Debug.Log($"   음표 영역 끝: X={noteAreaEndX:F1}");
        Debug.Log($"   음표 영역 총 폭: {totalNoteAreaWidth:F1}");

        // 마디별로 분할하여 배치
        List<List<object>> measures = SplitIntoMeasures(song.notes);

        if (measures.Count == 0)
        {
            Debug.LogWarning("음표가 없습니다.");
            return;
        }

        // ✅ 마디별 균등 배치
        for (int measureIndex = 0; measureIndex < measures.Count; measureIndex++)
        {
            List<object> measureElements = measures[measureIndex];
            
            // 마디선 생성 (첫 번째 마디가 아닌 경우)
            if (measureIndex > 0)
            {
                // 이전 마디들이 차지한 영역 계산해서 마디선 위치 결정
                float barLineX = CalculateMeasureStartX(measureIndex, measures, noteAreaStartX, totalNoteAreaWidth);
                NoteLayoutHelper.CreateBarLine(barLineX, staffPanel, staffLinePrefabForBarLine, spacing);
            }

            // ✅ 단순 균등 배치로 마디 내 음표들 배치
            LayoutMeasureWithEvenSpacing(measureElements, measureIndex, measures, noteAreaStartX, totalNoteAreaWidth, spacing);
        }

        // 🔧 수정: 마지막 마디선을 화면 오른쪽 끝에 정확히 배치
        if (measures.Count > 0)
        {
            float lastBarLineX = noteAreaEndX; // 오른쪽 여백을 고려한 정확한 위치
            NoteLayoutHelper.CreateBarLine(lastBarLineX, staffPanel, staffLinePrefabForBarLine, spacing);
            Debug.Log($"   마지막 마디선 위치: X={lastBarLineX:F1}");
        }

        Debug.Log($"✅ 단순 균등 배치 악보 완료: {song.clef} + {currentTimeSignatureString} + {measures.Count}개 마디");
    }

    /// <summary>
    /// 🎯 마디 시작 위치 계산 (균등 분할)
    /// </summary>
    private float CalculateMeasureStartX(int measureIndex, List<List<object>> measures, float noteAreaStartX, float totalNoteAreaWidth)
    {
        // 전체 마디 개수로 나누어 균등 분할
        float measureWidth = totalNoteAreaWidth / measures.Count;
        return noteAreaStartX + (measureIndex * measureWidth);
    }

    /// <summary>
    /// 🎯 단순 균등 간격으로 마디 내 음표 배치
    /// </summary>
    private void LayoutMeasureWithEvenSpacing(List<object> elements, int measureIndex, List<List<object>> allMeasures, 
                                            float noteAreaStartX, float totalNoteAreaWidth, float spacing)
    {
        if (elements.Count == 0) return;

        Debug.Log($"🎵 마디 {measureIndex + 1} 균등 배치: 요소수={elements.Count}");

        // 이 마디가 차지할 영역 계산
        float measureWidth = totalNoteAreaWidth / allMeasures.Count;
        float measureStartX = noteAreaStartX + (measureIndex * measureWidth);
        
        // 마디 내부 여백 적용
        float paddingSize = measureWidth * measurePaddingRatio;
        float usableWidth = measureWidth - (paddingSize * 2f);
        float contentStartX = measureStartX + paddingSize;

        Debug.Log($"   마디 시작X={measureStartX:F1}, 사용가능폭={usableWidth:F1}");

        // ✅ 단순 균등 배치: 요소 개수로 나누어 배치
        float elementSpacing = usableWidth / elements.Count;

        for (int i = 0; i < elements.Count; i++)
        {
            object element = elements[i];
            float elementX = contentStartX + (i * elementSpacing);

            if (element is NoteData note)
            {
                notePlacementHandler.SpawnNoteAtPosition(elementX, elementSpacing, spacing, note);
                Debug.Log($"   음표: {note.noteName}({note.duration}분음표) X={elementX:F1}");
            }
            else if (element is TupletData tuplet)
            {
                TupletVisualGroup visualGroup = notePlacementHandler.SpawnTupletGroup(tuplet, elementX, elementSpacing, spacing);

                if (visualGroup != null)
                {
                    Debug.Log($"   잇단음표: {tuplet.GetTupletTypeName()} X={elementX:F1}");
                }
                else
                {
                    Debug.LogError($"   ❌ 잇단음표 생성 실패: {tuplet.GetTupletTypeName()}");
                }
            }
        }

        Debug.Log($"   마디 {measureIndex + 1} 완료: {elements.Count}개 요소를 {elementSpacing:F1}px 간격으로 배치");
    }

    /// <summary>
    /// ✅ 마디별 분할 (단순 버전)
    /// </summary>
    private List<List<object>> SplitIntoMeasures(List<string> noteStrings)
    {
        List<List<object>> measures = new List<List<object>>();

        List<object> parsedElements;

        if (enableTupletSupport)
        {
            parsedElements = TupletParser.ParseWithTuplets(noteStrings);
            Debug.Log($"🎼 잇단음표 파싱 완료: {parsedElements.Count}개 요소");
        }
        else
        {
            parsedElements = new List<object>();
            foreach (string noteString in noteStrings)
            {
                if (noteString.Trim() == "|")
                {
                    // 마디선을 별도 처리 (파싱하지 않고 바로 구분자로 사용)
                    parsedElements.Add(new NoteData { isBarLine = true });
                }
                else
                {
                    parsedElements.Add(NoteParser.Parse(noteString));
                }
            }
            Debug.Log($"🎵 일반 파싱 완료: {parsedElements.Count}개 요소");
        }

        List<object> currentMeasure = new List<object>();

        foreach (object element in parsedElements)
        {
            if (element is NoteData note && note.isBarLine)
            {
                // 마디선을 만나면 현재 마디를 완료하고 새 마디 시작
                if (currentMeasure.Count > 0)
                {
                    measures.Add(new List<object>(currentMeasure));
                    currentMeasure.Clear();
                }
            }
            else
            {
                currentMeasure.Add(element);
            }
        }

        // 마지막 마디 추가
        if (currentMeasure.Count > 0)
        {
            measures.Add(currentMeasure);
        }

        Debug.Log($"🎼 총 {measures.Count}개 마디로 분할 완료");

        // 각 마디의 요소 개수 출력
        for (int i = 0; i < measures.Count; i++)
        {
            Debug.Log($"   마디 {i + 1}: {measures[i].Count}개 요소");
        }

        return measures;
    }

    private MusicLayoutConfig.TimeSignature ParseTimeSignatureFromString(string tsString)
    {
        if (string.IsNullOrEmpty(tsString) || !tsString.Contains("/"))
        {
            Debug.LogWarning($"잘못된 박자표 문자열입니다: {tsString}. 기본값(4/4)을 사용합니다.");
            return new MusicLayoutConfig.TimeSignature(4, 4);
        }

        string[] parts = tsString.Split('/');
        if (parts.Length == 2 && int.TryParse(parts[0], out int beats) && int.TryParse(parts[1], out int unitType))
        {
            return new MusicLayoutConfig.TimeSignature(beats, unitType);
        }

        Debug.LogWarning($"박자표 문자열 파싱에 실패했습니다: {tsString}. 기본값(4/4)을 사용합니다.");
        return new MusicLayoutConfig.TimeSignature(4, 4);
    }

    // 🔄 현재 선택된 곡을 새로고침하는 함수
    public void RefreshCurrentSong()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList == null || selectedSongIndex >= songList.songs.Count)
        {
            Debug.LogError("❌ 유효한 곡이 없습니다.");
            return;
        }

        ClearMusicElements();

        if (staffLineDrawer != null)
        {
            staffLineDrawer.RedrawStaffLines();
        }

        JsonLoader.SongData song = songList.songs[selectedSongIndex];
        Debug.Log($"🎵 곡 새로고침: \"{song.title}\" ({song.timeSignature} 박자)");

        // 박자표 및 마디선 개수 업데이트
        this.currentTimeSignatureString = song.timeSignature;
        this.currentSongTimeSignature = ParseTimeSignatureFromString(song.timeSignature);
        this.barLineCount = CountBarLines(song.notes);
        
        if (scoreSymbolSpawner != null)
        {
            scoreSymbolSpawner.Initialize(staffPanel, currentSongTimeSignature);
        }

        LayoutCompleteScore(song);
    }

    private void ClearMusicElements()
    {
        if (staffPanel != null)
        {
            for (int i = staffPanel.childCount - 1; i >= 0; i--)
            {
                GameObject child = staffPanel.GetChild(i).gameObject;
                if (!child.CompareTag("StaffLine"))
                {
                    DestroyImmediate(child);
                }
            }
        }
    }

    private void ClearAllAndRedrawStaff()
    {
        ClearMusicElements();

        if (staffLineDrawer != null)
        {
            staffLineDrawer.RedrawStaffLines();
        }

        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList != null && selectedSongIndex < songList.songs.Count)
        {
            JsonLoader.SongData song = songList.songs[selectedSongIndex];
            LayoutCompleteScore(song);
        }
    }

    // 🔧 디버깅용 메서드들
    [ContextMenu("다음 곡으로 변경")]
    public void NextSong()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList != null && songList.songs.Count > 0)
        {
            selectedSongIndex = (selectedSongIndex + 1) % songList.songs.Count;
            Debug.Log($"🎵 곡 변경: Index {selectedSongIndex} - {songList.songs[selectedSongIndex].title}");
        }
    }

    [ContextMenu("이전 곡으로 변경")]
    public void PreviousSong()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList != null && songList.songs.Count > 0)
        {
            selectedSongIndex = (selectedSongIndex - 1 + songList.songs.Count) % songList.songs.Count;
            Debug.Log($"🎵 곡 변경: Index {selectedSongIndex} - {songList.songs[selectedSongIndex].title}");
        }
    }

    [ContextMenu("현재 곡 정보 출력")]
    public void PrintCurrentSongInfo()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList != null && selectedSongIndex >= 0 && selectedSongIndex < songList.songs.Count)
        {
            JsonLoader.SongData song = songList.songs[selectedSongIndex];
            Debug.Log($"📋 현재 곡 정보:\n" +
                     $"   인덱스: {selectedSongIndex}\n" +
                     $"   제목: {song.title}\n" +
                     $"   박자: {song.timeSignature}\n" +
                     $"   조표: {song.keySignature}\n" +
                     $"   음표 수: {song.notes.Count}\n" +
                     $"   마디선 수: {barLineCount}개");
        }
    }

    [ContextMenu("화면 사용 비율 증가")]
    public void IncreaseScreenUsage()
    {
        screenUsageRatio = Mathf.Min(screenUsageRatio + 0.05f, 0.95f);
        Debug.Log($"📱 화면 사용 비율 증가: {screenUsageRatio:F2}");
        RefreshCurrentSong();
    }

    [ContextMenu("화면 사용 비율 감소")]
    public void DecreaseScreenUsage()
    {
        screenUsageRatio = Mathf.Max(screenUsageRatio - 0.05f, 0.8f);
        Debug.Log($"📱 화면 사용 비율 감소: {screenUsageRatio:F2}");
        RefreshCurrentSong();
    }

    [ContextMenu("마디 여백 증가")]
    public void IncreaseMeasurePadding()
    {
        measurePaddingRatio = Mathf.Min(measurePaddingRatio + 0.02f, 0.15f);
        Debug.Log($"📱 마디 여백 증가: {measurePaddingRatio:F2}");
        RefreshCurrentSong();
    }

    [ContextMenu("마디 여백 감소")]
    public void DecreaseMeasurePadding()
    {
        measurePaddingRatio = Mathf.Max(measurePaddingRatio - 0.02f, 0.05f);
        Debug.Log($"📱 마디 여백 감소: {measurePaddingRatio:F2}");
        RefreshCurrentSong();
    }

    [ContextMenu("설정 리셋")]
    public void ResetSettings()
    {
        screenUsageRatio = 0.9f;
        measurePaddingRatio = 0.08f;
        screenMarginRatio = 0.05f;
        showScreenDivisionDebug = true;
        
        Debug.Log("🔄 설정이 기본값으로 리셋되었습니다.");
        RefreshCurrentSong();
    }

    [ContextMenu("현재 설정 정보")]
    public void PrintCurrentSettings()
    {
        Debug.Log($"📱 현재 설정:");
        Debug.Log($"   박자표: {currentTimeSignatureString}");
        Debug.Log($"   마디선 개수: {barLineCount}개");
        Debug.Log($"   화면 사용 비율: {screenUsageRatio:F2}");
        Debug.Log($"   마디 여백 비율: {measurePaddingRatio:F2}");
        Debug.Log($"   화면 여백 비율: {screenMarginRatio:F2}");
        Debug.Log($"   잇단음표 지원: {(enableTupletSupport ? "활성화" : "비활성화")}");
    }
}
