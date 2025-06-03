using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NotePlacementHandler : MonoBehaviour
{
    [Header("음표 조립 프리팹")]
    public NoteAssembler assembler;

    [Header("덧줄 프리팹")]
    public GameObject ledgerLinePrefab;

    [Header("임시표 프리팹")]
    public GameObject sharpPrefab;
    public GameObject flatPrefab;
    public GameObject naturalPrefab;
    public GameObject doubleSharpPrefab;
    public GameObject doubleFlatPrefab;

    private RectTransform staffPanel;

    public void Initialize(RectTransform panel)
    {
        staffPanel = panel;
    }

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

        // 음표 생성
        if (note.isDotted)
        {
            assembler.SpawnDottedNoteFull(pos, noteIndex, isOnLine, note.duration);
        }
        else
        {
            assembler.SpawnNoteFull(pos, noteIndex, note.duration);
        }
    }

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

        assembler.SpawnRestNote(restPos, note.duration, note.isDotted);
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

    // 임시표 크기 조정 함수들
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