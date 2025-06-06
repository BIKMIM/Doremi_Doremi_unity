using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// NoteSpawner.cs - 해상도 독립적 음표 생성 시스템 + 박자 기반 공간 배분
// 모든 크기와 위치를 비율 기반으로 계산하여 어떤 해상도에서도 동일한 비율로 표시

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

    // (NoteAssembler는 그대로 유지)
    public NoteAssembler assembler;

    // StaffLineDrawer에 있는 linePrefab을 마디선용으로도 재활용
    [Header("오선 프리팹 (마디선용)")]
    public GameObject staffLinePrefabForBarLine; // StaffLineDrawer의 linePrefab을 여기에 연결

    [Header("✨ 잇단음표 지원")]
    public bool enableTupletSupport = true; // 잇단음표 기능 활성화

    [Header("🎯 박자 기반 레이아웃 설정")]
    [Range(0.1f, 0.5f)]
    public float measureMarginRatio = 0.1f; // 마디 내 여백 비율 (10%)
    
    [Range(1.0f, 3.0f)]
    public float beatSpacingMultiplier = 1.5f; // 박자 간격 배수

    private MusicLayoutConfig.TimeSignature currentSongTimeSignature;

    public StaffLineDrawer staffLineDrawer; // StaffLineDrawer 컴포넌트 참조 추가

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

        // staffLineDrawer 초기화 확인 (Start() 또는 ValidateComponents()에서)
        if (staffLineDrawer == null)
        {
            Debug.LogError("❌ StaffLineDrawer가 할당되지 않았습니다!");
            return;
        }

        JsonLoader.SongData song = songList.songs[selectedSongIndex];
        Debug.Log($"🎵 \"{song.title}\"의 음표 {song.notes.Count}개 생성 시작");

        this.currentSongTimeSignature = ParseTimeSignatureFromString(song.timeSignature);

        // 분리된 스크립트 초기화
        if (scoreSymbolSpawner == null || notePlacementHandler == null || staffLinePrefabForBarLine == null)
        {
            Debug.LogError("필요한 스크립트 또는 프리팹이 할당되지 않았습니다!");
            return;
        }
        scoreSymbolSpawner.Initialize(staffPanel, currentSongTimeSignature);
        notePlacementHandler.Initialize(staffPanel);
        notePlacementHandler.assembler = this.assembler; // NoteAssembler 연결

        // 🎯 해상도 독립적 비율 기반 레이아웃
        ClearAllAndRedrawStaff(); // 이 함수가 악보 로드 및 레이아웃까지 담당합니다.

        // 초기화 완료 표시
        lastSelectedSongIndex = selectedSongIndex;
        isInitialized = true;
    }

    // 🔄 매 프레임 곡 변경을 감지하는 Update 메서드
    void Update()
    {
        if (!isInitialized) return;

        // selectedSongIndex 변경 감지
        if (selectedSongIndex != lastSelectedSongIndex)
        {
            Debug.Log($"🔄 곡 인덱스 변경 감지: {lastSelectedSongIndex} → {selectedSongIndex}");
            
            // 유효한 인덱스인지 확인
            JsonLoader.SongList songList = jLoader.LoadSongs();
            if (songList != null && selectedSongIndex >= 0 && selectedSongIndex < songList.songs.Count)
            {
                RefreshCurrentSong();
                lastSelectedSongIndex = selectedSongIndex;
            }
            else
            {
                Debug.LogError($"❌ 잘못된 곡 인덱스: {selectedSongIndex}. 유효 범위: 0-{(songList?.songs.Count ?? 0) - 1}");
                selectedSongIndex = lastSelectedSongIndex; // 이전 값으로 복원
            }
        }
    }

    // ✅ 잇단음표 지원 마디별 레이아웃 새로운 방식
    private void LayoutCompleteScore(JsonLoader.SongData song)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);

        float panelWidth = staffPanel.rect.width;
        float leftEdge = -panelWidth * 0.5f;
        float leftMargin = panelWidth * 0.02f;
        float rightMargin = panelWidth * 0.02f;
        float usableWidth = panelWidth * (1.0f - 0.02f - 0.02f);

        float startX = leftEdge + leftMargin;
        float currentX = startX;

        Debug.Log($"🎯 패널 기준 레이아웃: 패널너비={panelWidth:F1}, 왼쪽끝={leftEdge:F1}, 시작X={startX:F1}");

        // 1. 🎼 음자리표 생성
        float clefWidth = scoreSymbolSpawner.SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;

        // 2. 🎼 조표 생성
        float keySignatureWidth = scoreSymbolSpawner.SpawnKeySignature(currentX, spacing, song.keySignature, song.clef);
        currentX += keySignatureWidth;

        // 3. 🎵 박자표 생성
        float timeSignatureWidth = scoreSymbolSpawner.SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;

        // ✅ 4. 잇단음표 지원 마디별 분할
        List<List<object>> measures = SplitIntoMeasuresWithTuplets(song.notes);

        if (measures.Count == 0)
        {
            Debug.LogWarning("음표가 없습니다.");
            return;
        }

        // 5. 🎶 마디별 레이아웃 (최대 2마디)
        float initialSymbolsWidth = currentX - startX;
        float remainingLayoutWidth = usableWidth - initialSymbolsWidth;

        int maxMeasures = Mathf.Min(measures.Count, 2); // 최대 2마디
        float measureWidth = remainingLayoutWidth / maxMeasures;

        for (int measureIndex = 0; measureIndex < maxMeasures; measureIndex++)
        {
            // 마디 시작 위치
            float measureStartX = currentX;

            // 마디선 생성 (첫 번째 마디가 아닌 경우)
            if (measureIndex > 0)
            {
                NoteLayoutHelper.CreateBarLine(measureStartX, staffPanel, staffLinePrefabForBarLine, spacing);
            }

            // ✅ 박자 기반 마디 배치 (개선됨)
            LayoutMeasureWithBeatBasedSpacing(measures[measureIndex], measureStartX, measureWidth, spacing);

            // 다음 마디 위치로 이동
            currentX += measureWidth;
        }

        // 마지막 마디선 생성
        if (maxMeasures > 0)
        {
            NoteLayoutHelper.CreateBarLine(currentX, staffPanel, staffLinePrefabForBarLine, spacing);
        }

        Debug.Log($"✅ 박자 기반 악보 완료: {song.clef} 음자리표 + 박자표 + {maxMeasures}개 마디");
    }

    // ✅ 잇단음표 지원 마디별 분할 함수
    private List<List<object>> SplitIntoMeasuresWithTuplets(List<string> noteStrings)
    {
        List<List<object>> measures = new List<List<object>>();

        // 1. 먼저 잇단음표 파싱
        List<object> parsedElements;

        if (enableTupletSupport)
        {
            parsedElements = TupletParser.ParseWithTuplets(noteStrings);
            Debug.Log($"🎼 잇단음표 파싱 완료: {parsedElements.Count}개 요소");
        }
        else
        {
            // 잇단음표 비활성화 시 기존 방식
            parsedElements = new List<object>();
            foreach (string noteString in noteStrings)
            {
                parsedElements.Add(NoteParser.Parse(noteString));
            }
            Debug.Log($"🎵 일반 파싱 완료: {parsedElements.Count}개 요소");
        }

        // 2. 마디별로 분할
        List<object> currentMeasure = new List<object>();

        foreach (object element in parsedElements)
        {
            if (element is NoteData note && note.isBarLine)
            {
                // 마디구분선 발견
                if (currentMeasure.Count > 0)
                {
                    measures.Add(new List<object>(currentMeasure));
                    currentMeasure.Clear();
                    Debug.Log($"마디 {measures.Count} 완료: {currentMeasure.Count}개 요소");
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
            Debug.Log($"마지막 마디 {measures.Count} 완료: {currentMeasure.Count}개 요소");
        }

        Debug.Log($"🎼 총 {measures.Count}개 마디로 분할 완료 (잣단음표 지원)");
        return measures;
    }

    // 🎯 NEW: 박자 기반 공간 배분 마디 레이아웃 함수
    private void LayoutMeasureWithBeatBasedSpacing(List<object> elements, float measureStartX, float measureWidth, float spacing)
    {
        if (elements.Count == 0) return;

        Debug.Log($"🎵 박자 기반 마디 레이아웃: 시작X={measureStartX:F1}, 폭={measureWidth:F1}, 요소수={elements.Count}");

        // 1. 📊 총 박자 수 계산
        float totalBeats = CalculateTotalBeats(elements);
        
        // 2. 🎯 박자당 공간 계산 (여백 고려)
        float usableWidth = measureWidth * (1f - measureMarginRatio * 2f);
        float leftMargin = measureWidth * measureMarginRatio;
        float beatSpacing = (usableWidth / totalBeats) * beatSpacingMultiplier;
        
        Debug.Log($"   총박자: {totalBeats:F2}, 사용가능폭: {usableWidth:F1}, 박자간격: {beatSpacing:F1}");

        // 3. 🎶 요소별 배치
        float currentX = measureStartX + leftMargin;

        for (int i = 0; i < elements.Count; i++)
        {
            object element = elements[i];

            if (element is NoteData note)
            {
                // 일반 음표의 박자 값 계산
                float noteBeats = CalculateNoteBeatValue(note);
                float noteWidth = beatSpacing * noteBeats;

                notePlacementHandler.SpawnNoteAtPosition(currentX, noteWidth, spacing, note);
                
                Debug.Log($"   일반음표: {note.noteName}({note.duration}분음표) = {noteBeats:F2}박자, 폭={noteWidth:F1}");
                
                currentX += noteWidth;
            }
            else if (element is TupletData tuplet)
            {
                // 잇단음표의 박자 값 계산 (예: 4잇단음표:2 = 2박자)
                float tupletBeats = tuplet.beatValue * 0.25f; // 예: beatValue=8이면 2박자
                float tupletWidth = beatSpacing * tupletBeats;

                TupletVisualGroup visualGroup = notePlacementHandler.SpawnTupletGroup(tuplet, currentX, tupletWidth, spacing);

                if (visualGroup != null)
                {
                    Debug.Log($"   잇단음표: {tuplet.GetTupletTypeName()} = {tupletBeats:F2}박자, 폭={tupletWidth:F1}");
                    currentX += tupletWidth;
                }
                else
                {
                    Debug.LogError($"   ❌ 잇단음표 생성 실패: {tuplet.GetTupletTypeName()}");
                    currentX += tupletWidth; // 실패해도 위치는 이동
                }
            }
            else
            {
                Debug.LogWarning($"   ⚠️ 알 수 없는 요소 타입: {element?.GetType().Name}");
            }
        }

        Debug.Log($"   마디 배치 완료: 최종X={currentX:F1} (시작X={measureStartX:F1})");
    }

    // 🎯 마디 내 총 박자 수 계산
    private float CalculateTotalBeats(List<object> elements)
    {
        float totalBeats = 0f;

        foreach (object element in elements)
        {
            if (element is NoteData note)
            {
                totalBeats += CalculateNoteBeatValue(note);
            }
            else if (element is TupletData tuplet)
            {
                // 잇단음표는 beatValue 사용 (예: TUPLET_START:4:2에서 2는 beatValue)
                totalBeats += tuplet.beatValue * 0.25f; // 4분음표 단위로 변환
            }
        }

        return totalBeats;
    }

    // 🎯 개별 음표의 박자 값 계산
    private float CalculateNoteBeatValue(NoteData note)
    {
        // duration: 1(온음표)=4박자, 2(2분음표)=2박자, 4(4분음표)=1박자, 8(8분음표)=0.5박자
        float beatValue = 4f / note.duration;
        
        // 점음표는 1.5배
        if (note.isDotted)
        {
            beatValue *= 1.5f;
        }
        
        return beatValue;
    }

    // 🎼 박자표 문자열을 파싱하여 MusicLayoutConfig.TimeSignature 객체로 변환
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

    // ✅ 잇단음표 기능 토글 (런타임에서 테스트용)
    [ContextMenu("잇단음표 기능 토글")]
    public void ToggleTupletSupport()
    {
        enableTupletSupport = !enableTupletSupport;
        Debug.Log($"잇단음표 기능: {(enableTupletSupport ? "활성화" : "비활성화")}");

        RefreshCurrentSong(); // ClearAllAndRedrawStaff 대신 새 함수 사용
    }

    // 🎯 곡 변경을 위한 새로운 public 메서드
    [ContextMenu("다음 곡으로 변경")]
    public void NextSong()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList != null && songList.songs.Count > 0)
        {
            selectedSongIndex = (selectedSongIndex + 1) % songList.songs.Count;
            Debug.Log($"🎵 곡 변경: Index {selectedSongIndex} - {songList.songs[selectedSongIndex].title}");
            // Update()에서 자동으로 RefreshCurrentSong() 호출됨
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
            // Update()에서 자동으로 RefreshCurrentSong() 호출됨
        }
    }

    // 🔄 현재 선택된 곡을 새로고침하는 함수 (무한 재귀 방지)
    public void RefreshCurrentSong()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList == null || selectedSongIndex >= songList.songs.Count)
        {
            Debug.LogError("❌ 유효한 곡이 없습니다.");
            return;
        }

        // 기존 악보 요소들만 제거 (오선은 유지)
        ClearMusicElements();

        // 오선 다시 그리기 (StaffLineDrawer 직접 호출 - 재귀 방지)
        if (staffLineDrawer != null)
        {
            staffLineDrawer.RedrawStaffLines();
        }

        // 선택된 곡 로드 및 레이아웃
        JsonLoader.SongData song = songList.songs[selectedSongIndex];
        Debug.Log($"🎵 곡 새로고침: \"{song.title}\" (Index: {selectedSongIndex})");

        // 박자표 업데이트
        this.currentSongTimeSignature = ParseTimeSignatureFromString(song.timeSignature);
        if (scoreSymbolSpawner != null)
        {
            scoreSymbolSpawner.Initialize(staffPanel, currentSongTimeSignature);
        }

        LayoutCompleteScore(song);
    }

    // 🧹 음악 요소만 제거하는 함수 (오선은 유지)
    private void ClearMusicElements()
    {
        if (staffPanel != null)
        {
            // 오선을 제외한 모든 자식 오브젝트 파괴
            for (int i = staffPanel.childCount - 1; i >= 0; i--)
            {
                GameObject child = staffPanel.GetChild(i).gameObject;
                if (!child.CompareTag("StaffLine")) // "StaffLine" 태그가 없는 오브젝트만 파괴
                {
                    DestroyImmediate(child);
                }
            }
        }
    }

    // ⚠️ 기존 ClearAllAndRedrawStaff 함수는 Start()에서만 사용하도록 수정
    private void ClearAllAndRedrawStaff()
    {
        // 기존 악보 요소들만 제거
        ClearMusicElements();

        // 오선 다시 그리기 (Start()에서만 호출되므로 안전)
        if (staffLineDrawer != null)
        {
            staffLineDrawer.RedrawStaffLines();
        }

        // 현재 선택된 곡으로 레이아웃
        JsonLoader.SongList songList = jLoader.LoadSongs();
        if (songList != null && selectedSongIndex < songList.songs.Count)
        {
            JsonLoader.SongData song = songList.songs[selectedSongIndex];
            LayoutCompleteScore(song);
        }
    }

    // 🔧 디버깅용 메서드들
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
                     $"   음표 수: {song.notes.Count}");
        }
        else
        {
            Debug.LogError($"❌ 잘못된 곡 인덱스: {selectedSongIndex}");
        }
    }

    // 🎯 박자 기반 레이아웃 설정 조정 (런타임 테스트용)
    [ContextMenu("박자 간격 증가")]
    public void IncreaseBeatSpacing()
    {
        beatSpacingMultiplier = Mathf.Min(beatSpacingMultiplier + 0.2f, 3.0f);
        Debug.Log($"박자 간격 배수: {beatSpacingMultiplier:F1}");
        RefreshCurrentSong();
    }

    [ContextMenu("박자 간격 감소")]
    public void DecreaseBeatSpacing()
    {
        beatSpacingMultiplier = Mathf.Max(beatSpacingMultiplier - 0.2f, 1.0f);
        Debug.Log($"박자 간격 배수: {beatSpacingMultiplier:F1}");
        RefreshCurrentSong();
    }
}