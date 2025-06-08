using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 음표 가시성 문제 해결 도구
/// </summary>
public class NoteVisibilityFixer : MonoBehaviour
{
    [Header("테스트 설정")]
    public Color testColor = Color.red;
    public float testScale = 2f;

    [ContextMenu("1. 모든 음표 찾아서 빨간색으로 표시")]
    public void HighlightAllNotes()
    {
        Debug.Log("🔍 === 모든 음표 찾아서 하이라이트 ===");
        
        GameObject staffPanel = GameObject.Find("Staff_Panel");
        if (staffPanel == null)
        {
            Debug.LogError("❌ Staff_Panel을 찾을 수 없습니다!");
            return;
        }

        int foundCount = 0;
        HighlightNotesRecursive(staffPanel.transform, ref foundCount);
        
        Debug.Log($"✅ 총 {foundCount}개의 음표 오브젝트를 빨간색으로 표시했습니다!");
        
        if (foundCount == 0)
        {
            Debug.LogWarning("⚠️ 음표 오브젝트를 찾을 수 없습니다. 다른 방법을 시도해보세요.");
        }
    }

    private void HighlightNotesRecursive(Transform parent, ref int count)
    {
        foreach (Transform child in parent)
        {
            // Image 컴포넌트가 있는 오브젝트들 확인
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                // 음표 관련 이름인지 확인
                string name = child.name.ToLower();
                if (name.Contains("note") || name.Contains("head") || name.Contains("stem") || 
                    name.Contains("flag") || name.Contains("rest") || name.Contains("dot") ||
                    name.Contains("tuplet") || name.Contains("beam"))
                {
                    // 빨간색으로 변경하고 크기 키우기
                    image.color = testColor;
                    child.localScale = Vector3.one * testScale;
                    
                    Debug.Log($"🎵 발견: {child.name} at {child.position}");
                    count++;
                }
            }

            // 자식들도 재귀적으로 검색
            HighlightNotesRecursive(child, ref count);
        }
    }

    [ContextMenu("2. 특정 위치에 테스트 음표 생성")]
    public void CreateTestNoteAtCenter()
    {
        Debug.Log("🧪 === 화면 중앙에 테스트 음표 생성 ===");
        
        GameObject staffPanel = GameObject.Find("Staff_Panel");
        if (staffPanel == null)
        {
            Debug.LogError("❌ Staff_Panel을 찾을 수 없습니다!");
            return;
        }

        // 테스트 음표 생성
        GameObject testNote = new GameObject("TEST_NOTE");
        testNote.transform.SetParent(staffPanel.transform, false);

        // RectTransform 설정
        RectTransform rt = testNote.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; // 중앙
        rt.sizeDelta = new Vector2(100, 100); // 큰 크기

        // Image 컴포넌트 추가
        Image image = testNote.AddComponent<Image>();
        image.color = Color.red;

        Debug.Log($"✅ 테스트 음표 생성 완료: {testNote.name}");
        Debug.Log($"   위치: {rt.anchoredPosition}");
        Debug.Log($"   크기: {rt.sizeDelta}");
        
        // 5초 후 삭제
        Destroy(testNote, 5f);
        Debug.Log("   (5초 후 자동 삭제됩니다)");
    }

    [ContextMenu("3. 모든 음표 위치 정보 출력")]
    public void PrintAllNotePositions()
    {
        Debug.Log("📍 === 모든 음표 위치 정보 ===");
        
        GameObject staffPanel = GameObject.Find("Staff_Panel");
        if (staffPanel == null)
        {
            Debug.LogError("❌ Staff_Panel을 찾을 수 없습니다!");
            return;
        }

        RectTransform staffRT = staffPanel.GetComponent<RectTransform>();
        Debug.Log($"StaffPanel 크기: {staffRT.rect.width} x {staffRT.rect.height}");
        Debug.Log($"StaffPanel 위치: {staffRT.anchoredPosition}");

        int foundCount = 0;
        PrintPositionsRecursive(staffPanel.transform, ref foundCount);
        
        if (foundCount == 0)
        {
            Debug.LogWarning("⚠️ 음표 오브젝트를 찾을 수 없습니다!");
        }
    }

    private void PrintPositionsRecursive(Transform parent, ref int count)
    {
        foreach (Transform child in parent)
        {
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                string name = child.name.ToLower();
                if (name.Contains("note") || name.Contains("head") || name.Contains("stem") || 
                    name.Contains("flag") || name.Contains("rest") || name.Contains("dot") ||
                    name.Contains("tuplet") || name.Contains("beam"))
                {
                    RectTransform rt = child.GetComponent<RectTransform>();
                    Debug.Log($"🎵 {child.name}:");
                    Debug.Log($"   위치: {rt.anchoredPosition}");
                    Debug.Log($"   크기: {rt.sizeDelta}");
                    Debug.Log($"   스케일: {child.localScale}");
                    Debug.Log($"   색상: {image.color}");
                    Debug.Log($"   활성화: {child.gameObject.activeInHierarchy}");
                    
                    count++;
                }
            }

            PrintPositionsRecursive(child, ref count);
        }
    }

    [ContextMenu("4. 음표 크기와 색상 수정")]
    public void FixNoteSizeAndColor()
    {
        Debug.Log("🔧 === 음표 크기와 색상 자동 수정 ===");
        
        GameObject staffPanel = GameObject.Find("Staff_Panel");
        if (staffPanel == null)
        {
            Debug.LogError("❌ Staff_Panel을 찾을 수 없습니다!");
            return;
        }

        int fixedCount = 0;
        FixNotesRecursive(staffPanel.transform, ref fixedCount);
        
        Debug.Log($"✅ 총 {fixedCount}개의 음표 오브젝트를 수정했습니다!");
    }

    private void FixNotesRecursive(Transform parent, ref int count)
    {
        foreach (Transform child in parent)
        {
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                string name = child.name.ToLower();
                if (name.Contains("note") || name.Contains("head") || name.Contains("stem") || 
                    name.Contains("flag") || name.Contains("rest") || name.Contains("dot") ||
                    name.Contains("tuplet") || name.Contains("beam"))
                {
                    // 크기 수정 (너무 작으면 키우기)
                    RectTransform rt = child.GetComponent<RectTransform>();
                    if (rt.sizeDelta.x < 10 || rt.sizeDelta.y < 10)
                    {
                        rt.sizeDelta = new Vector2(50, 50);
                        Debug.Log($"🔧 {child.name} 크기 수정: {rt.sizeDelta}");
                    }

                    // 색상 수정 (투명하거나 보이지 않으면 검은색으로)
                    if (image.color.a < 0.1f || image.color == Color.clear)
                    {
                        image.color = Color.black;
                        Debug.Log($"🎨 {child.name} 색상 수정: 검은색");
                    }

                    // 활성화 상태 확인
                    if (!child.gameObject.activeInHierarchy)
                    {
                        child.gameObject.SetActive(true);
                        Debug.Log($"🔛 {child.name} 활성화");
                    }

                    count++;
                }
            }

            FixNotesRecursive(child, ref count);
        }
    }
}
