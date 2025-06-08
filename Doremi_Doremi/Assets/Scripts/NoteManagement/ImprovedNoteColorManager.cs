using UnityEngine;
using Unity.VectorGraphics;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/// <summary>
/// 향상된 음표 색상 관리자
/// - 즉시 색상 변경 및 복원
/// - 더 안정적인 SVGImage 지원
/// - 색상 변경 이벤트
/// - 자동 색상 복원 시스템
/// </summary>
public class ImprovedNoteColorManager : MonoBehaviour
{
    [Header("기본 색상 설정")]
    [SerializeField] private Color defaultColor = Color.black;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color currentColor = Color.blue;
    [SerializeField] private Color highlightColor = Color.yellow;
    
    [Header("동작 설정")]
    [SerializeField] private bool forceImmediateUpdate = true; // 즉시 강제 업데이트
    [SerializeField] private bool restoreOnDisable = true;     // 비활성화 시 자동 복원
    [SerializeField] private bool logColorChanges = true;      // 색상 변경 로깅
    
    // 이벤트들
    public event Action<GameObject, Color> OnColorChanged;
    public event Action<GameObject> OnColorRestored;
    public event Action OnAllColorsRestored;
    
    // 색상 백업 시스템
    private Dictionary<GameObject, OriginalColorData> originalColors = new Dictionary<GameObject, OriginalColorData>();
    
    private void OnDisable()
    {
        if (restoreOnDisable)
        {
            RestoreAllColors();
        }
    }
    
    /// <summary>
    /// 음표 색상을 즉시 변경
    /// </summary>
    public void ChangeNoteColorImmediate(GameObject noteObject, Color targetColor)
    {
        if (noteObject == null)
        {
            Debug.LogWarning("⚠️ ImprovedNoteColorManager: 음표 객체가 null입니다.");
            return;
        }
        
        // 원본 색상 백업 (처음 변경하는 경우만)
        if (!originalColors.ContainsKey(noteObject))
        {
            BackupOriginalColors(noteObject);
        }
        
        // 색상 변경 실행
        ApplyColorToNoteObject(noteObject, targetColor);
        
        // 강제 즉시 업데이트
        if (forceImmediateUpdate)
        {
            ForceCanvasUpdate(noteObject);
        }
        
        // 이벤트 발생
        OnColorChanged?.Invoke(noteObject, targetColor);
        
        if (logColorChanges)
        {
            Debug.Log($"🎨 음표 색상 즉시 변경: {noteObject.name} → {ColorToString(targetColor)}");
        }
    }
    
    /// <summary>
    /// 음표 색상을 원본으로 복원
    /// </summary>
    public void RestoreNoteColor(GameObject noteObject)
    {
        if (noteObject == null || !originalColors.ContainsKey(noteObject))
        {
            return;
        }
        
        OriginalColorData colorData = originalColors[noteObject];
        RestoreOriginalColorsToObject(noteObject, colorData);
        
        // 강제 즉시 업데이트
        if (forceImmediateUpdate)
        {
            ForceCanvasUpdate(noteObject);
        }
        
        // 백업 데이터 제거
        originalColors.Remove(noteObject);
        
        // 이벤트 발생
        OnColorRestored?.Invoke(noteObject);
        
        if (logColorChanges)
        {
            Debug.Log($"🎨 음표 색상 복원: {noteObject.name}");
        }
    }
    
    /// <summary>
    /// 모든 음표 색상을 원본으로 복원
    /// </summary>
    public void RestoreAllColors()
    {
        var noteObjects = new List<GameObject>(originalColors.Keys);
        
        foreach (GameObject noteObject in noteObjects)
        {
            if (noteObject != null)
            {
                RestoreNoteColor(noteObject);
            }
        }
        
        originalColors.Clear();
        OnAllColorsRestored?.Invoke();
        
        if (logColorChanges)
        {
            Debug.Log($"🎨 모든 음표 색상 복원 완료 ({noteObjects.Count}개)");
        }
    }
    
    /// <summary>
    /// 원본 색상 데이터 백업
    /// </summary>
    private void BackupOriginalColors(GameObject noteObject)
    {
        OriginalColorData colorData = new OriginalColorData();
        
        // SVGImage 백업
        SVGImage svgImage = noteObject.GetComponent<SVGImage>();
        if (svgImage != null)
        {
            colorData.hasSVGImage = true;
            colorData.originalSVGColor = svgImage.color;
            colorData.originalSVGMaterial = svgImage.material;
        }
        
        // Image 백업 (SVGImage가 아닌 경우)
        Image image = noteObject.GetComponent<Image>();
        if (image != null && !(image is SVGImage))
        {
            colorData.hasImage = true;
            colorData.originalImageColor = image.color;
            colorData.originalImageMaterial = image.material;
        }
        
        // CanvasRenderer 백업
        CanvasRenderer canvasRenderer = noteObject.GetComponent<CanvasRenderer>();
        if (canvasRenderer != null)
        {
            colorData.hasCanvasRenderer = true;
            colorData.originalCanvasColor = canvasRenderer.GetColor();
        }
        
        // 자식 객체들도 백업 (stem, flag 등)
        BackupChildColors(noteObject.transform, colorData);
        
        originalColors[noteObject] = colorData;
        
        if (logColorChanges)
        {
            Debug.Log($"💾 원본 색상 백업: {noteObject.name} " +
                     $"(SVG: {colorData.hasSVGImage}, Image: {colorData.hasImage}, " +
                     $"Canvas: {colorData.hasCanvasRenderer}, 자식: {colorData.childColors.Count})");
        }
    }
    
    /// <summary>
    /// 자식 객체들의 색상도 백업
    /// </summary>
    private void BackupChildColors(Transform parent, OriginalColorData colorData)
    {
        foreach (Transform child in parent)
        {
            // SVGImage 체크
            SVGImage childSVG = child.GetComponent<SVGImage>();
            if (childSVG != null)
            {
                colorData.childColors.Add(new ChildColorInfo
                {
                    transform = child,
                    hasSVGImage = true,
                    originalColor = childSVG.color,
                    originalMaterial = childSVG.material
                });
            }
            else
            {
                // 일반 Image 체크
                Image childImage = child.GetComponent<Image>();
                if (childImage != null)
                {
                    colorData.childColors.Add(new ChildColorInfo
                    {
                        transform = child,
                        hasImage = true,
                        originalColor = childImage.color,
                        originalMaterial = childImage.material
                    });
                }
            }
            
            // 재귀적으로 더 깊은 자식들도 확인
            BackupChildColors(child, colorData);
        }
    }
    
    /// <summary>
    /// 음표 객체에 색상 적용
    /// </summary>
    private void ApplyColorToNoteObject(GameObject noteObject, Color targetColor)
    {
        // 1. SVGImage 우선 처리
        SVGImage svgImage = noteObject.GetComponent<SVGImage>();
        if (svgImage != null)
        {
            svgImage.color = targetColor;
            
            // CanvasRenderer도 같이 업데이트
            CanvasRenderer canvasRenderer = svgImage.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                canvasRenderer.SetColor(targetColor);
            }
        }
        else
        {
            // 2. 일반 Image 처리
            Image image = noteObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = targetColor;
                
                CanvasRenderer canvasRenderer = image.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetColor(targetColor);
                }
            }
        }
        
        // 3. 자식 객체들도 같은 색상으로 변경
        ApplyColorToChildren(noteObject.transform, targetColor);
    }
    
    /// <summary>
    /// 자식 객체들에도 색상 적용
    /// </summary>
    private void ApplyColorToChildren(Transform parent, Color targetColor)
    {
        foreach (Transform child in parent)
        {
            // SVGImage 우선
            SVGImage childSVG = child.GetComponent<SVGImage>();
            if (childSVG != null)
            {
                childSVG.color = targetColor;
                
                CanvasRenderer canvasRenderer = childSVG.GetComponent<CanvasRenderer>();
                if (canvasRenderer != null)
                {
                    canvasRenderer.SetColor(targetColor);
                }
            }
            else
            {
                // 일반 Image
                Image childImage = child.GetComponent<Image>();
                if (childImage != null)
                {
                    childImage.color = targetColor;
                    
                    CanvasRenderer canvasRenderer = childImage.GetComponent<CanvasRenderer>();
                    if (canvasRenderer != null)
                    {
                        canvasRenderer.SetColor(targetColor);
                    }
                }
            }
            
            // 재귀적으로 더 깊은 자식들도 처리
            ApplyColorToChildren(child, targetColor);
        }
    }
    
    /// <summary>
    /// 원본 색상으로 복원
    /// </summary>
    private void RestoreOriginalColorsToObject(GameObject noteObject, OriginalColorData colorData)
    {
        // SVGImage 복원
        if (colorData.hasSVGImage)
        {
            SVGImage svgImage = noteObject.GetComponent<SVGImage>();
            if (svgImage != null)
            {
                svgImage.color = colorData.originalSVGColor;
                if (colorData.originalSVGMaterial != null)
                {
                    svgImage.material = colorData.originalSVGMaterial;
                }
            }
        }
        
        // Image 복원
        if (colorData.hasImage)
        {
            Image image = noteObject.GetComponent<Image>();
            if (image != null && !(image is SVGImage))
            {
                image.color = colorData.originalImageColor;
                if (colorData.originalImageMaterial != null)
                {
                    image.material = colorData.originalImageMaterial;
                }
            }
        }
        
        // CanvasRenderer 복원
        if (colorData.hasCanvasRenderer)
        {
            CanvasRenderer canvasRenderer = noteObject.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                canvasRenderer.SetColor(colorData.originalCanvasColor);
            }
        }
        
        // 자식 객체들 복원
        RestoreChildColors(colorData);
    }
    
    /// <summary>
    /// 자식 객체들의 색상 복원
    /// </summary>
    private void RestoreChildColors(OriginalColorData colorData)
    {
        foreach (ChildColorInfo childInfo in colorData.childColors)
        {
            if (childInfo.transform == null) continue;
            
            if (childInfo.hasSVGImage)
            {
                SVGImage childSVG = childInfo.transform.GetComponent<SVGImage>();
                if (childSVG != null)
                {
                    childSVG.color = childInfo.originalColor;
                    if (childInfo.originalMaterial != null)
                    {
                        childSVG.material = childInfo.originalMaterial;
                    }
                }
            }
            else if (childInfo.hasImage)
            {
                Image childImage = childInfo.transform.GetComponent<Image>();
                if (childImage != null)
                {
                    childImage.color = childInfo.originalColor;
                    if (childInfo.originalMaterial != null)
                    {
                        childImage.material = childInfo.originalMaterial;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Canvas 강제 업데이트
    /// </summary>
    private void ForceCanvasUpdate(GameObject noteObject)
    {
        Canvas canvas = noteObject.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Canvas.ForceUpdateCanvases();
        }
        
        // CanvasRenderer도 강제 업데이트
        CanvasRenderer[] renderers = noteObject.GetComponentsInChildren<CanvasRenderer>();
        foreach (CanvasRenderer renderer in renderers)
        {
            renderer.SetColor(renderer.GetColor()); // 자기 자신으로 재설정하여 강제 업데이트
        }
    }
    
    /// <summary>
    /// 색상을 문자열로 변환
    /// </summary>
    private string ColorToString(Color color)
    {
        if (ColorUtility.ToHtmlStringRGBA(color) == ColorUtility.ToHtmlStringRGBA(correctColor)) 
            return "✅ 정답(녹색)";
        if (ColorUtility.ToHtmlStringRGBA(color) == ColorUtility.ToHtmlStringRGBA(incorrectColor)) 
            return "❌ 오답(빨강)";
        if (ColorUtility.ToHtmlStringRGBA(color) == ColorUtility.ToHtmlStringRGBA(currentColor)) 
            return "🔵 현재(파랑)";
        if (ColorUtility.ToHtmlStringRGBA(color) == ColorUtility.ToHtmlStringRGBA(highlightColor)) 
            return "✨ 강조(노랑)";
        if (ColorUtility.ToHtmlStringRGBA(color) == ColorUtility.ToHtmlStringRGBA(defaultColor)) 
            return "⚫ 기본(검정)";
        
        return $"🎨 사용자({color.r:F2}, {color.g:F2}, {color.b:F2})";
    }
    
    // === 편의 메서드들 ===
    
    public void SetCorrectColor(GameObject noteObject) => ChangeNoteColorImmediate(noteObject, correctColor);
    public void SetIncorrectColor(GameObject noteObject) => ChangeNoteColorImmediate(noteObject, incorrectColor);
    public void SetCurrentColor(GameObject noteObject) => ChangeNoteColorImmediate(noteObject, currentColor);
    public void SetHighlightColor(GameObject noteObject) => ChangeNoteColorImmediate(noteObject, highlightColor);
    public void SetDefaultColor(GameObject noteObject) => ChangeNoteColorImmediate(noteObject, defaultColor);
    
    // === 색상 설정 변경 ===
    
    public void UpdateCorrectColor(Color color) { correctColor = color; }
    public void UpdateIncorrectColor(Color color) { incorrectColor = color; }
    public void UpdateCurrentColor(Color color) { currentColor = color; }
    public void UpdateHighlightColor(Color color) { highlightColor = color; }
    public void UpdateDefaultColor(Color color) { defaultColor = color; }
    
    // === 디버그 메서드들 ===
    
    [ContextMenu("모든 색상 복원")]
    public void DebugRestoreAllColors()
    {
        RestoreAllColors();
    }
    
    [ContextMenu("색상 백업 상태 확인")]
    public void DebugCheckBackupStatus()
    {
        Debug.Log($"🔍 현재 백업된 음표 수: {originalColors.Count}");
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                Debug.Log($"  📝 {kvp.Key.name}: SVG={kvp.Value.hasSVGImage}, " +
                         $"Image={kvp.Value.hasImage}, 자식={kvp.Value.childColors.Count}개");
            }
        }
    }
    
    [ContextMenu("강제 Canvas 업데이트")]
    public void DebugForceCanvasUpdate()
    {
        Canvas.ForceUpdateCanvases();
        Debug.Log("🔄 Canvas 강제 업데이트 완료");
    }
}

/// <summary>
/// 원본 색상 데이터 저장 클래스
/// </summary>
[System.Serializable]
public class OriginalColorData
{
    public bool hasSVGImage = false;
    public Color originalSVGColor = Color.white;
    public Material originalSVGMaterial;
    
    public bool hasImage = false;
    public Color originalImageColor = Color.white;
    public Material originalImageMaterial;
    
    public bool hasCanvasRenderer = false;
    public Color originalCanvasColor = Color.white;
    
    public List<ChildColorInfo> childColors = new List<ChildColorInfo>();
}

/// <summary>
/// 자식 객체 색상 정보
/// </summary>
[System.Serializable]
public class ChildColorInfo
{
    public Transform transform;
    public bool hasSVGImage = false;
    public bool hasImage = false;
    public Color originalColor = Color.white;
    public Material originalMaterial;
}
