using System.Collections.Generic;
using UnityEngine;

// 잇단음표 그룹 데이터를 저장하는 클래스
[System.Serializable]
public class TupletData
{
    [Header("잇단음표 기본 정보")]
    public int noteCount;           // 음표 개수 (3, 4, 5, 6, 7...)
    public int beatValue;           // 차지하는 박자 수 (보통 noteCount - 1)
    public List<NoteData> notes;    // 그룹 내 음표들

    [Header("레이아웃 정보")]
    public float startX;            // 시작 X 위치
    public float totalWidth;        // 전체 폭
    public float noteSpacing;       // 내부 음표 간격
    public float centerX;           // 중앙 X 위치 (숫자 배치용)

    [Header("시각적 특성")]
    public bool hasAccidentals;     // 임시표 포함 여부
    public bool hasDottedNotes;     // 점음표 포함 여부
    public bool hasRests;           // 쉼표 포함 여부
    public float maxNoteY;          // 가장 높은 음표의 Y 위치 (숫자 위치 계산용)
    public float minNoteY;          // 가장 낮은 음표의 Y 위치

    // 생성자
    public TupletData()
    {
        notes = new List<NoteData>();
        noteCount = 3;  // 기본값: 셋잇단음표
        beatValue = 2;  // 기본값: 2박자
        hasAccidentals = false;
        hasDottedNotes = false;
        hasRests = false;
    }

    public TupletData(int noteCount, int beatValue)
    {
        this.noteCount = noteCount;
        this.beatValue = beatValue;
        notes = new List<NoteData>();
        hasAccidentals = false;
        hasDottedNotes = false;
        hasRests = false;
    }

    // 음표 추가 메서드
    public void AddNote(NoteData note)
    {
        if (notes == null)
            notes = new List<NoteData>();

        notes.Add(note);
        
        // 특성 업데이트
        if (note.accidental != AccidentalType.None)
            hasAccidentals = true;
        
        if (note.isDotted)
            hasDottedNotes = true;
            
        if (note.isRest)
            hasRests = true;

        Debug.Log($"잇단음표 그룹에 음표 추가: {note.noteName}, 현재 {notes.Count}/{noteCount}개");
    }

    // 그룹이 완성되었는지 확인
    public bool IsComplete()
    {
        return notes != null && notes.Count == noteCount;
    }

    // 잇단음표 타입 이름 반환
    public string GetTupletTypeName()
    {
        return noteCount switch
        {
            3 => "셋잇단음표",
            4 => "넷잇단음표", 
            5 => "다섯잇단음표",
            6 => "여섯잇단음표",
            7 => "일곱잇단음표",
            _ => $"{noteCount}잇단음표"
        };
    }

    // 잇단음표의 실제 박자 비율 계산
    public float GetTimeRatio()
    {
        return (float)beatValue / noteCount;
    }

    // 디버그 정보 출력
    public override string ToString()
    {
        string noteList = notes != null ? string.Join(", ", notes.ConvertAll(n => n.noteName)) : "없음";
        return $"{GetTupletTypeName()}({noteCount}:{beatValue}) | 음표: [{noteList}] | " +
               $"임시표:{hasAccidentals} | 점음표:{hasDottedNotes} | 쉼표:{hasRests} | 완성:{IsComplete()}";
    }

    // 레이아웃 정보 계산 (나중에 TupletLayoutHandler에서 호출)
    public void CalculateLayout(float spacing, float availableWidth)
    {
        if (!IsComplete())
        {
            Debug.LogWarning("잇단음표 그룹이 완성되지 않았습니다.");
            return;
        }

        // 기본 폭 계산 (임시 구현)
        float baseWidth = availableWidth * GetTimeRatio();
        totalWidth = baseWidth;
        noteSpacing = totalWidth / noteCount;
        centerX = startX + totalWidth * 0.5f;

        Debug.Log($"잇단음표 레이아웃 계산: 폭={totalWidth:F1}, 간격={noteSpacing:F1}, 중앙={centerX:F1}");
    }

    // 음표들의 Y 위치 범위 계산 - staffPanel 파라미터 추가
    public void CalculateVerticalRange(RectTransform staffPanel)
    {
        if (notes == null || notes.Count == 0)
        {
            maxNoteY = 0f;
            minNoteY = 0f;
            return;
        }

        // staffPanel이 null인 경우 기본 spacing 사용
        float spacing = 0f;
        if (staffPanel != null)
        {
            spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        }
        else
        {
            Debug.LogWarning("staffPanel이 null입니다. 기본 spacing(20)을 사용합니다.");
            spacing = 20f; // 기본값
        }

        maxNoteY = float.MinValue;
        minNoteY = float.MaxValue;

        foreach (var note in notes)
        {
            if (!note.isRest && NotePositioningData.noteIndexTable.ContainsKey(note.noteName))
            {
                float noteIndex = NotePositioningData.noteIndexTable[note.noteName];
                float noteY = noteIndex * spacing * 0.5f;
                
                if (noteY > maxNoteY) maxNoteY = noteY;
                if (noteY < minNoteY) minNoteY = noteY;
            }
        }

        // 쉼표만 있는 경우 기본값 설정
        if (maxNoteY == float.MinValue)
        {
            maxNoteY = 0f;
            minNoteY = 0f;
        }

        Debug.Log($"잇단음표 Y 범위: {minNoteY:F1} ~ {maxNoteY:F1}");
    }
}