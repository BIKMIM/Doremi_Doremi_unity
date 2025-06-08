using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 색상 백업 데이터를 저장하는 클래스
/// </summary>
[System.Serializable]
public class ColorBackupData
{
    [Header("숫자 색상 백업")]
    public Color numberColor = Color.clear;
    public Material numberMaterial;
    
    [Header("Beam 색상 백업")]
    public Color beamColor = Color.clear;
    public Material beamMaterial;
    
    /// <summary>
    /// 백업 데이터 초기화
    /// </summary>
    public void Reset()
    {
        numberColor = Color.clear;
        numberMaterial = null;
        beamColor = Color.clear;
        beamMaterial = null;
    }
    
    /// <summary>
    /// 백업 데이터가 유효한지 확인
    /// </summary>
    /// <returns>백업 데이터가 설정되어 있으면 true</returns>
    public bool IsValid()
    {
        return numberColor != Color.clear || beamColor != Color.clear || 
               numberMaterial != null || beamMaterial != null;
    }
    
    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    public void PrintDebugInfo()
    {
        Debug.Log($"=== ColorBackupData Debug Info ===");
        Debug.Log($"NumberColor: {numberColor}");
        Debug.Log($"NumberMaterial: {(numberMaterial != null ? numberMaterial.name : "NULL")}");
        Debug.Log($"BeamColor: {beamColor}");
        Debug.Log($"BeamMaterial: {(beamMaterial != null ? beamMaterial.name : "NULL")}");
        Debug.Log($"IsValid: {IsValid()}");
    }
}
