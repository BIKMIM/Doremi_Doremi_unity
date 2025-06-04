using UnityEngine;
using System.Collections.Generic;

// 잇단음표 시각적 테스트 스크립트
public class TupletVisualTest : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    public RectTransform staffPanel;
    public ScoreSymbolSpawner scoreSymbolSpawner;
    public NotePlacementHandler notePlacementHandler;
    public NoteAssembler assembler;
    public GameObject staffLinePrefabForBarLine;

    [Header("테스트 설정")]
    [Range(0, 2)]
    public int testCaseIndex = 0;
    public bool autoStart = true;

    void Start()
    {
        if (autoStart)
        {
            TestTupletVisuals();
        }
    }

    [ContextMenu("잇단음표 시각적 테스트")]
    public void TestTupletVisuals()
    {
        if (!ValidateComponents())
        {
            Debug.LogError("❌ 필수 컴포넌트가 할당되지 않았습니다!");
            return;
        }

        Debug.Log("🎼 === 잇단음표 시각적 테스트 시작 ===");

        // 화면 지우기
        ClearStaff();

        // 테스트 케이스 선택
        List<JsonLoader.SongData> testCases = CreateTestCases();
        
        if (testCaseIndex >= testCases.Count)
        {
            Debug.LogError($"❌ 잘못된 테스트 케이스 인덱스: {testCaseIndex}");
            return;
        }

        JsonLoader.SongData testSong = testCases[testCaseIndex];
        Debug.Log($"🎯 테스트 케이스 {testCaseIndex}: {testSong.title}");

        // 시스템 초기화
        var timeSignature = ParseTimeSignature(testSong.timeSignature);
        scoreSymbolSpawner.Initialize(staffPanel, timeSignature);
        notePlacementHandler.Initialize(staffPanel);
        notePlacementHandler.assembler = assembler;

        // 악보 레이아웃
        LayoutTestScore(testSong);
    }

    private List<JsonLoader.SongData> CreateTestCases()
    {
        return new List<JsonLoader.SongData>
        {
            // 테스트 케이스 0: 기본 셋잇단음표
            new JsonLoader.SongData
            {
                title = "🎯 기본 셋잇단음표 테스트",
                clef = "treble",
                timeSignature = "2/4",
                keySignature = "C",
                notes = new List<string>
                {
                    "C4:4",
                    "TUPLET_START:3:2",
                    "D4:8", "E4:8", "F4:8",
                    "TUPLET_END",
                    "|"
                }
            },

            // 테스트 케이스 1: 넷잇단음표
            new JsonLoader.SongData
            {
                title = "🎼 넷잇단음표 테스트",
                clef = "treble",
                timeSignature = "4/4",
                keySignature = "C",
                notes = new List<string>
                {
                    "C4:4",
                    "TUPLET_START:4:3",
                    "D4:16", "E4:16", "F4:16", "G4:16",
                    "TUPLET_END",
                    "A4:4",
                    "|"
                }
            },

            // 테스트 케이스 2: 복합 테스트 (임시표 + 점음표 + 잇단음표)
            new JsonLoader.SongData
            {
                title = "🎵 복합 잇단음표 테스트",
                clef = "treble",
                timeSignature = "3/4",
                keySignature = "C",
                notes = new List<string>
                {
                    "C4#:4",
                    "TUPLET_START:3:2",
                    "D4b:8.", "E4:16", "F4n:8",
                    "TUPLET_END",
                    "G4:2",
                    "|"
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

        Debug.Log($"🎯 테스트 레이아웃 시작: {song.title}");

        // 1. 음자리표
        float clefWidth = scoreSymbolSpawner.SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;

        // 2. 조표
        float keySignatureWidth = scoreSymbolSpawner.SpawnKeySignature(currentX, spacing, song.keySignature, song.clef);
        currentX += keySignatureWidth;

        // 3. 박자표
        float timeSignatureWidth = scoreSymbolSpawner.SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;

        // 4. 잇단음표 파싱 및 배치
        List<object> parsedElements = TupletParser.ParseWithTuplets(song.notes);
        TupletParser.DebugPrintParseResult(parsedElements);

        // 5. 요소별 배치
        float remainingWidth = usableWidth - (currentX - startX);
        int totalElements = CountLayoutElements(parsedElements);
        float elementSpacing = remainingWidth / totalElements;

        foreach (object element in parsedElements)
        {
            if (element is NoteData note)
            {
                if (note.isBarLine)
                {
                    // 마디선 생성
                    NoteLayoutHelper.CreateBarLine(currentX, staffPanel, staffLinePrefabForBarLine, spacing);
                    continue;
                }

                // 일반 음표 생성
                notePlacementHandler.SpawnNoteAtPosition(currentX, elementSpacing, spacing, note);
                currentX += elementSpacing;
            }
            else if (element is TupletData tuplet)
            {
                // 잇단음표 그룹 생성
                Debug.Log($"🎵 잇단음표 그룹 처리: {tuplet.GetTupletTypeName()}");
                
                TupletVisualGroup visualGroup = notePlacementHandler.SpawnTupletGroup(tuplet, currentX, elementSpacing * 2, spacing);
                
                if (visualGroup != null)
                {
                    currentX += tuplet.totalWidth;
                    Debug.Log($"✅ 잇단음표 생성 성공: 폭={tuplet.totalWidth:F1}");
                }
                else
                {
                    Debug.LogError("❌ 잇단음표 생성 실패");
                    currentX += elementSpacing;
                }
            }
        }

        Debug.Log($"✅ 잇단음표 테스트 완료: {song.title}");
    }

    private int CountLayoutElements(List<object> elements)
    {
        int count = 0;
        foreach (object element in elements)
        {
            if (element is NoteData note && !note.isBarLine)
                count++;
            else if (element is TupletData)
                count += 2; // 잇단음표는 2배 공간 할당
        }
        return Mathf.Max(count, 1);
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

    private bool ValidateComponents()
    {
        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 할당되지 않았습니다!");
            return false;
        }

        if (scoreSymbolSpawner == null)
        {
            Debug.LogError("❌ scoreSymbolSpawner가 할당되지 않았습니다!");
            return false;
        }

        if (notePlacementHandler == null)
        {
            Debug.LogError("❌ notePlacementHandler가 할당되지 않았습니다!");
            return false;
        }

        if (assembler == null)
        {
            Debug.LogError("❌ assembler가 할당되지 않았습니다!");
            return false;
        }

        // TupletAssembler 및 TupletLayoutHandler 확인
        if (notePlacementHandler.tupletAssembler == null)
        {
            Debug.LogError("❌ notePlacementHandler.tupletAssembler가 할당되지 않았습니다!");
            return false;
        }

        if (notePlacementHandler.tupletLayoutHandler == null)
        {
            Debug.LogError("❌ notePlacementHandler.tupletLayoutHandler가 할당되지 않았습니다!");
            return false;
        }

        // TupletAssembler 프리팹 확인
        if (!notePlacementHandler.tupletAssembler.ValidatePrefabs())
        {
            Debug.LogError("❌ TupletAssembler 프리팹이 올바르게 할당되지 않았습니다!");
            return false;
        }

        Debug.Log("✅ 모든 컴포넌트가 올바르게 할당되었습니다.");
        return true;
    }

    private void ClearStaff()
    {
        if (staffPanel != null)
        {
            for (int i = staffPanel.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(staffPanel.GetChild(i).gameObject);
            }
        }
    }

    // 테스트 케이스 변경
    [ContextMenu("다음 테스트 케이스")]
    public void NextTestCase()
    {
        testCaseIndex = (testCaseIndex + 1) % 3;
        TestTupletVisuals();
    }

    [ContextMenu("이전 테스트 케이스")]
    public void PreviousTestCase()
    {
        testCaseIndex = (testCaseIndex - 1 + 3) % 3;
        TestTupletVisuals();
    }

    // 개별 요소 테스트
    [ContextMenu("숫자 프리팹 테스트")]
    public void TestNumberPrefabs()
    {
        Debug.Log("🔢 === 숫자 프리팹 테스트 ===");
        
        if (notePlacementHandler.tupletAssembler == null)
        {
            Debug.LogError("❌ TupletAssembler가 할당되지 않았습니다!");
            return;
        }

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        Vector2 basePos = new Vector2(0, spacing * 3);

        for (int i = 0; i <= 9; i++)
        {
            Vector2 pos = basePos + new Vector2(spacing * i * 0.8f, 0);
            GameObject numberObj = notePlacementHandler.tupletAssembler.CreateTupletNumber(i, pos, spacing);
            
            if (numberObj != null)
            {
                Debug.Log($"✅ 숫자 {i} 생성 성공");
            }
            else
            {
                Debug.LogError($"❌ 숫자 {i} 생성 실패");
            }
        }
    }

    [ContextMenu("Beam 프리팹 테스트")]
    public void TestBeamPrefab()
    {
        Debug.Log("🌉 === Beam 프리팹 테스트 ===");
        
        if (notePlacementHandler.tupletAssembler == null)
        {
            Debug.LogError("❌ TupletAssembler가 할당되지 않았습니다!");
            return;
        }

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        Vector2 startPos = new Vector2(-spacing * 2, spacing * 1);
        Vector2 endPos = new Vector2(spacing * 2, spacing * 1.5f);
        float thickness = spacing * 0.15f;

        GameObject beamObj = notePlacementHandler.tupletAssembler.CreateBeamWithCode(startPos, endPos, thickness);
        
        if (beamObj != null)
        {
            Debug.Log("✅ Beam 생성 성공");
        }
        else
        {
            Debug.LogError("❌ Beam 생성 실패");
        }
    }
}