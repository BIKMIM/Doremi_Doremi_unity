using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// MusicLayoutConfig.cs - 오선 줄 간격, stem, flag 비율 등 전체 레이아웃 관리 파일

public static class MusicLayoutConfig
{
    public const int StaffSpacingDivisor = 10;   // 오선지 줄 간격 나눌 수
    public const float LineThicknessRatio = 0.2f;  // 줄 두께 비율
    public const float NoteHeadWidthRatio = 1.2f;  // 음표 머리 너비 비율
    public const float NoteHeadHeightRatio = 0.8f;  // 음표 머리 높이 비율

    public const float StemWidthRatio = 0.2f; // 스템 너비 비율
    public const float StemHeightRatio = 3f; // 스템 높이 비율

    public const float FlagOffsetRatio = 0.25f; // 플래그 오프셋 비율
    public const float FlagSizeXRatio = 0.9f; // 플래그 너비 비율
    public const float FlagSizeYRatio = 2.5f; // 플래그 높이 비율


    public static float GetSpacing(RectTransform staffPanel)    // 줄 간격 계산
    {
        return staffPanel.rect.height / StaffSpacingDivisor;  // 줄 간격 계산
    }

    public static float GetLineThickness(RectTransform staffPanel)  // 줄 두께 계산
    {
        float spacing = GetSpacing(staffPanel);  // 줄 간격 계산
        return Mathf.Max(spacing * LineThicknessRatio, 2f);  // 줄 두께 계산
    }
    
    public static float GetNoteHeadWidth(RectTransform staffPanel)  // 음표 머리 너비 계산
    {
        float spacing = GetSpacing(staffPanel);  // 줄 간격 계산
        return spacing * NoteHeadWidthRatio;  // 음표 머리 너비 계산
    }

    public static float GetNoteHeadHeight(RectTransform staffPanel)  // 음표 머리 높이 계산
    {
        float spacing = GetSpacing(staffPanel);  // 줄 간격 계산
        return spacing * NoteHeadHeightRatio;  // 음표 머리 높이 계산
    }

    // 🎯 오선 줄 간격에 따라 상대적인 박자 간격 계산
    public static float GetBeatSpacing(RectTransform staffPanel)
    {
        float spacing = GetSpacing(staffPanel); // 줄 간격
        return spacing * 2.5f; // 4분음표 기준 간격 (예: 줄 간격의 2.5배)
    }


    public static float GetBeatSpacingFor(RectTransform staffPanel, int duration, bool isDotted)
    {
        float beatUnit = GetBeatSpacing(staffPanel); // 오선 비율 기반으로 간격 계산
        float factor = 4f / duration; // 4분음표 = 1.0, 8분음표 = 0.5, 등등
        if (isDotted) factor *= 1.5f;

        return beatUnit * factor;
    }




}
