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

    // MusicLayoutConfig.cs 에 추가할 만한 상수
    public const float TimeSignatureScaleRatio = 0.8f; // 예: 오선지 한 칸 높이의 80%를 각 숫자의 높이로 사용
    public const float TimeSignatureVerticalCoverage = 4.0f; // 박자표가 오선 4칸 높이를 커버하도록 (위 숫자 2칸, 아래 숫자 2칸)

    


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


    // MusicLayoutConfig.cs (개선된 버전 예시)
    public static float GetNoteVisualWidth(float measureVisualWidth, TimeSignature timeSignature, int noteDataDuration, bool isDotted)
    {
        // noteDataDuration: 1(온), 2(2분), 4(4분), 8(8분), 16(16분)
        // 음표가 온음표(whole note)를 기준으로 얼마나 짧은지를 나타내는 값
        float noteValueRelativeToWhole = 1.0f / noteDataDuration;
        if (isDotted)
        {
            noteValueRelativeToWhole *= 1.5f;
        }

        // 박자표에서 한 마디가 온음표 기준으로 얼마만큼의 길이를 가지는지 계산
        // 예: 4/4 -> 1.0 (온음표 1개), 3/4 -> 0.75 (온음표의 3/4), 6/8 -> 0.75 (온음표의 6/8)
        float measureValueRelativeToWhole = (float)timeSignature.beatsPerMeasure / timeSignature.beatUnitType;

        if (measureValueRelativeToWhole == 0) return 0; // 0으로 나누기 방지

        // 해당 음표가 현재 마디에서 차지하는 비율
        float proportionOfMeasure = noteValueRelativeToWhole / measureValueRelativeToWhole;

        return measureVisualWidth * proportionOfMeasure;
    }

    // TimeSignature 클래스 또는 구조체 (박자표 정보를 담기 위함)
    public struct TimeSignature
    {
        public int beatsPerMeasure; // 예: 4/4에서 4
        public int beatUnitType;    // 예: 4/4에서 4 (4분음표가 기준)

        public TimeSignature(int beats, int unit)
        {
            beatsPerMeasure = beats;
            beatUnitType = unit;
        }
    }


}
