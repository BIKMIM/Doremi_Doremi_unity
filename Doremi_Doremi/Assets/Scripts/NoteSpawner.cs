using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// NoteSpawner.cs - 해상도 독립적 음표 생성 시스템
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

    private MusicLayoutConfig.TimeSignature currentSongTimeSignature;

    void Start()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();

        if (songList == null || selectedSongIndex >= songList.songs.Count)
        {
            Debug.LogError("❌ 유효한 곡이 없습니다.");
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
        LayoutCompleteScore(song);
    }

    // ✅ 마디별 레이아웃 새로운 방식
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

        // ✅ 4. 마디별로 음표 분할
        List<List<NoteData>> measures = SplitIntoMeasures(song.notes);
        
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

            // 마디 내 음표들 배치
            LayoutMeasure(measures[measureIndex], measureStartX, measureWidth, spacing);
            
            // 다음 마디 위치로 이동
            currentX += measureWidth;
        }

        // 마지막 마디선 생성
        if (maxMeasures > 0)
        {
            NoteLayoutHelper.CreateBarLine(currentX, staffPanel, staffLinePrefabForBarLine, spacing);
        }

        Debug.Log($"✅ 마디별 악보 완료: {song.clef} 음자리표 + 박자표 + {maxMeasures}개 마디");
    }

    // ✅ 음표를 마디별로 분할하는 함수
    private List<List<NoteData>> SplitIntoMeasures(List<string> noteStrings)
    {
        List<List<NoteData>> measures = new List<List<NoteData>>();
        List<NoteData> currentMeasure = new List<NoteData>();

        foreach (string noteString in noteStrings)
        {
            NoteData note = NoteParser.Parse(noteString);
            
            if (note.isBarLine) // 마디구분선
            {
                if (currentMeasure.Count > 0)
                {
                    measures.Add(new List<NoteData>(currentMeasure));
                    currentMeasure.Clear();
                    Debug.Log($"마디 {measures.Count} 완료: {currentMeasure.Count}개 음표");
                }
            }
            else
            {
                currentMeasure.Add(note);
            }
        }

        // 마지막 마디 추가 (마디구분선이 없는 경우)
        if (currentMeasure.Count > 0)
        {
            measures.Add(currentMeasure);
            Debug.Log($"마지막 마디 {measures.Count} 완료: {currentMeasure.Count}개 음표");
        }

        Debug.Log($"🎼 총 {measures.Count}개 마디로 분할 완료");
        return measures;
    }

    // ✅ 개별 마디 레이아웃 함수
    private void LayoutMeasure(List<NoteData> notes, float measureStartX, float measureWidth, float spacing)
    {
        if (notes.Count == 0) return;

        float noteSpacing = measureWidth / notes.Count;
        float currentX = measureStartX;

        Debug.Log($"🎵 마디 레이아웃: 시작X={measureStartX:F1}, 폭={measureWidth:F1}, 음표수={notes.Count}, 간격={noteSpacing:F1}");

        foreach (NoteData note in notes)
        {
            notePlacementHandler.SpawnNoteAtPosition(currentX, noteSpacing, spacing, note);
            currentX += noteSpacing;
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
}