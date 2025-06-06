using UnityEngine;
using System.Collections.Generic;

// 잇단음표 시각적 테스트 스크립트 - 비율 기반 간격 지원
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
    public bool autoStart = false; // 자동 시작 비활성화

    [Header("🎯 비율 기반 간격 설정")]
    [Range(0.5f, 1.5f)]
    public float tupletCompressionRatio = 0.7f; // 잇단음표 압축 비율
    
    [Range(1.0f, 2.0f)]
    public float normalNoteExpansionRatio = 1.3f; // 일반 음표 확장 비율

    public StaffLineDrawer staffLineDrawer; // StaffLineDrawer 컴포넌트 참조 추가

    void Start()
    {
        if (autoStart)
        {
            TestTupletVisuals();
        }
    }

    [ContextMenu("🎼 비율 기반 잇단음표 테스트")]
    public void TestTupletVisuals()
    {
        if (!ValidateComponents())
        {
            Debug.LogError("❌ 필수 컴포넌트가 할당되지 않았습니다!");
            return;
        }

        Debug.Log("🎼 === 비율 기반 잇단음표 시각적 테스트 시작 ===");

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

        // TupletLayoutHandler에 압축 비율 적용
        if (notePlacementHandler.tupletLayoutHandler != null)
        {
            notePlacementHandler.tupletLayoutHandler.SetCompressionRatio(tupletCompressionRatio);
        }

        // 악보 레이아웃 (비율 기반)
        LayoutTestScoreWithProportions(testSong);
    }

    private List<JsonLoader.SongData> CreateTestCases()
    {
        return new List<JsonLoader.SongData>
        {
            // 테스트 케이스 0: 8분음표 4개 + 4분음표 1개 (3/4박자)
            new JsonLoader.SongData
            {
                title = "🎯 8분음표4개(2박자) + 4분음표1개(1박자) = 3박자",
                clef = "treble",
                timeSignature = "3/4",
                keySignature = "C",
                notes = new List<string>
                {
                    "TUPLET_START:4:2", // 4잇단음표, 2박자
                    "C4:8", "D4:8", "E4:8", "F4:8",
                    "TUPLET_END",
                    "C5:4", // 1박자
                    "|"
                }
            },

            // 테스트 케이스 1: 넷잇단음표 + 일반 음표
            new JsonLoader.SongData
            {
                title = "🎼 넷잇단음표 + 일반음표",
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

    // ✅ 비율 기반 테스트 악보 레이아웃
    private void LayoutTestScoreWithProportions(JsonLoader.SongData song)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float panelWidth = staffPanel.rect.width;
        float leftEdge = -panelWidth * 0.5f;
        float leftMargin = panelWidth * 0.02f;
        float usableWidth = panelWidth * 0.96f;

        float startX = leftEdge + leftMargin;
        float currentX = startX;

        Debug.Log($"🎯 비율 기반 테스트 레이아웃: {song.title}");
        Debug.Log($"   패널폭: {panelWidth:F1}, 사용가능폭: {usableWidth:F1}");

        // 1. 음자리표
        float clefWidth = scoreSymbolSpawner.SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;

        // 2. 조표
        float keySignatureWidth = scoreSymbolSpawner.SpawnKeySignature(currentX, spacing, song.keySignature, song.clef);
        currentX += keySignatureWidth;

        // 3. 박자표
        float timeSignatureWidth = scoreSymbolSpawner.SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;

        // 4. 잇단음표 파싱 및 비율 기반 배치
        List<object> parsedElements = TupletParser.ParseWithTuplets(song.notes);
        TupletParser.DebugPrintParseResult(parsedElements);

        // 5. 📊 총 박자 수 계산
        float totalBeats = CalculateTotalBeats(parsedElements);
        
        // 6. 🎯 마디 공간 계산
        float measureWidth = usableWidth - (currentX - startX);
        float measureMarginRatio = 0.1f; // 10% 여백
        float measureUsableWidth = measureWidth * (1f - measureMarginRatio * 2f);
        float measureLeftMargin = measureWidth * measureMarginRatio;

        currentX += measureLeftMargin; // 마디 여백 적용

        Debug.Log($"   총박자: {totalBeats:F2}, 마디폭: {measureWidth:F1}, 마디사용폭: {measureUsableWidth:F1}");

        // 7. 🎼 요소별 비율 기반 배치
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

                // 일반 음표의 박자 값 및 비율 계산
                float noteBeats = CalculateNoteBeatValue(note);
                float beatRatio = noteBeats / totalBeats;
                
                // ✅ 일반 음표는 확장 비율 적용
                float baseWidth = measureUsableWidth * beatRatio;
                float noteWidth = baseWidth * normalNoteExpansionRatio;

                notePlacementHandler.SpawnNoteAtPosition(currentX, noteWidth, spacing, note);
                
                Debug.Log($"   일반음표: {note.noteName}({note.duration}분음표) = {noteBeats:F2}박자({beatRatio:P0}), 폭={noteWidth:F1}");
                
                currentX += noteWidth;
            }
            else if (element is TupletData tuplet)
            {
                // 잇단음표의 박자 값 및 비율 계산
                float tupletBeats = tuplet.beatValue * 0.25f;
                float beatRatio = tupletBeats / totalBeats;
                
                // ✅ 잇단음표는 압축 비율 적용
                float baseWidth = measureUsableWidth * beatRatio;
                float tupletWidth = baseWidth * tupletCompressionRatio;

                Debug.Log($"   잣단음표: {tuplet.GetTupletTypeName()} = {tupletBeats:F2}박자({beatRatio:P0})");
                Debug.Log($"   기본폭: {baseWidth:F1}, 압축폭: {tupletWidth:F1} (압축비율: {tupletCompressionRatio:F1})");
                
                TupletVisualGroup visualGroup = notePlacementHandler.SpawnTupletGroup(tuplet, currentX, tupletWidth, spacing);
                
                if (visualGroup != null)
                {
                    currentX += tupletWidth;
                    Debug.Log($"   ✅ 잇단음표 생성 성공");
                }
                else
                {
                    Debug.LogError("   ❌ 잇단음표 생성 실패");
                    currentX += tupletWidth;
                }
            }
        }

        Debug.Log($"✅ 비율 기반 잇단음표 테스트 완료: {song.title}");
        Debug.Log($"   🎯 압축비율={tupletCompressionRatio:F1}, 확장비율={normalNoteExpansionRatio:F1}");
    }

    // 🎯 총 박자 수 계산
    private float CalculateTotalBeats(List<object> elements)
    {
        float totalBeats = 0f;

        foreach (object element in elements)
        {
            if (element is NoteData note && !note.isBarLine)
            {
                totalBeats += CalculateNoteBeatValue(note);
            }
            else if (element is TupletData tuplet)
            {
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
                GameObject child = staffPanel.GetChild(i).gameObject;
                // "StaffLine" 태그를 가진 오브젝트는 파괴하지 않음
                if (child.CompareTag("StaffLine") == false)
                {
                    DestroyImmediate(child);
                }
            }
        }
    }

    // ✅ 비율 기반 테스트 케이스 변경
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

    // ✅ 비율 조정 메서드들
    [ContextMenu("🎼 잇단음표 압축 증가 (더 좁게)")]
    public void IncreaseTupletCompression()
    {
        tupletCompressionRatio = Mathf.Max(tupletCompressionRatio - 0.1f, 0.5f);
        Debug.Log($"🎼 잇단음표 압축비율: {tupletCompressionRatio:F1} (더 좁게)");
        
        // TupletLayoutHandler에도 적용
        if (notePlacementHandler?.tupletLayoutHandler != null)
        {
            notePlacementHandler.tupletLayoutHandler.SetCompressionRatio(tupletCompressionRatio);
        }
        
        TestTupletVisuals(); // 즉시 반영
    }

    [ContextMenu("🎼 잇단음표 압축 감소 (더 넓게)")]
    public void DecreaseTupletCompression()
    {
        tupletCompressionRatio = Mathf.Min(tupletCompressionRatio + 0.1f, 1.5f);
        Debug.Log($"🎼 잇단음표 압축비율: {tupletCompressionRatio:F1} (더 넓게)");
        
        // TupletLayoutHandler에도 적용
        if (notePlacementHandler?.tupletLayoutHandler != null)
        {
            notePlacementHandler.tupletLayoutHandler.SetCompressionRatio(tupletCompressionRatio);
        }
        
        TestTupletVisuals(); // 즉시 반영
    }

    [ContextMenu("🎵 일반음표 확장 증가 (더 넓게)")]
    public void IncreaseNormalNoteExpansion()
    {
        normalNoteExpansionRatio = Mathf.Min(normalNoteExpansionRatio + 0.1f, 2.0f);
        Debug.Log($"🎵 일반음표 확장비율: {normalNoteExpansionRatio:F1} (더 넓게)");
        TestTupletVisuals(); // 즉시 반영
    }

    [ContextMenu("🎵 일반음표 확장 감소 (더 좁게)")]
    public void DecreaseNormalNoteExpansion()
    {
        normalNoteExpansionRatio = Mathf.Max(normalNoteExpansionRatio - 0.1f, 1.0f);
        Debug.Log($"🎵 일반음표 확장비율: {normalNoteExpansionRatio:F1} (더 좁게)");
        TestTupletVisuals(); // 즉시 반영
    }

    [ContextMenu("🔄 비율 설정 리셋")]
    public void ResetRatioSettings()
    {
        tupletCompressionRatio = 0.7f; // 70%
        normalNoteExpansionRatio = 1.3f; // 130%
        
        // TupletLayoutHandler에도 적용
        if (notePlacementHandler?.tupletLayoutHandler != null)
        {
            notePlacementHandler.tupletLayoutHandler.SetCompressionRatio(tupletCompressionRatio);
        }
        
        Debug.Log("🔄 비율 설정이 기본값으로 리셋되었습니다.");
        TestTupletVisuals(); // 즉시 반영
    }

    [ContextMenu("📊 현재 비율 설정 출력")]
    public void PrintRatioSettings()
    {
        Debug.Log($"📊 현재 비율 기반 간격 설정:");
        Debug.Log($"   잇단음표 압축비율: {tupletCompressionRatio:F1} (낮을수록 좁게)");
        Debug.Log($"   일반음표 확장비율: {normalNoteExpansionRatio:F1} (높을수록 넓게)");
        Debug.Log($"   테스트 케이스: {testCaseIndex}");
    }

    // 개별 요소 테스트 (비율 기반)
    [ContextMenu("🔢 숫자 프리팹 테스트")]
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

    [ContextMenu("🌉 Beam 프리팹 테스트")]
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
