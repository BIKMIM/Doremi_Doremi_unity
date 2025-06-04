using UnityEngine;

// 점음표 시스템 테스트 스크립트
public class DottedNoteTest : MonoBehaviour
{
    void Start()
    {
        TestDottedNoteParser();
    }

    void TestDottedNoteParser()
    {
        Debug.Log("=== 점음표 파싱 테스트 ===");
        
        // 기본 점음표 테스트
        string[] testNotes = {
            "C4:4.",     // C4 점4분음표
            "D4#:8.",    // D4# 점8분음표
            "E4b:2.",    // E4♭ 점2분음표
            "F4##:16.",  // F4## 점16분음표
            "REST:4.",   // 점4분쉼표
            "REST:8.",   // 점8분쉼표
            "G4:1.",     // G4 점1분음표
            "A4bb:4.",   // A4♭♭ 점4분음표
            "B4n:4."     // B4♮ 점4분음표
        };

        foreach (string noteString in testNotes)
        {
            NoteData noteData = NoteParser.Parse(noteString);
            Debug.Log($"입력: {noteString}");
            Debug.Log($"  → 음표명: {noteData.noteName}");
            Debug.Log($"  → 길이: {noteData.duration}분음표");
            Debug.Log($"  → 점음표: {noteData.isDotted}");
            Debug.Log($"  → 쉼표: {noteData.isRest}");
            Debug.Log($"  → 임시표: {noteData.accidental}");
            Debug.Log($"  → 전체정보: {noteData}");
            Debug.Log("---");
        }

        Debug.Log("=== 점음표 파싱 테스트 완료 ===");
    }
}