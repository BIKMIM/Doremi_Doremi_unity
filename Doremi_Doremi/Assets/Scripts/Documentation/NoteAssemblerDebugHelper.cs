using UnityEngine;

/// <summary>
/// 모듈화된 음표 조립 시스템의 디버그 도우미
/// </summary>
public class NoteAssemblerDebugHelper : MonoBehaviour
{
    [Header("디버그 대상")]
    public ModularNoteAssembler assembler;
    public NoteSpawner noteSpawner;
    public NotePlacementHandler placementHandler;

    [Header("테스트 설정")]
    public Vector2 testPosition = new Vector2(0, 0);
    public float testNoteIndex = 0f; // B4
    public int testDuration = 4;

    [ContextMenu("시스템 상태 확인")]
    public void CheckSystemStatus()
    {
        Debug.Log("🔍 === 음표 시스템 상태 확인 ===");
        
        // ModularNoteAssembler 확인
        if (assembler == null)
        {
            Debug.LogError("❌ ModularNoteAssembler가 할당되지 않았습니다!");
        }
        else
        {
            Debug.Log($"✅ ModularNoteAssembler 발견: {assembler.name}");
            CheckModularAssemblerComponents();
        }

        // NoteSpawner 확인
        if (noteSpawner == null)
        {
            Debug.LogError("❌ NoteSpawner가 할당되지 않았습니다!");
        }
        else
        {
            Debug.Log($"✅ NoteSpawner 발견: {noteSpawner.name}");
            CheckNoteSpawnerComponents();
        }

        // NotePlacementHandler 확인
        if (placementHandler == null)
        {
            Debug.LogError("❌ NotePlacementHandler가 할당되지 않았습니다!");
        }
        else
        {
            Debug.Log($"✅ NotePlacementHandler 발견: {placementHandler.name}");
            CheckPlacementHandlerComponents();
        }
    }

    private void CheckModularAssemblerComponents()
    {
        if (assembler.staffPanel == null)
        {
            Debug.LogError("❌ ModularNoteAssembler.staffPanel이 null입니다!");
        }
        else
        {
            Debug.Log($"✅ StaffPanel: {assembler.staffPanel.name}");
        }

        // 각 컴포넌트들 확인
        var headCreator = assembler.headCreator;
        var stemCreator = assembler.stemCreator;
        var flagCreator = assembler.flagCreator;
        var dotCreator = assembler.dotCreator;
        var restCreator = assembler.restCreator;

        Debug.Log($"🎵 HeadCreator: {(headCreator != null ? "✅" : "❌")}");
        Debug.Log($"🦴 StemCreator: {(stemCreator != null ? "✅" : "❌")}");
        Debug.Log($"🎏 FlagCreator: {(flagCreator != null ? "✅" : "❌")}");
        Debug.Log($"🎯 DotCreator: {(dotCreator != null ? "✅" : "❌")}");
        Debug.Log($"💤 RestCreator: {(restCreator != null ? "✅" : "❌")}");

        // 프리팹들 확인
        if (headCreator != null)
        {
            CheckPrefabs(headCreator);
        }
    }

    private void CheckPrefabs(NoteHeadCreator headCreator)
    {
        Debug.Log("🎵 === 음표 머리 프리팹 확인 ===");
        Debug.Log($"head1Prefab: {(headCreator.head1Prefab != null ? "✅" : "❌")}");
        Debug.Log($"head2Prefab: {(headCreator.head2Prefab != null ? "✅" : "❌")}");
        Debug.Log($"head4Prefab: {(headCreator.head4Prefab != null ? "✅" : "❌")}");
    }

    private void CheckNoteSpawnerComponents()
    {
        Debug.Log($"🎼 NoteSpawner.assembler: {(noteSpawner.assembler != null ? "✅" : "❌")}");
        Debug.Log($"📊 NoteSpawner.staffPanel: {(noteSpawner.staffPanel != null ? "✅" : "❌")}");
        Debug.Log($"🎯 NoteSpawner.notePlacementHandler: {(noteSpawner.notePlacementHandler != null ? "✅" : "❌")}");
    }

    private void CheckPlacementHandlerComponents()
    {
        Debug.Log($"🔧 PlacementHandler.assembler: {(placementHandler.assembler != null ? "✅" : "❌")}");
    }

    [ContextMenu("테스트 음표 생성")]
    public void TestCreateNote()
    {
        if (assembler == null)
        {
            Debug.LogError("❌ ModularNoteAssembler가 없습니다!");
            return;
        }

        Debug.Log($"🎵 테스트 음표 생성 시도: 위치={testPosition}, noteIndex={testNoteIndex}, duration={testDuration}");

        try
        {
            GameObject note = assembler.CreateNote(testPosition, testNoteIndex, testDuration);
            
            if (note != null)
            {
                Debug.Log($"✅ 음표 생성 성공: {note.name}");
                Debug.Log($"   위치: {note.transform.position}");
                Debug.Log($"   부모: {(note.transform.parent != null ? note.transform.parent.name : "없음")}");
            }
            else
            {
                Debug.LogError("❌ 음표 생성 실패: null 반환");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 음표 생성 중 오류: {e.Message}");
            Debug.LogError($"스택트레이스: {e.StackTrace}");
        }
    }

    [ContextMenu("컴포넌트 자동 찾기")]
    public void FindComponents()
    {
        if (assembler == null)
            assembler = FindObjectOfType<ModularNoteAssembler>();
        
        if (noteSpawner == null)
            noteSpawner = FindObjectOfType<NoteSpawner>();
            
        if (placementHandler == null)
            placementHandler = FindObjectOfType<NotePlacementHandler>();

        Debug.Log("🔍 컴포넌트 자동 찾기 완료");
        CheckSystemStatus();
    }

    [ContextMenu("ModularNoteAssembler 컴포넌트 강제 재초기화")]
    public void ForceReinitializeAssembler()
    {
        if (assembler == null)
        {
            Debug.LogError("❌ ModularNoteAssembler가 없습니다!");
            return;
        }

        // 강제로 컴포넌트들 재할당
        var go = assembler.gameObject;
        
        if (assembler.headCreator == null)
        {
            assembler.headCreator = go.GetComponent<NoteHeadCreator>();
            if (assembler.headCreator == null)
            {
                assembler.headCreator = go.AddComponent<NoteHeadCreator>();
                Debug.Log("🎵 NoteHeadCreator 추가됨");
            }
        }

        if (assembler.stemCreator == null)
        {
            assembler.stemCreator = go.GetComponent<NoteStemCreator>();
            if (assembler.stemCreator == null)
            {
                assembler.stemCreator = go.AddComponent<NoteStemCreator>();
                Debug.Log("🦴 NoteStemCreator 추가됨");
            }
        }

        if (assembler.flagCreator == null)
        {
            assembler.flagCreator = go.GetComponent<NoteFlagCreator>();
            if (assembler.flagCreator == null)
            {
                assembler.flagCreator = go.AddComponent<NoteFlagCreator>();
                Debug.Log("🎏 NoteFlagCreator 추가됨");
            }
        }

        if (assembler.dotCreator == null)
        {
            assembler.dotCreator = go.GetComponent<NoteDotCreator>();
            if (assembler.dotCreator == null)
            {
                assembler.dotCreator = go.AddComponent<NoteDotCreator>();
                Debug.Log("🎯 NoteDotCreator 추가됨");
            }
        }

        if (assembler.restCreator == null)
        {
            assembler.restCreator = go.GetComponent<RestNoteCreator>();
            if (assembler.restCreator == null)
            {
                assembler.restCreator = go.AddComponent<RestNoteCreator>();
                Debug.Log("💤 RestNoteCreator 추가됨");
            }
        }

        Debug.Log("🔄 ModularNoteAssembler 컴포넌트 재초기화 완료");
        CheckSystemStatus();
    }
}
