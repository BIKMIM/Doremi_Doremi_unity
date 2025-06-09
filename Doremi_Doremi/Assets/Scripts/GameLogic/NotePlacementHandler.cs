using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// System.Diagnostics 제거하여 Debug 충돌 해결

public class NotePlacementHandler : MonoBehaviour
{
    [Header("음표 조립 시스템")]
    public ModularNoteAssembler assembler; // NoteAssembler → ModularNoteAssembler 변경

    [Header("덧줄 프리팹")]
    public GameObject ledgerLinePrefab;

    [Header("임시표 프리팹")]
    public GameObject sharpPrefab;
    public GameObject flatPrefab;
    public GameObject naturalPrefab;
    public GameObject doubleSharpPrefab;
    public GameObject doubleFlatPrefab;

    [Header("✨ 잇단음표 시스템")]
    public TupletLayoutHandler tupletLayoutHandler;
    public TupletAssembler tupletAssembler;

    private RectTransform staffPanel;

    public List<GameObject> spawnedNoteHeadsInOrder = new List<GameObject>(); // public으로 노출하거나 getter 제공


    public void Initialize(RectTransform panel)
    {
        staffPanel = panel;
        
        // 잇단음표 시스템 초기화
        if (tupletLayoutHandler != null)
            tupletLayoutHandler.Initialize(panel);
    }

    // ✅ 일반 음표 처리 (ModularNoteAssembler 사용)
    public void SpawnNoteAtPosition(float x, float noteSpacing, float spacing, NoteData note)
    {
        // ✅ 마디구분선 처리 - 건너뛰기
        if (note.isBarLine)
        {
            Debug.Log("마디구분선 건너뛰기");
            return;
        }

        // ✅ 쉼표 처리 개선
        if (note.isRest)
        {
            SpawnRestAtPosition(x, noteSpacing, spacing, note);
            return;
        }

        // 일반 음표 처리
        if (!NotePositioningData.noteIndexTable.ContainsKey(note.noteName))
        {
            Debug.LogWarning($"알 수 없는 음표 이름: {note.noteName}");
            return;
        }

        float noteIndex = NotePositioningData.noteIndexTable[note.noteName];
        float y = noteIndex * spacing * 0.5f;

        Vector2 pos = new Vector2(x + noteSpacing * 0.5f, y);

        // 임시표 생성 (음표보다 먼저)
        if (note.accidental != AccidentalType.None)
        {
            SpawnAccidental(pos, note.accidental, spacing);
        }

        // 덧줄 생성
        SpawnLedgerLines(pos.x, note.noteName, spacing);

        bool isOnLine = NotePositioningData.lineNotes.Contains(note.noteName);

        Debug.Log($"음표 생성: {note.noteName} at X={pos.x:F1}, Y={pos.y:F1}, 임시표:{note.accidental}");

        // ModularNoteAssembler 사용
        GameObject noteHeadResult; // 변수 선언 추가
        if (note.isDotted)
        {
            noteHeadResult = assembler.CreateDottedNote(pos, noteIndex, note.duration, isOnLine);
        }
        else
        {
            noteHeadResult = assembler.CreateNote(pos, noteIndex, note.duration);
        }
        // 이 부분을 추가/수정하세요.
        if (noteHeadResult != null) spawnedNoteHeadsInOrder.Add(noteHeadResult);
    }



    // NotePlacementHandler에 spawnedNoteHeadsInOrder 리스트 초기화 함수 추가 (NoteSpawner에서 호출 예정)
    public void ClearSpawnedNotes()
    {
        spawnedNoteHeadsInOrder.Clear();
    }


    // ✅ 새로운 비율 기반 잇단음표 그룹 처리 함수
    public TupletVisualGroup SpawnTupletGroup(TupletData tupletData, float startX, float availableWidth, float spacing)
    {
        if (!tupletData.IsComplete())
        {
            Debug.LogError("❌ 잇단음표 그룹이 완성되지 않았습니다.");
            return null;
        }

        if (tupletLayoutHandler == null || tupletAssembler == null)
        {
            Debug.LogError("❌ 잇단음표 시스템이 초기화되지 않았습니다!");
            return null;
        }

        Debug.Log($"🎼 비율 기반 잇단음표 그룹 생성: {tupletData.GetTupletTypeName()} at X={startX:F1}");

        // 1. ✅ 비율 기반 잇단음표 폭 계산
        float tupletWidth = tupletLayoutHandler.CalculateTupletWidth(tupletData, spacing, availableWidth, tupletData.noteCount);
        
        // 2. 레이아웃 설정 (해상도 독립적)
        tupletLayoutHandler.LayoutTupletNotes(tupletData, startX, tupletWidth, spacing);

        // 3. 개별 음표들 생성 (flag 없이)
        List<GameObject> noteHeads = new List<GameObject>();
        List<GameObject> stems = new List<GameObject>();
        
        // ✅ 개선된 음표 배치 (비율 기반)
        float marginRatio = 0.1f; // 10% 여백
        float usableWidth = tupletWidth * (1f - marginRatio * 2f);
        float leftMargin = tupletWidth * marginRatio;
        
        for (int i = 0; i < tupletData.notes.Count; i++)
        {
            NoteData note = tupletData.notes[i];
            
            // ✅ 비율 기반 음표 위치 계산
            float noteRatio = (float)i / Mathf.Max(tupletData.notes.Count - 1, 1); // 0~1 비율
            float noteX = startX + leftMargin + (usableWidth * noteRatio);
            
            // 개별 음표 생성 (잇단음표 전용 - flag 없이)
            var (noteHead, stem) = SpawnTupletNote(note, noteX, spacing);
            
            if (noteHead != null) noteHeads.Add(noteHead);
            if (stem != null) stems.Add(stem);
            
            Debug.Log($"   음표 {i}: {note.noteName} at X={noteX:F1} (비율: {noteRatio:F2})");
        }

        // 4. 잇단음표 시각적 요소 조립 (숫자 + beam)
        TupletVisualGroup visualGroup = tupletAssembler.AssembleTupletGroup(tupletData, noteHeads, stems, spacing);

        if (visualGroup != null)
        {
            Debug.Log($"✅ 비율 기반 잇단음표 그룹 완성: {tupletData.GetTupletTypeName()}, 폭={tupletWidth:F1}");
        }
        else
        {
            Debug.LogError($"❌ 잇단음표 시각적 요소 생성 실패");
        }

        return visualGroup;
    }

    // ✅ 잇단음표용 개별 음표 생성 (flag 없는 전용 메서드 사용)
    private (GameObject noteHead, GameObject stem) SpawnTupletNote(NoteData note, float x, float spacing)
    {
        // 쉼표 처리
        if (note.isRest)
        {
            Vector2 restPos = new Vector2(x, spacing * 0.0f);
            assembler.CreateRest(restPos, note.duration, note.isDotted);
            return (null, null); // 쉼표는 stem이 없음
        }

        // 일반 음표 처리
        if (!NotePositioningData.noteIndexTable.ContainsKey(note.noteName))
        {
            Debug.LogWarning($"알 수 없는 음표 이름: {note.noteName}");
            return (null, null);
        }

        float noteIndex = NotePositioningData.noteIndexTable[note.noteName];
        float y = noteIndex * spacing * 0.5f;
        Vector2 pos = new Vector2(x, y);

        // 임시표 생성
        if (note.accidental != AccidentalType.None)
        {
            SpawnAccidental(pos, note.accidental, spacing);
        }

        // 덧줄 생성
        SpawnLedgerLines(pos.x, note.noteName, spacing);

        // 잇단음표용 음표 생성 (flag 없이)
        GameObject noteHeadResult; // noteHead 변수명 변경 (중복 선언 방지)
        bool isOnLine = NotePositioningData.lineNotes.Contains(note.noteName);

        if (note.isDotted)
        {
            noteHeadResult = assembler.CreateTupletDottedNote(pos, noteIndex, note.duration, isOnLine);
        }
        else
        {
            noteHeadResult = assembler.CreateTupletNote(pos, noteIndex, note.duration);
        }

        // NotePlacementHandler의 spawnedNoteHeadsInOrder 리스트에 추가 (이전 답변에서 추가 요청된 부분)
        if (noteHeadResult != null) spawnedNoteHeadsInOrder.Add(noteHeadResult);

        // stem 찾기 (noteHead의 자식으로 생성됨)
        GameObject stem = null; // 여기서 stem을 선언
        if (noteHeadResult != null && note.duration >= 2) // noteHeadResult 사용
        {
            // ModularNoteAssembler에서 생성된 stem 찾기
            Transform stemTransform = noteHeadResult.transform.Find("stem(Clone)"); // noteHeadResult 사용
            if (stemTransform == null)
            {
                // 다른 가능한 이름들 시도
                stemTransform = noteHeadResult.transform.Find("Stem"); // noteHeadResult 사용
                if (stemTransform == null)
                {
                    // 자식 중에서 "stem"이 포함된 이름 찾기
                    for (int i = 0; i < noteHeadResult.transform.childCount; i++) // noteHeadResult 사용
                    {
                        Transform child = noteHeadResult.transform.GetChild(i);
                        if (child.name.ToLower().Contains("stem"))
                        {
                            stemTransform = child;
                            break;
                        }
                    }
                }
            }

            if (stemTransform != null)
            {
                stem = stemTransform.gameObject;
            }
        }

        Debug.Log($"🎵 잇단음표 개별 음표 생성: {note.noteName}, stem={stem != null}");

        return (noteHeadResult, stem); // noteHeadResult 반환
    }

    // ✅ 기존 함수들 (변경 없음)
    private float SpawnAccidental(Vector2 notePosition, AccidentalType accidental, float staffSpacing)
    {
        return AccidentalHelper.SpawnAccidental(
            notePosition, 
            accidental, 
            staffSpacing, 
            staffPanel, 
            sharpPrefab, 
            flatPrefab, 
            naturalPrefab, 
            doubleSharpPrefab, 
            doubleFlatPrefab, 
            null
        );
    }

    public void SpawnRestAtPosition(float x, float noteSpacing, float spacing, NoteData note)
    {
        float restY = spacing * 0.0f;
        Vector2 restPos = new Vector2(x + noteSpacing * 0.5f, restY);

        Debug.Log($"쉼표 생성: {note.duration}분 쉼표 at X={restPos.x:F1}");

        assembler.CreateRest(restPos, note.duration, note.isDotted);
    }

    public void SpawnLedgerLines(float notePosX, string noteName, float staffSpacing)
    {
        if (!NotePositioningData.noteIndexTable.ContainsKey(noteName))
        {
            Debug.LogWarning($"알 수 없는 음표: {noteName}");
            return;
        }

        if (ledgerLinePrefab == null)
        {
            Debug.LogWarning("덧줄 프리팹이 설정되지 않았습니다.");
            return;
        }

        float noteIndex = NotePositioningData.noteIndexTable[noteName];

        if (!NoteLayoutHelper.NeedsLedgerLines(noteIndex))
        {
            Debug.Log($"{noteName}: 오선 내부 음표, 덧줄 불필요");
            return;
        }

        List<float> ledgerPositions = NoteLayoutHelper.GetLedgerPositions(noteIndex);

        Debug.Log($"{noteName}에 대해 {ledgerPositions.Count}개 덧줄 생성");

        foreach (float ledgerIndex in ledgerPositions)
        {
            NoteLayoutHelper.CreateSingleLedgerLine(notePosX, ledgerIndex, staffSpacing, staffPanel, ledgerLinePrefab);
        }
    }

    // 임시표 크기 조정 함수들 (기존 유지)
    public void SetDoubleSharpSize(float widthRatio, float heightRatio)
    {
        var config = AccidentalHelper.GetDefaultConfig();
        config.doubleSharpWidthRatio = widthRatio;
        config.doubleSharpHeightRatio = heightRatio;
        AccidentalHelper.UpdateDefaultConfig(config);
        Debug.Log($"더블샵 크기 설정: {widthRatio} x {heightRatio}");
    }

    public void SetDoubleFlatSize(float widthRatio, float heightRatio, float yOffsetRatio = 0.1f)
    {
        var config = AccidentalHelper.GetDefaultConfig();
        config.doubleFlatWidthRatio = widthRatio;
        config.doubleFlatHeightRatio = heightRatio;
        config.doubleFlatYOffsetRatio = yOffsetRatio;
        AccidentalHelper.UpdateDefaultConfig(config);
        Debug.Log($"더블플랫 크기 설정: {widthRatio} x {heightRatio}, Y오프셋: {yOffsetRatio}");
    }

    public void SetNaturalSize(float widthRatio, float heightRatio)
    {
        var config = AccidentalHelper.GetDefaultConfig();
        config.naturalWidthRatio = widthRatio;
        config.naturalHeightRatio = heightRatio;
        AccidentalHelper.UpdateDefaultConfig(config);
        Debug.Log($"내츄럴 크기 설정: {widthRatio} x {heightRatio}");
    }

    public void SetAccidentalXOffset(float xOffsetRatio)
    {
        var config = AccidentalHelper.GetDefaultConfig();
        config.accidentalXOffsetRatio = xOffsetRatio;
        AccidentalHelper.UpdateDefaultConfig(config);
        Debug.Log($"임시표 X 오프셋 설정: {xOffsetRatio}");
    }
}
