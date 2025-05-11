using UnityEngine;

/// <summary>
/// 디버깅을 위해 음표 위치, 스페이싱 등을 출력하는 유틸리티 클래스
/// </summary>
public static class NoteDebugLogger
{
    public static void LogNote(string pitch, float index, float spacing, float baseY)
    {
        float y = baseY + index * spacing;
        Debug.Log($"🎵 Pitch: {pitch} | Index: {index} | Spacing: {spacing} → Y: {y}");
    }
}
