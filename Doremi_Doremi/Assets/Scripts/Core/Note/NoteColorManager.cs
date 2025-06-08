using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 음표 및 잇단음표 색상 관리 시스템
/// </summary>
public class NoteColorManager : MonoBehaviour
{
    [Header("색상 설정")]
    public Color defaultColor = Color.black;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public Color currentColor = Color.blue;
    public Color highlightColor = Color.yellow;

    // 색상 백업 데이터 저장소
    private Dictionary<GameObject, ColorBackupData> colorBackups = new Dictionary<GameObject, ColorBackupData>();

    void Awake()
    {
        Debug.Log("🎨 NoteColorManager 초기화 완료");
    }

    /// <summary>
    /// 오브젝트 색상 변경
    /// </summary>
    public void ChangeNoteColor(GameObject noteObject, Color color)
    {
        if (noteObject == null) return;

        // 첫 번째 색상 변경 시 백업 생성
        if (!colorBackups.ContainsKey(noteObject))
        {
            BackupOriginalColor(noteObject);
        }

        ApplyColorToObject(noteObject, color);
    }

    /// <summary>
    /// 원본 색상으로 복원
    /// </summary>
    public void RestoreNoteColor(GameObject noteObject)
    {
        if (noteObject == null || !colorBackups.ContainsKey(noteObject)) return;

        ColorBackupData backup = colorBackups[noteObject];
        
        // Image 컴포넌트 복원
        Image image = noteObject.GetComponent<Image>();
        if (image != null && backup.numberColor != Color.clear)
        {
            image.color = backup.numberColor;
            if (backup.numberMaterial != null)
            {
                image.material = backup.numberMaterial;
            }
        }

        // 자식 오브젝트들도 복원
        RestoreChildrenColors(noteObject, backup);
    }

    /// <summary>
    /// 원본 색상 백업
    /// </summary>
    private void BackupOriginalColor(GameObject noteObject)
    {
        ColorBackupData backup = new ColorBackupData();

        // Image 컴포넌트 백업
        Image image = noteObject.GetComponent<Image>();
        if (image != null)
        {
            backup.numberColor = image.color;
            backup.numberMaterial = image.material;
        }

        colorBackups[noteObject] = backup;
    }

    /// <summary>
    /// 오브젝트에 색상 적용
    /// </summary>
    private void ApplyColorToObject(GameObject obj, Color color)
    {
        // Image 컴포넌트들 색상 변경
        Image[] images = obj.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            img.color = color;
        }

        // CanvasRenderer 색상 변경
        CanvasRenderer[] renderers = obj.GetComponentsInChildren<CanvasRenderer>(true);
        foreach (CanvasRenderer renderer in renderers)
        {
            renderer.SetColor(color);
        }
    }

    /// <summary>
    /// 자식 오브젝트들 색상 복원
    /// </summary>
    private void RestoreChildrenColors(GameObject parent, ColorBackupData backup)
    {
        // 간단한 구현: 모든 자식에 기본 색상 적용
        Image[] childImages = parent.GetComponentsInChildren<Image>(true);
        foreach (Image img in childImages)
        {
            if (backup.numberColor != Color.clear)
            {
                img.color = backup.numberColor;
            }
            else
            {
                img.color = defaultColor;
            }
        }
    }

    /// <summary>
    /// 색상 백업 데이터 정리
    /// </summary>
    public void ClearBackupData(GameObject noteObject)
    {
        if (colorBackups.ContainsKey(noteObject))
        {
            colorBackups.Remove(noteObject);
        }
    }

    /// <summary>
    /// 모든 백업 데이터 정리
    /// </summary>
    public void ClearAllBackupData()
    {
        foreach (var backup in colorBackups.Values)
        {
            backup.Reset();
        }
        colorBackups.Clear();
        Debug.Log("🗑️ 모든 색상 백업 데이터 정리 완료");
    }

    /// <summary>
    /// 편의 메서드: 정답 색상 적용
    /// </summary>
    public void SetCorrectColor(GameObject noteObject)
    {
        ChangeNoteColor(noteObject, correctColor);
    }

    /// <summary>
    /// 편의 메서드: 오답 색상 적용
    /// </summary>
    public void SetIncorrectColor(GameObject noteObject)
    {
        ChangeNoteColor(noteObject, incorrectColor);
    }

    /// <summary>
    /// 편의 메서드: 현재 색상 적용
    /// </summary>
    public void SetCurrentColor(GameObject noteObject)
    {
        ChangeNoteColor(noteObject, currentColor);
    }

    /// <summary>
    /// 편의 메서드: 강조 색상 적용
    /// </summary>
    public void SetHighlightColor(GameObject noteObject)
    {
        ChangeNoteColor(noteObject, highlightColor);
    }

    /// <summary>
    /// 편의 메서드: 기본 색상 적용
    /// </summary>
    public void SetDefaultColor(GameObject noteObject)
    {
        ChangeNoteColor(noteObject, defaultColor);
    }

    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    [ContextMenu("색상 백업 상태 확인")]
    public void PrintBackupStatus()
    {
        Debug.Log($"🎨 === NoteColorManager 상태 ===");
        Debug.Log($"백업된 오브젝트 수: {colorBackups.Count}개");
        Debug.Log($"기본 색상: {defaultColor}");
        Debug.Log($"정답 색상: {correctColor}");
        Debug.Log($"오답 색상: {incorrectColor}");
        Debug.Log($"현재 색상: {currentColor}");
        Debug.Log($"강조 색상: {highlightColor}");
        
        int validBackups = 0;
        foreach (var kvp in colorBackups)
        {
            if (kvp.Key != null && kvp.Value.IsValid())
            {
                validBackups++;
            }
        }
        Debug.Log($"유효한 백업: {validBackups}개");
    }

    /// <summary>
    /// 색상 설정 테스트
    /// </summary>
    [ContextMenu("색상 테스트")]
    public void TestColors()
    {
        Debug.Log("🎨 색상 시스템 테스트 완료 - 실제 음표 오브젝트가 있을 때 색상 변경을 테스트할 수 있습니다.");
    }

    void OnDestroy()
    {
        ClearAllBackupData();
    }
}
