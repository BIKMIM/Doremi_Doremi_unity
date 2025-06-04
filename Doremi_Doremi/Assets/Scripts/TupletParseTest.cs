using UnityEngine;
using System.Collections.Generic;

// 잇단음표 파싱 테스트 스크립트
public class TupletParseTest : MonoBehaviour
{
    void Start()
    {
        TestTupletParsing();
    }

    void TestTupletParsing()
    {
        Debug.Log("🎼 === 잇단음표 파싱 테스트 시작 ===");

        // 테스트 케이스들
        List<List<string>> testCases = new List<List<string>>
        {
            // 테스트 1: 기본 셋잇단음표
            new List<string>
            {
                "C4:4",
                "TUPLET_START:3:2",
                "D4:8", "E4:8", "F4:8",
                "TUPLET_END",
                "G4:4",
                "|"
            },

            // 테스트 2: 넷잇단음표
            new List<string>
            {
                "TUPLET_START:4:3",
                "C4:16", "D4:16", "E4:16", "F4:16",
                "TUPLET_END",
                "|"
            },

            // 테스트 3: 임시표 포함 셋잇단음표
            new List<string>
            {
                "TRIPLET_START:3",
                "C4#:8", "D4b:8", "E4n:8",
                "TRIPLET_END",
                "|"
            },

            // 테스트 4: 점음표 포함 잇단음표
            new List<string>
            {
                "TUPLET_START:3:2",
                "F4:8.", "G4:16", "A4:8",
                "TUPLET_END",
                "|"
            },

            // 테스트 5: 쉼표 포함 잇단음표
            new List<string>
            {
                "TUPLET_START:5:4",
                "B4:16", "REST:16", "C5:16", "D5:16", "E5:16",
                "TUPLET_END",
                "|"
            },

            // 테스트 6: 복합 마디 (일반음표 + 잇단음표 + 일반음표)
            new List<string>
            {
                "C4:4",
                "TUPLET_START:3:2", 
                "D4:8", "E4:8", "F4:8",
                "TUPLET_END",
                "G4:2",
                "|"
            }
        };

        string[] testNames = {
            "기본 셋잇단음표",
            "넷잇단음표", 
            "임시표 포함 셋잇단음표",
            "점음표 포함 잇단음표",
            "쉼표 포함 잇단음표",
            "복합 마디"
        };

        // 각 테스트 케이스 실행
        for (int i = 0; i < testCases.Count; i++)
        {
            Debug.Log($"\n🎯 테스트 {i + 1}: {testNames[i]}");
            Debug.Log($"입력: [{string.Join(", ", testCases[i])}]");
            
            try
            {
                List<object> result = TupletParser.ParseWithTuplets(testCases[i]);
                
                Debug.Log($"✅ 파싱 성공: {result.Count}개 요소");
                TupletParser.DebugPrintParseResult(result);
                
                // 결과 검증
                ValidateParseResult(result, i + 1);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 테스트 {i + 1} 파싱 실패: {e.Message}");
            }

            Debug.Log($"--- 테스트 {i + 1} 완료 ---\n");
        }

        // 오류 케이스 테스트
        TestErrorCases();
        
        Debug.Log("🎼 === 잇단음표 파싱 테스트 완료 ===");
    }

    void ValidateParseResult(List<object> result, int testNumber)
    {
        Debug.Log($"🔍 테스트 {testNumber} 결과 검증:");
        
        int noteCount = 0;
        int tupletCount = 0;
        
        foreach (var item in result)
        {
            if (item is NoteData note)
            {
                noteCount++;
                if (note.isTupletMember)
                {
                    Debug.LogWarning($"⚠️ 일반 음표가 잇단음표 멤버로 표시됨: {note.noteName}");
                }
            }
            else if (item is TupletData tuplet)
            {
                tupletCount++;
                
                if (!TupletParser.ValidateTupletData(tuplet))
                {
                    Debug.LogError($"❌ 잘못된 잇단음표 데이터");
                }
                
                // 멤버 음표들 검증
                for (int i = 0; i < tuplet.notes.Count; i++)
                {
                    var member = tuplet.notes[i];
                    if (!member.isTupletMember)
                    {
                        Debug.LogError($"❌ 잇단음표 멤버가 제대로 설정되지 않음: {member.noteName}");
                    }
                    if (member.tupletPosition != i)
                    {
                        Debug.LogError($"❌ 잘못된 잇단음표 위치: {member.noteName} 예상={i}, 실제={member.tupletPosition}");
                    }
                }
            }
        }
        
        Debug.Log($"📊 검증 완료: 일반음표 {noteCount}개, 잇단음표그룹 {tupletCount}개");
    }

    void TestErrorCases()
    {
        Debug.Log("\n🚫 === 오류 케이스 테스트 ===");
        
        List<List<string>> errorCases = new List<List<string>>
        {
            // 오류 1: TUPLET_END가 없는 경우
            new List<string> { "TUPLET_START:3:2", "C4:8", "D4:8", "E4:8" },
            
            // 오류 2: TUPLET_START가 없는 경우  
            new List<string> { "C4:8", "D4:8", "E4:8", "TUPLET_END" },
            
            // 오류 3: 잘못된 매개변수
            new List<string> { "TUPLET_START:abc:def", "C4:8", "TUPLET_END" },
            
            // 오류 4: 중첩된 잇단음표
            new List<string> { "TUPLET_START:3:2", "TUPLET_START:2:1", "C4:8", "TUPLET_END", "TUPLET_END" }
        };

        string[] errorNames = {
            "TUPLET_END 누락",
            "TUPLET_START 누락", 
            "잘못된 매개변수",
            "중첩된 잇단음표"
        };

        for (int i = 0; i < errorCases.Count; i++)
        {
            Debug.Log($"\n🚫 오류 테스트 {i + 1}: {errorNames[i]}");
            Debug.Log($"입력: [{string.Join(", ", errorCases[i])}]");
            
            try
            {
                List<object> result = TupletParser.ParseWithTuplets(errorCases[i]);
                Debug.Log($"⚠️ 예상치 못한 성공: {result.Count}개 요소");
            }
            catch (System.Exception e)
            {
                Debug.Log($"✅ 예상된 오류 처리: {e.Message}");
            }
        }
        
        Debug.Log("🚫 === 오류 케이스 테스트 완료 ===");
    }

    // 지원되는 형식 도움말 출력
    [ContextMenu("잇단음표 형식 도움말")]
    public void ShowSupportedFormats()
    {
        Debug.Log("📖 === 지원되는 잇단음표 형식 ===");
        
        string[] formats = TupletParser.GetSupportedFormats();
        for (int i = 0; i < formats.Length; i++)
        {
            Debug.Log($"{i + 1}. {formats[i]}");
        }
        
        Debug.Log("\n📖 === 사용 예시 ===");
        Debug.Log("셋잇단음표: TUPLET_START:3:2 → C4:8, D4:8, E4:8 → TUPLET_END");
        Debug.Log("넷잇단음표: TUPLET_START:4:3 → F4:16, G4:16, A4:16, B4:16 → TUPLET_END");
        Debug.Log("다섯잇단음표: TUPLET_START:5:4 → C5:16, D5:16, E5:16, F5:16, G5:16 → TUPLET_END");
    }

    // 특정 잇단음표 테스트
    [ContextMenu("셋잇단음표만 테스트")]
    public void TestTripletsOnly()
    {
        Debug.Log("🎯 === 셋잇단음표 전용 테스트 ===");
        
        List<string> tripletTest = new List<string>
        {
            "C4:4",
            "TRIPLET_START:3",
            "D4:8", "E4:8", "F4:8",
            "TRIPLET_END", 
            "G4:4",
            "|"
        };
        
        List<object> result = TupletParser.ParseWithTuplets(tripletTest);
        TupletParser.DebugPrintParseResult(result);
        
        // 셋잇단음표 찾기
        foreach (var item in result)
        {
            if (item is TupletData tuplet)
            {
                Debug.Log($"🎵 발견된 셋잇단음표: {tuplet.GetTupletTypeName()}");
                Debug.Log($"   시간 비율: {tuplet.GetTimeRatio():F2}");
                Debug.Log($"   음표들: {string.Join(", ", tuplet.notes.ConvertAll(n => n.noteName))}");
            }
        }
    }
}