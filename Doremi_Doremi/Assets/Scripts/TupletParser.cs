using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

// 잇단음표 파싱을 담당하는 정적 클래스
public static class TupletParser
{
    private static int nextGroupId = 0; // 잇단음표 그룹 ID 생성용

    // 메인 파싱 함수: 문자열 리스트를 받아서 NoteData와 TupletData 혼합 리스트 반환
    public static List<object> ParseWithTuplets(List<string> noteStrings)
    {
        List<object> result = new List<object>();
        nextGroupId = 0; // 그룹 ID 초기화
        
        int i = 0;
        while (i < noteStrings.Count)
        {
            string current = noteStrings[i];
            
            if (IsTupletStart(current))
            {
                // 잇단음표 시작 발견
                var (noteCount, beatValue) = ParseTupletParams(current);
                var (tupletData, endIndex) = ParseTupletGroup(noteStrings, i, noteCount, beatValue);
                
                if (tupletData != null)
                {
                    result.Add(tupletData);
                    i = endIndex + 1; // TUPLET_END 다음으로 이동
                    Debug.Log($"✅ 잇단음표 파싱 완료: {tupletData}");
                }
                else
                {
                    Debug.LogError($"❌ 잇단음표 파싱 실패: {current}");
                    i++;
                }
            }
            else if (IsTupletEnd(current))
            {
                // TUPLET_END가 단독으로 나타남 (오류)
                Debug.LogWarning($"⚠️ 대응되는 TUPLET_START 없이 TUPLET_END 발견: {i}번째");
                i++;
            }
            else
            {
                // 일반 음표 파싱
                NoteData note = NoteParser.Parse(current);
                result.Add(note);
                i++;
            }
        }
        
        Debug.Log($"🎼 잇단음표 파싱 완료: 총 {result.Count}개 요소 (일반음표 + 잇단음표그룹)");
        return result;
    }

    // 잇단음표 시작 태그인지 확인
    public static bool IsTupletStart(string noteString)
    {
        if (string.IsNullOrEmpty(noteString)) return false;
        return noteString.StartsWith("TUPLET_START:") || noteString.StartsWith("TRIPLET_START:");
    }

    // 잇단음표 끝 태그인지 확인
    public static bool IsTupletEnd(string noteString)
    {
        if (string.IsNullOrEmpty(noteString)) return false;
        return noteString == "TUPLET_END" || noteString == "TRIPLET_END";
    }

    // 잇단음표 시작 태그에서 매개변수 추출
    public static (int noteCount, int beatValue) ParseTupletParams(string startTag)
    {
        try
        {
            // TUPLET_START:3:2 형식 파싱
            if (startTag.StartsWith("TUPLET_START:"))
            {
                string paramPart = startTag.Substring("TUPLET_START:".Length);
                string[] parts = paramPart.Split(':');
                
                if (parts.Length >= 1)
                {
                    int noteCount = int.Parse(parts[0]);
                    int beatValue = parts.Length >= 2 ? int.Parse(parts[1]) : (noteCount - 1); // 기본값
                    
                    Debug.Log($"잇단음표 매개변수 파싱: {noteCount}개 음표, {beatValue}박자");
                    return (noteCount, beatValue);
                }
            }
            // TRIPLET_START:3 형식 (셋잇단음표 간편 표기)
            else if (startTag.StartsWith("TRIPLET_START:"))
            {
                string paramPart = startTag.Substring("TRIPLET_START:".Length);
                int noteCount = int.Parse(paramPart);
                int beatValue = 2; // 셋잇단음표는 항상 2박자
                
                Debug.Log($"셋잇단음표 매개변수 파싱: {noteCount}개 음표, {beatValue}박자");
                return (noteCount, beatValue);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"잇단음표 매개변수 파싱 오류: {startTag} - {e.Message}");
        }

        // 기본값 반환 (셋잇단음표)
        Debug.LogWarning($"잉단음표 매개변수 파싱 실패, 기본값 사용: 3:2");
        return (3, 2);
    }

    // 잇단음표 그룹 파싱 (시작 인덱스부터 TUPLET_END까지)
    public static (TupletData tupletData, int endIndex) ParseTupletGroup(List<string> noteStrings, int startIndex, int expectedNoteCount, int beatValue)
    {
        TupletData tupletData = new TupletData(expectedNoteCount, beatValue);
        int currentGroupId = nextGroupId++;
        int notePosition = 0;
        
        Debug.Log($"🎵 잇단음표 그룹 파싱 시작: {expectedNoteCount}:{beatValue}, 그룹ID: {currentGroupId}");

        // TUPLET_START 다음부터 시작
        int i = startIndex + 1;
        
        while (i < noteStrings.Count)
        {
            string current = noteStrings[i];
            
            if (IsTupletEnd(current))
            {
                // 잇단음표 끝 발견
                if (tupletData.IsComplete())
                {
                    Debug.Log($"✅ 잇단음표 그룹 완성: {tupletData}");
                    return (tupletData, i);
                }
                else
                {
                    Debug.LogWarning($"⚠️ 잇단음표 그룹 미완성: {tupletData.notes.Count}/{expectedNoteCount}개");
                    return (tupletData, i); // 미완성이라도 반환
                }
            }
            else if (IsTupletStart(current))
            {
                // 중첩된 잇단음표는 지원하지 않음
                Debug.LogError($"❌ 중첩된 잇단음표는 지원되지 않습니다: {current}");
                return (null, i);
            }
            else
            {
                // 일반 음표 파싱 후 잇단음표 멤버로 설정
                NoteData note = NoteParser.Parse(current);
                note.SetAsTupletMember(currentGroupId, notePosition);
                tupletData.AddNote(note);
                notePosition++;
                
                // 예상 개수를 초과한 경우
                if (tupletData.notes.Count > expectedNoteCount)
                {
                    Debug.LogWarning($"⚠️ 잇단음표 예상 개수 초과: {tupletData.notes.Count}/{expectedNoteCount}");
                }
            }
            
            i++;
        }
        
        // TUPLET_END를 찾지 못한 경우
        Debug.LogError($"❌ TUPLET_END를 찾을 수 없습니다. 시작: {startIndex}");
        return (null, noteStrings.Count - 1);
    }

    // 잇단음표 유효성 검사
    public static bool ValidateTupletData(TupletData tupletData)
    {
        if (tupletData == null)
        {
            Debug.LogError("TupletData가 null입니다.");
            return false;
        }

        if (!tupletData.IsComplete())
        {
            Debug.LogWarning($"잇단음표 그룹이 미완성입니다: {tupletData.notes.Count}/{tupletData.noteCount}");
            return false;
        }

        if (tupletData.noteCount < 2 || tupletData.noteCount > 9)
        {
            Debug.LogError($"지원되지 않는 잇단음표 개수: {tupletData.noteCount} (2~9개만 지원)");
            return false;
        }

        Debug.Log($"✅ 잇단음표 유효성 검사 통과: {tupletData}");
        return true;
    }

    // 디버그용: 파싱 결과 출력
    public static void DebugPrintParseResult(List<object> parseResult)
    {
        Debug.Log("=== 잇단음표 파싱 결과 ===");
        
        for (int i = 0; i < parseResult.Count; i++)
        {
            var item = parseResult[i];
            
            if (item is NoteData note)
            {
                Debug.Log($"[{i}] 일반음표: {note}");
            }
            else if (item is TupletData tuplet)
            {
                Debug.Log($"[{i}] {tuplet}");
                for (int j = 0; j < tuplet.notes.Count; j++)
                {
                    Debug.Log($"    [{j}] {tuplet.notes[j]}");
                }
            }
            else
            {
                Debug.Log($"[{i}] 알 수 없는 타입: {item?.GetType().Name}");
            }
        }
        
        Debug.Log("=== 파싱 결과 출력 완료 ===");
    }

    // 지원되는 잇단음표 형식 반환 (도움말용)
    public static string[] GetSupportedFormats()
    {
        return new string[]
        {
            "TUPLET_START:3:2",     // 3잇단음표 (3개가 2박자)
            "TUPLET_START:4:3",     // 4잇단음표 (4개가 3박자)  
            "TUPLET_START:5:4",     // 5잇단음표 (5개가 4박자)
            "TRIPLET_START:3",      // 셋잇단음표 간편 표기
            "TUPLET_END",           // 잇단음표 끝
            "TRIPLET_END"           // 셋잇단음표 끝 (호환성)
        };
    }
}