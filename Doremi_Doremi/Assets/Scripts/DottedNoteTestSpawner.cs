using UnityEngine;
using System.Collections.Generic;

// 점음표 시스템 테스트를 위한 전용 스포너
public class DottedNoteTestSpawner : MonoBehaviour
{
    [Header("음표 시스템 컴포넌트")]
    public RectTransform staffPanel;
    public ScoreSymbolSpawner scoreSymbolSpawner;
    public NotePlacementHandler notePlacementHandler;
    public NoteAssembler assembler;
    public GameObject staffLinePrefabForBarLine;

    [Header("테스트 곡 선택")]
    [Range(0, 2)]
    public int testSongIndex = 0;

    void Start()
    {
        if (staffPanel == null || scoreSymbolSpawner == null || notePlacementHandler == null || assembler == null)
        {
            Debug.LogError("❌ 필요한 컴포넌트가 할당되지 않았습니다!");
            return;
        }

        GenerateTestSong();
    }

    void GenerateTestSong()
    {
        List<JsonLoader.SongData> testSongs = CreateTestSongs();
        
        if (testSongIndex >= testSongs.Count)
        {
            Debug.LogError($"❌ 잘못된 테스트 곡 인덱스: {testSongIndex}");
            return;
        }

        JsonLoader.SongData selectedSong = testSongs[testSongIndex];
        Debug.Log($"🎯 점음표 테스트 시작: \"{selectedSong.title}\"");

        // 시스템 초기화
        var timeSignature = ParseTimeSignature(selectedSong.timeSignature);
        scoreSymbolSpawner.Initialize(staffPanel, timeSignature);
        notePlacementHandler.Initialize(staffPanel);
        notePlacementHandler.assembler = this.assembler;

        // 악보 레이아웃
        LayoutTestScore(selectedSong);
    }

    private List<JsonLoader.SongData> CreateTestSongs()
    {
        return new List<JsonLoader.SongData>
        {
            // 테스트 곡 1: 기본 점음표 테스트
            new JsonLoader.SongData
            {
                title = "🎯 기본 점음표 테스트",
                clef = "treble",
                timeSignature = "4/4",
                keySignature = "C",
                notes = new List<string>
                {
                    "C4:4.", "D4:8", "E4:4.", "F4:8", "|",
                    "G4:2.", "A4:4", "|",
                    "B4:8.", "C5:16", "D5:4", "REST:4.", "|"
                }
            },

            // 테스트 곡 2: 임시표 + 점음표
            new JsonLoader.SongData
            {
                title = "♯♭ 임시표 점음표 테스트",
                clef = "treble",
                timeSignature = "3/4",
                keySignature = "C",
                notes = new List<string>
                {
                    "C4#:4.", "D4b:8", "E4:4", "|",
                    "F4##:2.", "|",
                    "G4bb:8.", "A4n:16", "B4:2", "|"
                }
            },

            // 테스트 곡 3: 다양한 쉼표 점음표
            new JsonLoader.SongData
            {
                title = "🎵 쉼표 점음표 테스트",
                clef = "treble",
                timeSignature = "4/4",
                keySignature = "C",
                notes = new List<string>
                {
                    "C4:4", "REST:4.", "D4:8", "|",
                    "REST:2.", "E4:4", "|",
                    "F4:8.", "REST:16", "G4:8", "REST:8.", "|"
                }
            }
        };
    }

    private void LayoutTestScore(JsonLoader.SongData song)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float panelWidth = staffPanel.rect.width;
        float leftEdge = -panelWidth * 0.5f;
        float leftMargin = panelWidth * 0.02f;
        float usableWidth = panelWidth * 0.96f;

        float startX = leftEdge + leftMargin;
        float currentX = startX;

        Debug.Log($"🎯 점음표 테스트 레이아웃 시작: {song.title}");

        // 1. 음자리표 생성
        float clefWidth = scoreSymbolSpawner.SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;

        // 2. 조표 생성
        float keySignatureWidth = scoreSymbolSpawner.SpawnKeySignature(currentX, spacing, song.keySignature, song.clef);
        currentX += keySignatureWidth;

        // 3. 박자표 생성
        float timeSignatureWidth = scoreSymbolSpawner.SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;

        // 4. 마디별로 음표 분할
        List<List<NoteData>> measures = SplitIntoMeasures(song.notes);

        if (measures.Count == 0)
        {
            Debug.LogWarning("점음표 테스트용 음표가 없습니다.");
            return;
        }

        // 5. 마디별 레이아웃
        float initialSymbolsWidth = currentX - startX;
        float remainingLayoutWidth = usableWidth - initialSymbolsWidth;
        
        int maxMeasures = Mathf.Min(measures.Count, 3); // 최대 3마디
        float measureWidth = remainingLayoutWidth / maxMeasures;

        for (int measureIndex = 0; measureIndex < maxMeasures; measureIndex++)
        {
            float measureStartX = currentX;
            
            if (measureIndex > 0)
            {
                NoteLayoutHelper.CreateBarLine(measureStartX, staffPanel, staffLinePrefabForBarLine, spacing);
            }

            LayoutMeasure(measures[measureIndex], measureStartX, measureWidth, spacing);
            currentX += measureWidth;
        }

        // 마지막 마디선
        if (maxMeasures > 0)
        {
            NoteLayoutHelper.CreateBarLine(currentX, staffPanel, staffLinePrefabForBarLine, spacing);
        }

        Debug.Log($"✅ 점음표 테스트 악보 완료: {maxMeasures}개 마디");
    }

    private List<List<NoteData>> SplitIntoMeasures(List<string> noteStrings)
    {
        List<List<NoteData>> measures = new List<List<NoteData>>();
        List<NoteData> currentMeasure = new List<NoteData>();

        foreach (string noteString in noteStrings)
        {
            NoteData note = NoteParser.Parse(noteString);
            
            if (note.isBarLine)
            {
                if (currentMeasure.Count > 0)
                {
                    measures.Add(new List<NoteData>(currentMeasure));
                    currentMeasure.Clear();
                }
            }
            else
            {
                currentMeasure.Add(note);
            }
        }

        if (currentMeasure.Count > 0)
        {
            measures.Add(currentMeasure);
        }

        Debug.Log($"🎼 점음표 테스트용 {measures.Count}개 마디 생성");
        return measures;
    }

    private void LayoutMeasure(List<NoteData> notes, float measureStartX, float measureWidth, float spacing)
    {
        if (notes.Count == 0) return;

        float noteSpacing = measureWidth / notes.Count;
        float currentX = measureStartX;

        Debug.Log($"🎵 점음표 테스트 마디: 시작X={measureStartX:F1}, 폭={measureWidth:F1}, 음표수={notes.Count}");

        foreach (NoteData note in notes)
        {
            if (note.isDotted)
            {
                Debug.Log($"🎯 점음표 발견: {note.noteName}:{note.duration}. (점음표)");
            }
            
            notePlacementHandler.SpawnNoteAtPosition(currentX, noteSpacing, spacing, note);
            currentX += noteSpacing;
        }
    }

    private MusicLayoutConfig.TimeSignature ParseTimeSignature(string tsString)
    {
        if (string.IsNullOrEmpty(tsString) || !tsString.Contains("/"))
        {
            return new MusicLayoutConfig.TimeSignature(4, 4);
        }

        string[] parts = tsString.Split('/');
        if (parts.Length == 2 && int.TryParse(parts[0], out int beats) && int.TryParse(parts[1], out int unitType))
        {
            return new MusicLayoutConfig.TimeSignature(beats, unitType);
        }

        return new MusicLayoutConfig.TimeSignature(4, 4);
    }

    // 인스펙터에서 테스트 곡 변경
    [ContextMenu("다음 테스트 곡")]
    public void NextTestSong()
    {
        testSongIndex = (testSongIndex + 1) % 3;
        ClearExistingNotes();
        GenerateTestSong();
    }

    [ContextMenu("이전 테스트 곡")]
    public void PrevTestSong()
    {
        testSongIndex = (testSongIndex - 1 + 3) % 3;
        ClearExistingNotes();
        GenerateTestSong();
    }

    private void ClearExistingNotes()
    {
        if (staffPanel != null)
        {
            for (int i = staffPanel.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(staffPanel.GetChild(i).gameObject);
            }
        }
    }
}