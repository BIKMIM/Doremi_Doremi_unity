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
        if (scoreSymbolSpawner == null || notePlacementHandler == null)
        {
            Debug.LogError("분리된 스크립트(ScoreSymbolSpawner, NotePlacementHandler)를 할당해주세요!");
            return;
        }
        scoreSymbolSpawner.Initialize(staffPanel, currentSongTimeSignature);
        notePlacementHandler.Initialize(staffPanel);
        notePlacementHandler.assembler = this.assembler; // NoteAssembler 연결

        // 🎯 해상도 독립적 비율 기반 레이아웃
        LayoutCompleteScore(song);
    }

    // NoteSpawner.cs의 LayoutCompleteScore 함수 수정
    private void LayoutCompleteScore(JsonLoader.SongData song)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);

        float panelWidth = staffPanel.rect.width;
        float leftEdge = -panelWidth * 0.5f;
        float leftMargin = panelWidth * 0.02f;
        float usableWidth = panelWidth * 0.96f;

        float startX = leftEdge + leftMargin;
        float currentX = startX;

        Debug.Log($"🎯 패널 기준 레이아웃: 패널너비={panelWidth:F1}, 왼쪽끝={leftEdge:F1}, 시작X={startX:F1}");

        // 1. 🎼 음자리표 생성 (분리된 스크립트 호출)
        float clefWidth = scoreSymbolSpawner.SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;

        // 2. 🎼 조표 생성 (분리된 스크립트 호출)
        float keySignatureWidth = scoreSymbolSpawner.SpawnKeySignature(currentX, spacing, song.keySignature, song.clef);
        currentX += keySignatureWidth;

        // 3. 🎵 박자표 생성 (분리된 스크립트 호출)
        float timeSignatureWidth = scoreSymbolSpawner.SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;

        // 4. 🎶 음표들 배치 (남은 공간에 균등 배치)
        float usedWidth = clefWidth + timeSignatureWidth + keySignatureWidth; // 조표 너비 추가
        float remainingWidth = usableWidth - usedWidth - (startX - leftEdge);
        float noteSpacing = remainingWidth / song.notes.Count;

        Debug.Log($"🎯 완전한 레이아웃: 음자리표={clefWidth:F1}, 조표={keySignatureWidth:F1}, 박자표={timeSignatureWidth:F1}, 남은공간={remainingWidth:F1}, 음표간격={noteSpacing:F1}");

        int order = 0;
        foreach (string rawNote in song.notes)
        {
            NoteData note = NoteParser.Parse(rawNote);

            if (note.isRest)
            {
                notePlacementHandler.SpawnRestAtPosition(currentX, noteSpacing, spacing, note);
            }
            else
            {
                notePlacementHandler.SpawnNoteAtPosition(currentX, noteSpacing, spacing, note);
            }

            currentX += noteSpacing;
            order++;
        }

        Debug.Log($"✅ 패널 기준 악보 완료: {song.clef} 음자리표 + 박자표 + {order}개 음표");
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
}