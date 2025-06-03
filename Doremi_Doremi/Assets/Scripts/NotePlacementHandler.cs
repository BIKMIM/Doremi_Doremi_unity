using UnityEngine;
using UnityEngine.UI; // Color.black 때문
using System.Collections.Generic;


// NotePlacementHandler.cs
// 음표와 쉼표를 배치하고 덧줄을 생성하는 스크립트

public class NotePlacementHandler : MonoBehaviour
{
    [Header("음표 조립 프리팹")]
    public NoteAssembler assembler;

    [Header("🎼 덧줄 프리팹")]
    public GameObject ledgerLinePrefab;

    public GameObject naturalPrefab; // 추가
    public GameObject doubleSharpPrefab; // 추가
    public GameObject doubleFlatPrefab; // 추가



    // 음표 배치 대상 패널 (Initialize에서 받음)
    private RectTransform staffPanel;

    // 초기화 메소드 (NoteSpawner에서 호출)
    public void Initialize(RectTransform panel)
    {
        staffPanel = panel;
    }

    public void SpawnNoteAtPosition(float x, float noteSpacing, float spacing, NoteData note)
    {
        if (!NotePositioningData.noteIndexTable.ContainsKey(note.noteName))
        {
            Debug.LogWarning($"🎵 알 수 없는 음표 이름: {note.noteName}");
            return;
        }

        float noteIndex = NotePositioningData.noteIndexTable[note.noteName];
        float y = noteIndex * spacing * 0.5f;

        Vector2 pos = new Vector2(x + noteSpacing * 0.5f, y);

        // 🎼 덧줄 생성 (NoteLayoutHelper의 static 함수 호출)
        SpawnLedgerLines(pos.x, note.noteName, spacing);

        bool isOnLine = NotePositioningData.lineNotes.Contains(note.noteName);

        Debug.Log($"🎵 음표 생성: {note.noteName} at X={pos.x:F1}, Y={pos.y:F1}");

        if (note.isDotted)
        {
            assembler.SpawnDottedNoteFull(pos, noteIndex, isOnLine, note.duration);
        }
        else
        {
            assembler.SpawnNoteFull(pos, noteIndex, note.duration);
        }
    }



    // 🎵 쉼표 생성 함수

    public void SpawnRestAtPosition(float x, float noteSpacing, float spacing, NoteData note)
    {
        float restY = spacing * 0.0f;
        Vector2 restPos = new Vector2(x + noteSpacing * 0.5f, restY);

        Debug.Log($"🎵 쉼표 생성: {note.duration}분 쉼표 at X={restPos.x:F1}");

        assembler.SpawnRestNote(restPos, note.duration, note.isDotted);
    }



    // 🎼 해상도 독립적 덧줄 생성 함수 (NoteLayoutHelper를 사용하도록 변경)
    public void SpawnLedgerLines(float notePosX, string noteName, float staffSpacing)
    {
        if (!NotePositioningData.noteIndexTable.ContainsKey(noteName))
        {
            Debug.LogWarning($"⚠️ 알 수 없는 음표: {noteName}");
            return;
        }

        if (ledgerLinePrefab == null)
        { // 이 경고가 콘솔에 뜨는지 확인
            Debug.LogWarning("⚠️ 덧줄 프리팹이 설정되지 않았습니다.");
            return;
        }

        float noteIndex = NotePositioningData.noteIndexTable[noteName];

        if (!NoteLayoutHelper.NeedsLedgerLines(noteIndex))
        {
            Debug.Log($"🎼 {noteName}: 오선 내부 음표, 덧줄 불필요");
            return;
        }

        List<float> ledgerPositions = NoteLayoutHelper.GetLedgerPositions(noteIndex);

        Debug.Log($"🎼 {noteName}에 대해 {ledgerPositions.Count}개 덧줄 생성: [{string.Join(", ", ledgerPositions)}]");

        foreach (float ledgerIndex in ledgerPositions)
        {
            // 이 호출이 핵심입니다. staffPanel과 ledgerLinePrefab이 정확히 전달되는지 확인.
            NoteLayoutHelper.CreateSingleLedgerLine(notePosX, ledgerIndex, staffSpacing, staffPanel, ledgerLinePrefab);
        }
    }


    // 🎼 개별 덧줄 생성 함수 (해상도 독립적)
    private void CreateSingleLedgerLine(float x, float ledgerIndex, float staffSpacing)
    {
        GameObject ledgerLine = Instantiate(ledgerLinePrefab, staffPanel);
        RectTransform ledgerRT = ledgerLine.GetComponent<RectTransform>();

        float panelHeight = staffPanel.rect.height;
        float ledgerWidth = staffSpacing * 1.6f;
        float ledgerThickness = MusicLayoutConfig.GetLineThickness(staffPanel);

        ledgerRT.sizeDelta = new Vector2(ledgerWidth, ledgerThickness);
        ledgerRT.anchorMin = new Vector2(0.5f, 0.5f);
        ledgerRT.anchorMax = new Vector2(0.5f, 0.5f);
        ledgerRT.pivot = new Vector2(0.5f, 0.5f);

        float ledgerY = ledgerIndex * staffSpacing * 0.5f;
        ledgerRT.anchoredPosition = new Vector2(x, ledgerY);

        UnityEngine.UI.Image ledgerImage = ledgerLine.GetComponent<UnityEngine.UI.Image>();
        if (ledgerImage != null)
        {
            ledgerImage.color = Color.black;
        }

        Debug.Log($"   → 덧줄: 인덱스={ledgerIndex}, Y={ledgerY:F1}, 크기={ledgerWidth:F1}x{ledgerThickness:F1}");
    }
}