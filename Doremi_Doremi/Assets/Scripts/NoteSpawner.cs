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

    // 기존 noteIndexTable, trebleKeySignaturePositions, bassKeySignaturePositions, lineNotes는 NotePositioningData.cs로 이동했으므로 삭제


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
        if (scoreSymbolSpawner == null || notePlacementHandler == null || staffLinePrefabForBarLine == null) // 마디선 프리팹 추가
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


    // 기존 LayoutCompleteScore 함수는 NoteLayoutHelper.cs로 이동했으므로 삭제
    // NoteSpawner.cs의 LayoutCompleteScore 함수 수정
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

        // 4. 🎶 음표들 배치를 위한 공간 계산 및 마디선 추가
        float initialSymbolsWidth = currentX - startX;
        float remainingLayoutWidth = usableWidth - initialSymbolsWidth;

        // 2마디로 나누므로, 남은 공간의 중앙에 마디선 배치
        float barLineXPosition = startX + initialSymbolsWidth + remainingLayoutWidth * 0.5f;
        // 🎯 NoteLayoutHelper.CreateBarLine 호출 시 staffSpacing 전달
        NoteLayoutHelper.CreateBarLine(barLineXPosition, staffPanel, staffLinePrefabForBarLine, spacing); // spacing 인자 추가

        // ... (이후 음표 배치 로직은 동일) ...

        int totalNotes = song.notes.Count;
        int notesInFirstMeasure = Mathf.CeilToInt(totalNotes / 2f);
        int notesInSecondMeasure = totalNotes - notesInFirstMeasure;

        float firstMeasureUsableWidth = remainingLayoutWidth * 0.5f;
        float firstMeasureNoteSpacing = notesInFirstMeasure > 0 ? firstMeasureUsableWidth / notesInFirstMeasure : 0;

        Debug.Log($"🎯 첫 번째 마디: 사용가능너비={firstMeasureUsableWidth:F1}, 음표수={notesInFirstMeasure}, 음표간격={firstMeasureNoteSpacing:F1}");

        for (int i = 0; i < notesInFirstMeasure; i++)
        {
            NoteData note = NoteParser.Parse(song.notes[i]);
            notePlacementHandler.SpawnNoteAtPosition(currentX, firstMeasureNoteSpacing, spacing, note);
            currentX += firstMeasureNoteSpacing;
        }

        currentX = barLineXPosition;

        float secondMeasureUsableWidth = remainingLayoutWidth * 0.5f;
        float secondMeasureNoteSpacing = notesInSecondMeasure > 0 ? secondMeasureUsableWidth / notesInSecondMeasure : 0;

        Debug.Log($"🎯 두 번째 마디: 사용가능너비={secondMeasureUsableWidth:F1}, 음표수={notesInSecondMeasure}, 음표간격={secondMeasureNoteSpacing:F1}");

        for (int i = notesInFirstMeasure; i < totalNotes; i++)
        {
            NoteData note = NoteParser.Parse(song.notes[i]);
            notePlacementHandler.SpawnNoteAtPosition(currentX, secondMeasureNoteSpacing, spacing, note);
            currentX += secondMeasureNoteSpacing;
        }

        Debug.Log($"✅ 패널 기준 악보 완료: {song.clef} 음자리표 + 박자표 + {totalNotes}개 음표, 2마디 분할");
    }




    // 🎼 박자표 문자열을 파싱하여 MusicLayoutConfig.TimeSignature 객체로 변환
    // ParseTimeSignatureFromString 함수 유지
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