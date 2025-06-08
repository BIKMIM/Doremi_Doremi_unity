using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
using System.Collections.Generic;

/// <summary>
/// 간소화된 음표 색상 관리자
/// - SVG 프리팹 완전 지원
/// - 모듈화된 구조로 유지보수성 향상
/// </summary>
public class NoteColorManager : MonoBehaviour
{
    [Header("기본 색상 설정")]
    public Color defaultColor = Color.black;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public Color currentColor = Color.blue;
    public Color highlightColor = Color.yellow;
    
    // 음표별 색상 백업 데이터
    private Dictionary<GameObject, ColorBackupData> colorBackups = new Dictionary<GameObject, ColorBackupData>();
    
    /// <summary>
    /// 음표 그룹의 색상을 변경
    /// </summary>
    public void ChangeNoteColor(GameObject noteGroup, Color color)
    {
        if (noteGroup == null) return;
        
        // 백업 데이터 저장 (최초 한 번만)
        if (!colorBackups.ContainsKey(noteGroup))
        {
            colorBackups[noteGroup] = BackupColors(noteGroup);
        }
        
        // 색상 적용
        ColorUtils.ApplyColorToHierarchy(noteGroup, color);
        
        Debug.Log($"🎼 음표 색상 변경: {noteGroup.name} → {ColorUtils.ColorToString(color)}");
    }
    
    /// <summary>
    /// 음표 그룹의 색상을 타입별로 변경
    /// </summary>
    public void ChangeNoteColor(GameObject noteGroup, NoteColorType colorType)
    {
        Color targetColor = GetColorByType(colorType);
        ChangeNoteColor(noteGroup, targetColor);
    }
    
    /// <summary>
    /// 음표 색상을 원래대로 복원
    /// </summary>
    public void RestoreNoteColor(GameObject noteGroup)
    {
        if (noteGroup == null || !colorBackups.ContainsKey(noteGroup))
            return;
        
        ColorBackupData backup = colorBackups[noteGroup];
        RestoreFromBackup(noteGroup, backup);
        
        Debug.Log($"🎼 음표 색상 복원: {noteGroup.name}");
    }
    
    /// <summary>
    /// 모든 등록된 음표들을 원래 색상으로 복원
    /// </summary>
    public void RestoreAllNoteColors()
    {
        foreach (var kvp in colorBackups)
        {
            if (kvp.Key != null)
            {
                RestoreFromBackup(kvp.Key, kvp.Value);
            }
        }
        
        Debug.Log($"🎼 모든 음표 색상 복원 완료 ({colorBackups.Count}개)");
    }
    
    /// <summary>
    /// 원본 색상 백업
    /// </summary>
    private ColorBackupData BackupColors(GameObject noteGroup)
    {
        ColorBackupData backup = new ColorBackupData();
        
        // SVGImage 백업
        SVGImage[] svgImages = noteGroup.GetComponentsInChildren<SVGImage>(true);
        foreach (SVGImage svgImg in svgImages)
        {
            ComponentColorInfo info = new ComponentColorInfo
            {
                svgComponent = svgImg,
                originalColor = svgImg.color,
                originalMaterial = svgImg.material
            };
            backup.svgImageColors.Add(info);
        }
        
        // 일반 Image 백업
        Image[] images = noteGroup.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (!(img is SVGImage))
            {
                ComponentColorInfo info = new ComponentColorInfo
                {
                    component = img,
                    originalColor = img.color,
                    originalMaterial = img.material
                };
                backup.imageColors.Add(info);
            }
        }
        
        // CanvasRenderer 백업
        CanvasRenderer[] renderers = noteGroup.GetComponentsInChildren<CanvasRenderer>(true);
        foreach (CanvasRenderer renderer in renderers)
        {
            ComponentColorInfo info = new ComponentColorInfo
            {
                canvasRenderer = renderer,
                originalColor = renderer.GetColor()
            };
            backup.rendererColors.Add(info);
        }
        
        Debug.Log($"🎨 원본 색상 백업: {noteGroup.name} (SVG: {backup.svgImageColors.Count}, Image: {backup.imageColors.Count})");
        
        return backup;
    }
    
    /// <summary>
    /// 백업으로부터 색상 복원
    /// </summary>
    private void RestoreFromBackup(GameObject noteGroup, ColorBackupData backup)
    {
        // SVGImage 복원
        foreach (var info in backup.svgImageColors)
        {
            if (info.svgComponent != null)
            {
                info.svgComponent.color = info.originalColor;
                if (info.originalMaterial != null)
                {
                    info.svgComponent.material = info.originalMaterial;
                }
            }
        }
        
        // Image 복원
        foreach (var info in backup.imageColors)
        {
            if (info.component != null)
            {
                info.component.color = info.originalColor;
                if (info.originalMaterial != null)
                {
                    info.component.material = info.originalMaterial;
                }
            }
        }
        
        // CanvasRenderer 복원
        foreach (var info in backup.rendererColors)
        {
            if (info.canvasRenderer != null)
            {
                info.canvasRenderer.SetColor(info.originalColor);
            }
        }
    }
    
    /// <summary>
    /// 색상 타입에 따른 색상 반환
    /// </summary>
    private Color GetColorByType(NoteColorType colorType)
    {
        return colorType switch
        {
            NoteColorType.Default => defaultColor,
            NoteColorType.Correct => correctColor,
            NoteColorType.Incorrect => incorrectColor,
            NoteColorType.Current => currentColor,
            NoteColorType.Highlight => highlightColor,
            _ => defaultColor
        };
    }
    
    // === 편의 메서드들 ===
    
    public void SetCorrectColor(GameObject noteGroup) => ChangeNoteColor(noteGroup, correctColor);
    public void SetIncorrectColor(GameObject noteGroup) => ChangeNoteColor(noteGroup, incorrectColor);
    public void SetCurrentColor(GameObject noteGroup) => ChangeNoteColor(noteGroup, currentColor);
    public void SetHighlightColor(GameObject noteGroup) => ChangeNoteColor(noteGroup, highlightColor);
    public void SetDefaultColor(GameObject noteGroup) => ChangeNoteColor(noteGroup, defaultColor);
    
    // === 색상 설정 변경 ===
    
    public void SetCorrectColor(Color color) => correctColor = color;
    public void SetIncorrectColor(Color color) => incorrectColor = color;
    public void SetCurrentColor(Color color) => currentColor = color;
    public void SetHighlightColor(Color color) => highlightColor = color;
    public void SetDefaultColor(Color color) => defaultColor = color;
    
    // === 디버그 메서드들 ===
    
    [ContextMenu("모든 색상 복원")]
    public void RestoreAllColors()
    {
        RestoreAllNoteColors();
    }
    
    [ContextMenu("캐시 정리")]
    public void ClearCache()
    {
        colorBackups.Clear();
        Debug.Log("🧹 색상 백업 캐시 정리 완료");
    }
    
    [ContextMenu("색상 테스트")]
    public void TestColors()
    {
        // 첫 번째 SVGImage 찾아서 테스트
        SVGImage[] svgImages = FindObjectsOfType<SVGImage>();
        if (svgImages.Length > 0)
        {
            GameObject testTarget = svgImages[0].gameObject;
            StartCoroutine(TestColorSequence(testTarget));
        }
        else
        {
            Debug.LogWarning("⚠️ 테스트할 SVGImage를 찾을 수 없습니다");
        }
    }
    
    private System.Collections.IEnumerator TestColorSequence(GameObject noteGroup)
    {
        Debug.Log("🎨 색상 테스트 시작...");
        
        ChangeNoteColor(noteGroup, NoteColorType.Current);
        yield return new WaitForSeconds(1f);
        
        ChangeNoteColor(noteGroup, NoteColorType.Correct);
        yield return new WaitForSeconds(1f);
        
        ChangeNoteColor(noteGroup, NoteColorType.Incorrect);
        yield return new WaitForSeconds(1f);
        
        ChangeNoteColor(noteGroup, NoteColorType.Highlight);
        yield return new WaitForSeconds(1f);
        
        RestoreNoteColor(noteGroup);
        Debug.Log("🎨 색상 테스트 완료");
    }
}
