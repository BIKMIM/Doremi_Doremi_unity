using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 씬의 음표들을 찾고 관리하는 클래스
/// - 음표 자동 감지
/// - 위치 기반 정렬
/// - 유효성 검사
/// </summary>
public class NoteFinder : MonoBehaviour
{
    [Header("검색 설정")]
    [SerializeField] private Transform staffPanel;
    [SerializeField] private bool autoFindStaffPanel = true;
    [SerializeField] private bool sortByPosition = true;
    [SerializeField] private bool includeInactive = false;
    
    [Header("검색 조건")]
    [SerializeField] private List<string> noteNamePatterns = new List<string> 
    { 
        "head-", "note-", "Note", "Head" 
    };
    [SerializeField] private List<string> noteTagPatterns = new List<string> 
    { 
        "Note", "NoteHead" 
    };
    
    private List<GameObject> foundNoteGroups = new List<GameObject>();
    
    private void Awake()
    {
        if (autoFindStaffPanel && staffPanel == null)
        {
            FindStaffPanel();
        }
    }
    
    /// <summary>
    /// Staff Panel 자동 찾기
    /// </summary>
    private void FindStaffPanel()
    {
        GameObject staffPanelObj = GameObject.Find("Staff_Panel");
        if (staffPanelObj != null)
        {
            staffPanel = staffPanelObj.transform;
            Debug.Log($"📍 Staff Panel 자동 발견: {staffPanel.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Staff_Panel을 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 모든 음표 그룹 찾기
    /// </summary>
    public List<GameObject> FindAllNoteGroups()
    {
        foundNoteGroups.Clear();
        
        if (staffPanel == null)
        {
            Debug.LogWarning("⚠️ Staff Panel이 설정되지 않았습니다.");
            return foundNoteGroups;
        }
        
        SearchNoteGroupsRecursive(staffPanel);
        
        if (sortByPosition)
        {
            SortNotesByPosition();
        }
        
        ValidateNoteGroups();
        
        Debug.Log($"🎼 총 {foundNoteGroups.Count}개의 음표 그룹을 찾았습니다.");
        LogFoundNotes();
        
        return new List<GameObject>(foundNoteGroups);
    }
    
    /// <summary>
    /// 재귀적으로 음표 그룹 검색
    /// </summary>
    private void SearchNoteGroupsRecursive(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            
            if (!includeInactive && !child.gameObject.activeInHierarchy)
                continue;
            
            if (IsNoteGroup(child.gameObject))
            {
                foundNoteGroups.Add(child.gameObject);
            }
            else
            {
                SearchNoteGroupsRecursive(child);
            }
        }
    }
    
    /// <summary>
    /// 객체가 음표 그룹인지 판단
    /// </summary>
    private bool IsNoteGroup(GameObject obj)
    {
        // 1. 이름 기반 검사
        foreach (string pattern in noteNamePatterns)
        {
            if (obj.name.Contains(pattern))
                return true;
        }
        
        // 2. 태그 기반 검사
        foreach (string tagPattern in noteTagPatterns)
        {
            if (obj.tag == tagPattern)
                return true;
        }
        
        // 3. 컴포넌트 기반 검사 (SVGImage나 Image가 있고 stem/flag 자식이 있는 경우)
        if (obj.GetComponent<UnityEngine.UI.Image>() != null || 
            obj.GetComponent<Unity.VectorGraphics.SVGImage>() != null)
        {
            Transform stem = obj.transform.Find("stem");
            Transform flag = obj.transform.Find("flag");
            if (stem != null || flag != null)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 위치 기준으로 음표 정렬
    /// </summary>
    private void SortNotesByPosition()
    {
        foundNoteGroups.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
    }
    
    /// <summary>
    /// 찾은 음표들의 유효성 검사
    /// </summary>
    private void ValidateNoteGroups()
    {
        foundNoteGroups.RemoveAll(noteGroup => 
        {
            if (noteGroup == null)
            {
                Debug.LogWarning("⚠️ null 음표 그룹이 발견되어 제거됩니다.");
                return true;
            }
            
            // SVGImage나 Image 컴포넌트가 있는지 확인
            bool hasImageComponent = noteGroup.GetComponent<UnityEngine.UI.Image>() != null ||
                                   noteGroup.GetComponent<Unity.VectorGraphics.SVGImage>() != null;
            
            if (!hasImageComponent)
            {
                Debug.LogWarning($"⚠️ {noteGroup.name}에 Image 컴포넌트가 없어 제거됩니다.");
                return true;
            }
            
            return false;
        });
    }
    
    /// <summary>
    /// 찾은 음표들 로그 출력
    /// </summary>
    private void LogFoundNotes()
    {
        for (int i = 0; i < foundNoteGroups.Count; i++)
        {
            GameObject note = foundNoteGroups[i];
            Vector3 pos = note.transform.position;
            string componentInfo = GetComponentInfo(note);
            
            Debug.Log($"🎼 음표 {i + 1}: {note.name} " +
                     $"(위치: {pos.x:F1}, {pos.y:F1}) " +
                     $"[{componentInfo}]");
        }
    }
    
    /// <summary>
    /// 객체의 컴포넌트 정보 반환
    /// </summary>
    private string GetComponentInfo(GameObject obj)
    {
        List<string> components = new List<string>();
        
        if (obj.GetComponent<Unity.VectorGraphics.SVGImage>() != null)
            components.Add("SVGImage");
        if (obj.GetComponent<UnityEngine.UI.Image>() != null)
            components.Add("Image");
        if (obj.transform.Find("stem") != null)
            components.Add("stem");
        if (obj.transform.Find("flag") != null)
            components.Add("flag");
        
        return string.Join(", ", components);
    }
    
    /// <summary>
    /// 특정 개수만큼의 음표만 반환
    /// </summary>
    public List<GameObject> GetNoteGroups(int maxCount)
    {
        if (foundNoteGroups.Count == 0)
        {
            FindAllNoteGroups();
        }
        
        int count = Mathf.Min(maxCount, foundNoteGroups.Count);
        return foundNoteGroups.Take(count).ToList();
    }
    
    /// <summary>
    /// 인덱스로 특정 음표 반환
    /// </summary>
    public GameObject GetNoteAt(int index)
    {
        if (index >= 0 && index < foundNoteGroups.Count)
            return foundNoteGroups[index];
        return null;
    }
    
    /// <summary>
    /// 찾은 음표 개수 반환
    /// </summary>
    public int GetNoteCount()
    {
        return foundNoteGroups.Count;
    }
    
    /// <summary>
    /// 수동으로 Staff Panel 설정
    /// </summary>
    public void SetStaffPanel(Transform panel)
    {
        staffPanel = panel;
        Debug.Log($"📍 Staff Panel 수동 설정: {panel?.name ?? "null"}");
    }
    
    // === 디버그 메서드들 ===
    
    [ContextMenu("음표 다시 찾기")]
    public void RefreshNoteGroups()
    {
        FindAllNoteGroups();
    }
    
    [ContextMenu("Staff Panel 다시 찾기")]
    public void RefreshStaffPanel()
    {
        FindStaffPanel();
    }
    
    [ContextMenu("찾은 음표들 로그 출력")]
    public void LogAllFoundNotes()
    {
        if (foundNoteGroups.Count == 0)
        {
            Debug.Log("🔍 아직 음표를 찾지 않았습니다. FindAllNoteGroups()를 먼저 호출하세요.");
            return;
        }
        
        Debug.Log($"🎼 현재 찾은 음표들 ({foundNoteGroups.Count}개):");
        LogFoundNotes();
    }
}
