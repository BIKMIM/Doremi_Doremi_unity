using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
using System.Collections.Generic;

/// <summary>
/// 색상 변경에 사용되는 데이터 클래스들
/// </summary>
[System.Serializable]
public class ColorBackupData
{
    public List<ComponentColorInfo> imageColors = new List<ComponentColorInfo>();
    public List<ComponentColorInfo> svgImageColors = new List<ComponentColorInfo>();
    public List<ComponentColorInfo> rendererColors = new List<ComponentColorInfo>();
}

[System.Serializable]
public class ComponentColorInfo
{
    public Image component;
    public SVGImage svgComponent;
    public CanvasRenderer canvasRenderer;
    public Color originalColor;
    public Material originalMaterial;
}

/// <summary>
/// 음표 색상 타입 열거형
/// </summary>
public enum NoteColorType
{
    Default,    // 기본 색상 (검정)
    Correct,    // 정답 색상 (녹색)
    Incorrect,  // 오답 색상 (빨강)
    Current,    // 현재 음표 색상 (파랑)
    Highlight   // 강조 색상 (노랑)
}
