using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 하나의 음표 정보 저장 클래스

public class NoteData
{
    public string noteName;   // 예: "C4"
    public int duration;      // 예: 1, 2, 4, 8, 16
    public bool isDotted;     // 점음표 여부
    public bool isRest;       // 쉼표 여부
    public AccidentalType accidental; // 임시표 타입 (Sharp, Flat, Natural 등)
    public bool isBarLine;    // 마디구분선 여부

    // ✅ 잇단음표 관련 필드 추가
    public bool isTupletMember;    // 잇단음표의 구성원인지 여부
    public int tupletGroupId;      // 잇단음표 그룹 ID (같은 그룹은 같은 ID)
    public int tupletPosition;     // 그룹 내에서의 위치 (0, 1, 2...)

    public override string ToString() // 음표 정보를 문자열로 변환하는 메서드
    {
        if (isBarLine) return "마디구분선 |";

        string tupletInfo = isTupletMember ? $" | 잇단음표: 그룹{tupletGroupId}-{tupletPosition}" : "";
        
        return $"{noteName} | {duration}분음표 | 점음표: {isDotted} | 쉼표: {isRest} | 임시표: {accidental}{tupletInfo}"; 
    }

    // ✅ 잇단음표 멤버로 설정하는 헬퍼 메서드
    public void SetAsTupletMember(int groupId, int position)
    {
        isTupletMember = true;
        tupletGroupId = groupId;
        tupletPosition = position;
        Debug.Log($"음표 {noteName}를 잇단음표 그룹 {groupId}의 {position}번째 멤버로 설정");
    }

    // ✅ 잇단음표 멤버 해제
    public void ClearTupletMembership()
    {
        isTupletMember = false;
        tupletGroupId = -1;
        tupletPosition = -1;
    }

    // ✅ 잇단음표 정보 확인
    public bool IsInSameTupletGroup(NoteData other)
    {
        return isTupletMember && other.isTupletMember && tupletGroupId == other.tupletGroupId;
    }
}