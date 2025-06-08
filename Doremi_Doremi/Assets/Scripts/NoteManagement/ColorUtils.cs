using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;

/// <summary>
/// 색상 변경 핵심 로직을 담당하는 유틸리티 클래스
/// </summary>
public static class ColorUtils
{
    /// <summary>
    /// SVGImage에 색상 적용
    /// </summary>
    public static void ApplyColorToSVGImage(SVGImage svgImage, Color color)
    {
        if (svgImage == null) return;
        
        svgImage.color = color;
        
        // CanvasRenderer를 통한 추가 보장
        CanvasRenderer renderer = svgImage.GetComponent<CanvasRenderer>();
        if (renderer != null)
        {
            renderer.SetColor(color);
        }
    }
    
    /// <summary>
    /// 일반 Image에 색상 적용
    /// </summary>
    public static void ApplyColorToImage(Image image, Color color)
    {
        if (image == null) return;
        
        image.color = color;
        
        // CanvasRenderer를 통한 추가 보장
        CanvasRenderer renderer = image.GetComponent<CanvasRenderer>();
        if (renderer != null)
        {
            renderer.SetColor(color);
        }
    }
    
    /// <summary>
    /// GameObject와 모든 자식에 색상 적용
    /// </summary>
    public static void ApplyColorToHierarchy(GameObject obj, Color color)
    {
        if (obj == null) return;
        
        // SVGImage 우선 처리
        SVGImage[] svgImages = obj.GetComponentsInChildren<SVGImage>(true);
        foreach (SVGImage svgImg in svgImages)
        {
            ApplyColorToSVGImage(svgImg, color);
        }
        
        // 일반 Image 처리 (SVGImage가 아닌 것만)
        Image[] images = obj.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (!(img is SVGImage))
            {
                ApplyColorToImage(img, color);
            }
        }
        
        // CanvasRenderer 직접 처리
        CanvasRenderer[] renderers = obj.GetComponentsInChildren<CanvasRenderer>(true);
        foreach (CanvasRenderer renderer in renderers)
        {
            renderer.SetColor(color);
        }
    }
    
    /// <summary>
    /// 색상을 문자열로 변환 (디버깅용)
    /// </summary>
    public static string ColorToString(Color color)
    {
        if (ColorEquals(color, Color.green)) return "✅ 정답 (녹색)";
        if (ColorEquals(color, Color.red)) return "❌ 오답 (빨강)";
        if (ColorEquals(color, Color.blue)) return "🔵 현재 (파랑)";
        if (ColorEquals(color, Color.yellow)) return "✨ 강조 (노랑)";
        if (ColorEquals(color, Color.black)) return "⚫ 기본 (검정)";
        return $"🎨 사용자 정의 ({color.r:F2}, {color.g:F2}, {color.b:F2})";
    }
    
    /// <summary>
    /// 색상 비교 (부동소수점 오차 고려)
    /// </summary>
    public static bool ColorEquals(Color a, Color b, float threshold = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < threshold && 
               Mathf.Abs(a.g - b.g) < threshold && 
               Mathf.Abs(a.b - b.b) < threshold && 
               Mathf.Abs(a.a - b.a) < threshold;
    }
}
