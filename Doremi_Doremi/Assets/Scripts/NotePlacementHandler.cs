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

        // 🎼 덧줄 먼저 생성 (음표 아래 레이어에 표시되도록)
        SpawnLedgerLines(pos, note.noteName, spacing);

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

    public void SpawnRestAtPosition(float x, float noteSpacing, float spacing, NoteData note)
    {
        float restY = spacing * 0.0f;
        Vector2 restPos = new Vector2(x + noteSpacing * 0.5f, restY);

        Debug.Log($"🎵 쉼표 생성: {note.duration}분 쉼표 at X={restPos.x:F1}");

        assembler.SpawnRestNote(restPos, note.duration, note.isDotted);
    }

    // 🎼 해상도 독립적 덧줄 생성 함수
    public void SpawnLedgerLines(Vector2 notePosition, string noteName, float staffSpacing)
    {
        if (!NotePositioningData.noteIndexTable.ContainsKey(noteName))
        {
            Debug.LogWarning($"⚠️ 알 수 없는 음표: {noteName}");
            return;
        }

        if (ledgerLinePrefab == null)
        {
            Debug.LogWarning("⚠️ 덧줄 프리팹이 설정되지 않았습니다.");
            return;
        }

        float noteIndex = NotePositioningData.noteIndexTable[noteName];

        Debug.Log($"🎼 {noteName} 음표: 인덱스={noteIndex}, Y위치={notePosition.y:F1}");

        // 🎯 덧줄이 필요한 음표인지 확인 (오선 범위: E4(-4) ~ F5(4), 즉 -4 ~ 4 사이는 덧줄 불필요)
        if (noteIndex >= -4f && noteIndex <= 4f)
        {
            Debug.Log($"🎼 {noteName}: 오선 내부 음표, 덧줄 불필요");
            return;
        }

        List<float> ledgerPositions = new List<float>();

        if (noteIndex < -4f) // 오선 아래
        {
            Debug.Log($"🎼 {noteName}: 오선 아래 음표");
            // 음표가 짝수 인덱스(덧줄 위)인지 홀수 인덱스(덧줄 사이)인지 확인
            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0);

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 해당 덧줄부터 위쪽 모든 덧줄
                Debug.Log($"🎼 {noteName}: 덧줄 위에 위치 (인덱스={noteIndex})");
                for (float ledgerPos = noteIndex; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 위쪽 덧줄만
                float upperLedger = Mathf.Ceil(noteIndex / 2f) * 2f;
                Debug.Log($"🎼 {noteName}: 덧줄 사이에 위치 (인덱스={noteIndex}), 위쪽 덧줄={upperLedger}");
                for (float ledgerPos = upperLedger; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
        }
        else if (noteIndex > 4f) // 오선 위
        {
            Debug.Log($"🎼 {noteName}: 오선 위 음표");
            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0);

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 6부터 해당 덧줄까지
                Debug.Log($"🎼 {noteName}: 덧줄 위에 위치 (인덱스={noteIndex})");
                for (float ledgerPos = 6f; ledgerPos <= noteIndex; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 아래쪽 덧줄부터
                float lowerLedger = Mathf.Floor(noteIndex / 2f) * 2f;
                Debug.Log($"🎼 {noteName}: 덧줄 사이에 위치 (인덱스={noteIndex}), 아래쪽 덧줄={lowerLedger}");
                for (float ledgerPos = 6f; ledgerPos <= lowerLedger; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
        }

        Debug.Log($"🎼 {noteName}에 대해 {ledgerPositions.Count}개 덧줄 생성: [{string.Join(", ", ledgerPositions)}]");

        foreach (float ledgerIndex in ledgerPositions)
        {
            CreateSingleLedgerLine(notePosition.x, ledgerIndex, staffSpacing);
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