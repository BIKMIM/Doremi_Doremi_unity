using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 잇단음표 시각적 그룹 정보를 담는 클래스 (색상 변경 기능 포함)
/// </summary>
[System.Serializable]
public class TupletVisualGroup
{
    public TupletData tupletData;
    public GameObject numberObject;
    public GameObject beamObject;
    public List<GameObject> noteObjects;  // noteHeads -> noteObjects로 변경
    public List<GameObject> stemObjects;
    
    // 색상 백업 데이터
    private ColorBackupData colorBackup;
    private bool hasColorBackup = false;

    public TupletVisualGroup(TupletData data)
    {
        tupletData = data;
        noteObjects = new List<GameObject>();
        stemObjects = new List<GameObject>();
    }

    /// <summary>
    /// 잇단음표 그룹의 색상을 변경
    /// </summary>
    /// <param name="color">변경할 색상</param>
    public void ChangeColor(Color color)
    {
        // 처음 색상 변경시 백업 생성
        if (!hasColorBackup)
        {
            BackupOriginalColors();
        }
        
        ApplyColor(color);
    }
    
    /// <summary>
    /// 원본 색상으로 복원
    /// </summary>
    public void RestoreColor()
    {
        if (!hasColorBackup || colorBackup == null)
            return;
            
        RestoreOriginalColors();
    }
    
    /// <summary>
    /// 원본 색상 백업
    /// </summary>
    private void BackupOriginalColors()
    {
        colorBackup = new ColorBackupData();
        
        // 숫자 색상 백업
        if (numberObject != null)
        {
            Image numberImage = numberObject.GetComponent<Image>();
            if (numberImage != null)
            {
                colorBackup.numberColor = numberImage.color;
                colorBackup.numberMaterial = numberImage.material;
            }
        }
        
        // Beam 색상 백업
        if (beamObject != null)
        {
            Image beamImage = beamObject.GetComponent<Image>();
            if (beamImage != null)
            {
                colorBackup.beamColor = beamImage.color;
                colorBackup.beamMaterial = beamImage.material;
            }
        }
        
        hasColorBackup = true;
    }
    
    /// <summary>
    /// 색상 적용
    /// </summary>
    private void ApplyColor(Color color)
    {
        // 숫자 색상 변경
        if (numberObject != null)
        {
            ApplyColorToGameObject(numberObject, color);
        }
        
        // Beam 색상 변경
        if (beamObject != null)
        {
            ApplyColorToGameObject(beamObject, color);
        }
    }
    
    /// <summary>
    /// 개별 GameObject에 색상 적용
    /// </summary>
    private void ApplyColorToGameObject(GameObject obj, Color color)
    {
        if (obj == null) return;
        
        // Image 컴포넌트들 색상 변경
        Image[] images = obj.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            img.color = color;
        }
        
        // CanvasRenderer 색상 변경 (SVG 호환)
        CanvasRenderer[] renderers = obj.GetComponentsInChildren<CanvasRenderer>(true);
        foreach (CanvasRenderer renderer in renderers)
        {
            renderer.SetColor(color);
        }
    }
    
    /// <summary>
    /// 원본 색상 복원
    /// </summary>
    private void RestoreOriginalColors()
    {
        // 숫자 색상 복원
        if (numberObject != null && colorBackup.numberColor != Color.clear)
        {
            Image numberImage = numberObject.GetComponent<Image>();
            if (numberImage != null)
            {
                numberImage.color = colorBackup.numberColor;
                if (colorBackup.numberMaterial != null)
                {
                    numberImage.material = colorBackup.numberMaterial;
                }
            }
        }
        
        // Beam 색상 복원
        if (beamObject != null && colorBackup.beamColor != Color.clear)
        {
            Image beamImage = beamObject.GetComponent<Image>();
            if (beamImage != null)
            {
                beamImage.color = colorBackup.beamColor;
                if (colorBackup.beamMaterial != null)
                {
                    beamImage.material = colorBackup.beamMaterial;
                }
            }
        }
    }
    
    /// <summary>
    /// 모든 오브젝트 삭제
    /// </summary>
    public void DestroyAll()
    {
        if (numberObject != null)
            Object.DestroyImmediate(numberObject);
        if (beamObject != null)
            Object.DestroyImmediate(beamObject);

        // note와 stem은 다른 곳에서 관리되므로 여기서는 제거하지 않음
        noteObjects.Clear();
        stemObjects.Clear();
        
        // 색상 백업 정리
        if (colorBackup != null)
        {
            colorBackup.Reset();
        }
        hasColorBackup = false;
    }
    
    /// <summary>
    /// 그룹이 유효한지 확인
    /// </summary>
    public bool IsValid()
    {
        return tupletData != null && (numberObject != null || beamObject != null);
    }
    
    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    public void PrintDebugInfo()
    {
        Debug.Log($"=== TupletVisualGroup Debug Info ===");
        Debug.Log($"TupletData: {(tupletData != null ? tupletData.GetTupletTypeName() : "NULL")}");
        Debug.Log($"NumberObject: {(numberObject != null ? numberObject.name : "NULL")}");
        Debug.Log($"BeamObject: {(beamObject != null ? beamObject.name : "NULL")}");
        Debug.Log($"NoteObjects: {noteObjects.Count}개");
        Debug.Log($"StemObjects: {stemObjects.Count}개");
        Debug.Log($"ColorBackup: {(hasColorBackup ? "있음" : "없음")}");
    }
}
