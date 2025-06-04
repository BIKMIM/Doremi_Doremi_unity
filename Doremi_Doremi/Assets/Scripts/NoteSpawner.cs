using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// NoteSpawner.cs - 해상도 독립적 음표 생성 시스템 + 잇단음표 지원
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

    private MusicLayoutConfig.TimeSignature currentSongTimeSignature;

    public StaffLineDrawer staffLineDrawer; // StaffLineDrawer 컴포넌트 참조 추가

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

            // ✅ 잇단음표 지원 마디 배치
            LayoutMeasureWithTuplets(measures[measureIndex], measureStartX, measureWidth, spacing);

            // 다음 마디 위치로 이동
            currentX += measureWidth;
        }

        // 마지막 마디선 생성
        if (maxMeasures > 0)
        {
            NoteLayoutHelper.CreateBarLine(currentX, staffPanel, staffLinePrefabForBarLine, spacing);
        }

        Debug.Log($"✅ 잇단음표 지원 악보 완료: {song.clef} 음자리표 + 박자표 + {maxMeasures}개 마디");
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

    // ✅ 잇단음표 지원 개별 마디 레이아웃 함수
    private void LayoutMeasureWithTuplets(List<object> elements, float measureStartX, float measureWidth, float spacing)
    {
        if (elements.Count == 0) return;

        Debug.Log($"🎵 잇단음표 지원 마디 레이아웃: 시작X={measureStartX:F1}, 폭={measureWidth:F1}, 요소수={elements.Count}");

        float currentX = measureStartX;
        float remainingWidth = measureWidth;

        // 요소별 폭 계산 및 배치
        for (int i = 0; i < elements.Count; i++)
        {
            object element = elements[i];

            if (element is NoteData note)
            {
                // 일반 음표 처리
                float noteWidth = remainingWidth / (elements.Count - i); // 남은 폭을 남은 요소 수로 분배
                notePlacementHandler.SpawnNoteAtPosition(currentX, noteWidth, spacing, note);
                currentX += noteWidth;
                remainingWidth -= noteWidth;

                Debug.Log($"   일반음표: {note.noteName}, 폭={noteWidth:F1}");
            }
            else if (element is TupletData tuplet)
            {
                // 잇단음표 그룹 처리
                float tupletWidth = remainingWidth / (elements.Count - i); // 임시 폭 할당

                TupletVisualGroup visualGroup = notePlacementHandler.SpawnTupletGroup(tuplet, currentX, tupletWidth, spacing);

                if (visualGroup != null)
                {
                    float actualWidth = tuplet.totalWidth;
                    currentX += actualWidth;
                    remainingWidth -= actualWidth;

                    Debug.Log($"   잇단음표: {tuplet.GetTupletTypeName()}, 폭={actualWidth:F1}");
                }
                else
                {
                    Debug.LogError($"   ❌ 잇단음표 생성 실패: {tuplet.GetTupletTypeName()}");
                    currentX += tupletWidth; // 실패해도 위치는 이동
                    remainingWidth -= tupletWidth;
                }
            }
            else
            {
                Debug.LogWarning($"   ⚠️ 알 수 없는 요소 타입: {element?.GetType().Name}");
            }
        }
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

        ClearAllAndRedrawStaff(); // 모든 것을 지우고 오선을 다시 그림
    }



    // 모든 악보 요소를 지우고 오선을 다시 그리는 새로운 통합 함수
    private void ClearAllAndRedrawStaff()
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
        // 오선을 다시 그림
        staffLineDrawer.RedrawStaffLines(); // StaffLineDrawer에 추가한 public 함수 호출

        // 악보를 처음부터 다시 로드하고 레이아웃
        JsonLoader.SongData song = jLoader.LoadSongs().songs[selectedSongIndex];
        LayoutCompleteScore(song);
    }



}