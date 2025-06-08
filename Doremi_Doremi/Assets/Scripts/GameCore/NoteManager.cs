using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 음표 관리 및 검색을 담당하는 클래스
/// - 악보에서 음표 객체들을 찾고 관리
/// - 음표 그룹 정렬 및 선택
/// </summary>
public class NoteManager : MonoBehaviour
{
    [Header("검색 설정")]
    [SerializeField] private Transform staffPanel;
    [SerializeField] private bool autoFindStaffPanel = true;
    [SerializeField] private bool sortByPosition = true;
    
    // 찾은 음표들
    private List<GameObject> noteGroups = new List<GameObject>();
    private List<string> musicNotes = new List<string>();
    
    public int NoteCount => noteGroups.Count;
    public List<GameObject> NoteGroups => new List<GameObject>(noteGroups);
    
    private void Awake()
    {
        if (autoFindStaffPanel && staffPanel == null)
        {
            FindStaffPanel();
        }
    }
    
    /// <summary>
    /// Staff Panel 자동 검색
    /// </summary>
    private void FindStaffPanel()
    {
        GameObject staffPanelObj = GameObject.Find("Staff_Panel");
        if (staffPanelObj != null)
        {
            staffPanel = staffPanelObj.transform;
            Debug.Log("✅ Staff Panel 자동 검색 성공");
        }
        else
        {
            Debug.LogWarning("⚠️ Staff Panel을 찾을 수 없습니다");
        }
    }
    
    /// <summary>
    /// 곡 데이터를 기반으로 음표 설정
    /// </summary>
    public void SetupNotes(List<string> songNotes)
    {
        // 실제 음표만 추출 (쉼표, 마디선 제외)
        musicNotes.Clear();
        foreach (string note in songNotes)
        {
            if (!IsRestOrBarLine(note))
            {
                musicNotes.Add(note);
            }
        }
        
        // 악보에서 음표 객체들 찾기
        FindNoteGroups();
        
        Debug.Log($"🎵 음표 설정 완료: {musicNotes.Count}개 음표, {noteGroups.Count}개 객체");
    }
    
    /// <summary>
    /// 악보에서 음표 그룹들 찾기
    /// </summary>
    private void FindNoteGroups()
    {
        noteGroups.Clear();
        
        if (staffPanel == null)
        {
            Debug.LogWarning("⚠️ Staff Panel이 설정되지 않았습니다");
            return;
        }
        
        List<GameObject> allNoteGroups = new List<GameObject>();
        FindNoteGroupsRecursive(staffPanel, allNoteGroups);
        
        // 위치별로 정렬
        if (sortByPosition)
        {
            allNoteGroups.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        }
        
        // 필요한 개수만큼 선택
        int notesToSelect = Mathf.Min(musicNotes.Count, allNoteGroups.Count);
        for (int i = 0; i < notesToSelect; i++)
        {
            noteGroups.Add(allNoteGroups[i]);
        }
        
        Debug.Log($"🎼 {noteGroups.Count}개 음표 그룹 발견");
    }
    
    /// <summary>
    /// 재귀적으로 음표 그룹 검색
    /// </summary>
    private void FindNoteGroupsRecursive(Transform parent, List<GameObject> noteGroups)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            
            if (IsNoteGroup(child))
            {
                noteGroups.Add(child.gameObject);
            }
            else
            {
                FindNoteGroupsRecursive(child, noteGroups);
            }
        }
    }
    
    /// <summary>
    /// 음표 그룹인지 판별
    /// </summary>
    private bool IsNoteGroup(Transform obj)
    {
        // 1. 이름 기반 식별
        if (obj.name.StartsWith("head-") || 
            obj.name.StartsWith("note-") || 
            obj.name.StartsWith("Note") ||
            obj.name.Contains("Head"))
        {
            return true;
        }
        
        // 2. 태그 기반 식별
        if (obj.CompareTag("Note") || obj.CompareTag("NoteHead"))
        {
            return true;
        }
        
        // 3. 컴포넌트 기반 식별
        if (obj.GetComponent<Image>() != null)
        {
            Transform stem = obj.Find("stem");
            Transform flag = obj.Find("flag");
            if (stem != null || flag != null)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 쉼표나 마디선인지 확인
    /// </summary>
    private bool IsRestOrBarLine(string noteData)
    {
        return noteData.StartsWith("REST") || 
               noteData.StartsWith("rest") || 
               noteData.Contains("rest") ||
               noteData == "|" || 
               noteData.Contains("TUPLET") || 
               noteData.Contains("DOUBLE") || 
               noteData.Contains("BAR");
    }
    
    /// <summary>
    /// 특정 인덱스의 음표 그룹 반환
    /// </summary>
    public GameObject GetNoteGroup(int index)
    {
        if (index >= 0 && index < noteGroups.Count)
        {
            return noteGroups[index];
        }
        return null;
    }
    
    /// <summary>
    /// 특정 인덱스의 음표 데이터 반환
    /// </summary>
    public string GetMusicNote(int index)
    {
        if (index >= 0 && index < musicNotes.Count)
        {
            return musicNotes[index];
        }
        return "";
    }
    
    /// <summary>
    /// 모든 음표 그룹 반환 (복사본)
    /// </summary>
    public List<GameObject> GetAllNoteGroups()
    {
        return new List<GameObject>(noteGroups);
    }
    
    /// <summary>
    /// 모든 음표 데이터 반환 (복사본)
    /// </summary>
    public List<string> GetAllMusicNotes()
    {
        return new List<string>(musicNotes);
    }
    
    /// <summary>
    /// 음표 매니저 초기화
    /// </summary>
    public void Clear()
    {
        noteGroups.Clear();
        musicNotes.Clear();
    }
    
    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    [ContextMenu("디버그 - 현재 음표 정보")]
    public void DebugNoteInfo()
    {
        Debug.Log("=== 음표 매니저 정보 ===");
        Debug.Log($"Staff Panel: {(staffPanel != null ? staffPanel.name : "없음")}");
        Debug.Log($"음표 데이터: {musicNotes.Count}개");
        Debug.Log($"음표 객체: {noteGroups.Count}개");
        
        for (int i = 0; i < Mathf.Min(musicNotes.Count, noteGroups.Count); i++)
        {
            string noteData = i < musicNotes.Count ? musicNotes[i] : "없음";
            string objName = i < noteGroups.Count && noteGroups[i] != null ? noteGroups[i].name : "없음";
            Debug.Log($"  {i + 1}: {noteData} → {objName}");
        }
    }
    
    /// <summary>
    /// 수동으로 음표 그룹 다시 검색
    /// </summary>
    [ContextMenu("음표 다시 검색")]
    public void RefreshNoteGroups()
    {
        FindNoteGroups();
        Debug.Log($"🔄 음표 그룹 다시 검색: {noteGroups.Count}개 발견");
    }
}
