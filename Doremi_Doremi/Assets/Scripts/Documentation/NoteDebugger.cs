using UnityEngine;

/// <summary>
/// 음표 생성 문제 진단 및 해결 도구
/// </summary>
public class NoteDebugger : MonoBehaviour
{
    [Header("테스트 대상")]
    public ModularNoteAssembler assembler;
    public NoteSpawner noteSpawner;
    public RectTransform staffPanel;

    [Header("테스트 설정")]
    public Vector2 testPosition = new Vector2(100, 0);
    public int testDuration = 4;
    public float testNoteIndex = 0f;

    [ContextMenu("1. 전체 시스템 진단")]
    public void DiagnoseSystem()
    {
        Debug.Log("🔍 === 음표 시스템 전체 진단 ===");
        
        // 1. 기본 컴포넌트 확인
        CheckBasicComponents();
        
        // 2. 프리팹 할당 확인
        CheckPrefabAssignments();
        
        // 3. NoteSpawner 연결 확인
        CheckNoteSpawnerConnection();
        
        // 4. StaffPanel 확인
        CheckStaffPanel();
    }

    private void CheckBasicComponents()
    {
        Debug.Log("📋 기본 컴포넌트 확인:");
        
        if (assembler == null)
        {
            assembler = FindObjectOfType<ModularNoteAssembler>();
            Debug.Log($"   ModularNoteAssembler: {(assembler != null ? "✅ 자동 발견" : "❌ 없음")}");
        }
        else
        {
            Debug.Log("   ModularNoteAssembler: ✅ 할당됨");
        }

        if (noteSpawner == null)
        {
            noteSpawner = FindObjectOfType<NoteSpawner>();
            Debug.Log($"   NoteSpawner: {(noteSpawner != null ? "✅ 자동 발견" : "❌ 없음")}");
        }
        else
        {
            Debug.Log("   NoteSpawner: ✅ 할당됨");
        }

        if (staffPanel == null)
        {
            GameObject staffObj = GameObject.Find("Staff_Panel");
            if (staffObj != null)
            {
                staffPanel = staffObj.GetComponent<RectTransform>();
                Debug.Log("   StaffPanel: ✅ 자동 발견");
            }
            else
            {
                Debug.Log("   StaffPanel: ❌ 없음");
            }
        }
        else
        {
            Debug.Log("   StaffPanel: ✅ 할당됨");
        }
    }

    private void CheckPrefabAssignments()
    {
        Debug.Log("🎵 프리팹 할당 확인:");
        
        if (assembler == null) return;

        var headCreator = assembler.headCreator;
        if (headCreator != null)
        {
            Debug.Log($"   Head1Prefab: {(headCreator.head1Prefab != null ? "✅" : "❌ 누락")}");
            Debug.Log($"   Head2Prefab: {(headCreator.head2Prefab != null ? "✅" : "❌ 누락")}");
            Debug.Log($"   Head4Prefab: {(headCreator.head4Prefab != null ? "✅" : "❌ 누락")} ← 중요!");
        }
        else
        {
            Debug.Log("   ❌ NoteHeadCreator가 없습니다!");
        }

        var stemCreator = assembler.stemCreator;
        if (stemCreator != null)
        {
            Debug.Log($"   StemPrefab: {(stemCreator.stemPrefab != null ? "✅" : "❌ 누락")}");
        }
        else
        {
            Debug.Log("   ❌ NoteStemCreator가 없습니다!");
        }
    }

    private void CheckNoteSpawnerConnection()
    {
        Debug.Log("🔗 NoteSpawner 연결 확인:");
        
        if (noteSpawner == null)
        {
            Debug.Log("   ❌ NoteSpawner가 없습니다!");
            return;
        }

        Debug.Log($"   noteSpawner.assembler: {(noteSpawner.assembler != null ? "✅ 연결됨" : "❌ 연결 안됨")}");
        
        if (noteSpawner.assembler != null && noteSpawner.assembler != assembler)
        {
            Debug.Log("   ⚠️ NoteSpawner가 다른 ModularNoteAssembler를 참조하고 있습니다!");
        }
    }

    private void CheckStaffPanel()
    {
        Debug.Log("📋 StaffPanel 확인:");
        
        if (staffPanel == null)
        {
            Debug.Log("   ❌ StaffPanel이 없습니다!");
            return;
        }

        Debug.Log($"   StaffPanel 이름: {staffPanel.name}");
        Debug.Log($"   자식 오브젝트 수: {staffPanel.childCount}개");
        Debug.Log($"   크기: {staffPanel.rect.width} x {staffPanel.rect.height}");
        
        if (assembler != null && assembler.staffPanel != staffPanel)
        {
            Debug.Log("   ⚠️ ModularNoteAssembler가 다른 StaffPanel을 참조하고 있습니다!");
        }
    }

    [ContextMenu("2. 수동 음표 생성 테스트")]
    public void TestManualNoteCreation()
    {
        Debug.Log("🧪 === 수동 음표 생성 테스트 ===");
        
        if (assembler == null)
        {
            Debug.LogError("❌ ModularNoteAssembler가 없습니다!");
            return;
        }

        try
        {
            Debug.Log($"🎵 음표 생성 시도: 위치={testPosition}, duration={testDuration}");
            
            GameObject note = assembler.CreateNote(testPosition, testNoteIndex, testDuration);
            
            if (note != null)
            {
                Debug.Log($"✅ 음표 생성 성공!");
                Debug.Log($"   생성된 오브젝트: {note.name}");
                Debug.Log($"   위치: {note.transform.position}");
                Debug.Log($"   부모: {note.transform.parent.name}");
                Debug.Log($"   활성화 상태: {note.activeInHierarchy}");
                
                // 5초 후 삭제 (테스트용)
                Destroy(note, 5f);
                Debug.Log("   (5초 후 자동 삭제됩니다)");
            }
            else
            {
                Debug.LogError("❌ 음표 생성 실패: null 반환");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 음표 생성 중 오류: {e.Message}");
        }
    }

    [ContextMenu("3. NoteSpawner 강제 연결")]
    public void ForceConnectNoteSpawner()
    {
        Debug.Log("🔗 === NoteSpawner 강제 연결 ===");
        
        if (noteSpawner == null)
        {
            noteSpawner = FindObjectOfType<NoteSpawner>();
        }

        if (assembler == null)
        {
            assembler = FindObjectOfType<ModularNoteAssembler>();
        }

        if (noteSpawner != null && assembler != null)
        {
            noteSpawner.assembler = assembler;
            Debug.Log("✅ NoteSpawner.assembler 연결 완료");
            
            // NotePlacementHandler도 연결
            var placementHandler = FindObjectOfType<NotePlacementHandler>();
            if (placementHandler != null)
            {
                placementHandler.assembler = assembler;
                Debug.Log("✅ NotePlacementHandler.assembler 연결 완료");
            }
        }
        else
        {
            Debug.LogError("❌ 연결 실패: 필요한 컴포넌트를 찾을 수 없습니다");
        }
    }

    [ContextMenu("4. 게임 실행 테스트")]
    public void TestGameExecution()
    {
        Debug.Log("🎮 === 게임 실행 테스트 ===");
        
        if (noteSpawner == null)
        {
            Debug.LogError("❌ NoteSpawner가 없습니다!");
            return;
        }

        try
        {
            // NoteSpawner의 현재 곡 새로고침
            noteSpawner.RefreshCurrentSong();
            Debug.Log("✅ 곡 새로고침 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 게임 실행 테스트 실패: {e.Message}");
        }
    }

    [ContextMenu("5. 모든 문제 자동 수정")]
    public void AutoFix()
    {
        Debug.Log("🔧 === 자동 수정 시작 ===");
        
        // 1. 컴포넌트 찾기
        if (assembler == null) assembler = FindObjectOfType<ModularNoteAssembler>();
        if (noteSpawner == null) noteSpawner = FindObjectOfType<NoteSpawner>();
        if (staffPanel == null)
        {
            GameObject staffObj = GameObject.Find("Staff_Panel");
            if (staffObj != null) staffPanel = staffObj.GetComponent<RectTransform>();
        }

        // 2. 연결 수정
        if (noteSpawner != null && assembler != null)
        {
            noteSpawner.assembler = assembler;
            Debug.Log("✅ NoteSpawner 연결 수정");
        }

        if (assembler != null && staffPanel != null)
        {
            assembler.staffPanel = staffPanel;
            Debug.Log("✅ StaffPanel 연결 수정");
        }

        // 3. NotePlacementHandler 연결
        var placementHandler = FindObjectOfType<NotePlacementHandler>();
        if (placementHandler != null && assembler != null)
        {
            placementHandler.assembler = assembler;
            Debug.Log("✅ NotePlacementHandler 연결 수정");
        }

        // 4. 컴포넌트 재초기화
        if (assembler != null)
        {
            assembler.ForceReinitialize();
            Debug.Log("✅ ModularNoteAssembler 재초기화");
        }

        Debug.Log("🎉 자동 수정 완료! 이제 테스트해보세요.");
    }
}
