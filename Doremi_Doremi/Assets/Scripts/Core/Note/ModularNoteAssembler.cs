using UnityEngine;

/// <summary>
/// 모듈화된 음표 조립 시스템
/// 각 컴포넌트들을 조합하여 완전한 음표를 생성
/// </summary>
public class ModularNoteAssembler : MonoBehaviour
{
    [Header("오선 패널")]
    public RectTransform staffPanel;

    [Header("컴포넌트 참조")]
    public NoteHeadCreator headCreator;
    public NoteStemCreator stemCreator;
    public NoteFlagCreator flagCreator;
    public NoteDotCreator dotCreator;
    public RestNoteCreator restCreator;

    [Header("디버그")]
    public bool showDebugInfo = true;

    void Awake()
    {
        InitializeComponents();
        ValidateComponents();
    }

    /// <summary>
    /// 컴포넌트 자동 초기화
    /// </summary>
    private void InitializeComponents()
    {
        if (showDebugInfo) Debug.Log("🔧 ModularNoteAssembler 컴포넌트 초기화 시작");

        // GameObject 참조
        GameObject go = gameObject;

        // 각 컴포넌트 찾기 또는 생성
        if (headCreator == null)
        {
            headCreator = go.GetComponent<NoteHeadCreator>();
            if (headCreator == null)
            {
                headCreator = go.AddComponent<NoteHeadCreator>();
                if (showDebugInfo) Debug.Log("🎵 NoteHeadCreator 자동 생성됨");
            }
        }

        if (stemCreator == null)
        {
            stemCreator = go.GetComponent<NoteStemCreator>();
            if (stemCreator == null)
            {
                stemCreator = go.AddComponent<NoteStemCreator>();
                if (showDebugInfo) Debug.Log("🦴 NoteStemCreator 자동 생성됨");
            }
        }

        if (flagCreator == null)
        {
            flagCreator = go.GetComponent<NoteFlagCreator>();
            if (flagCreator == null)
            {
                flagCreator = go.AddComponent<NoteFlagCreator>();
                if (showDebugInfo) Debug.Log("🎏 NoteFlagCreator 자동 생성됨");
            }
        }

        if (dotCreator == null)
        {
            dotCreator = go.GetComponent<NoteDotCreator>();
            if (dotCreator == null)
            {
                dotCreator = go.AddComponent<NoteDotCreator>();
                if (showDebugInfo) Debug.Log("🎯 NoteDotCreator 자동 생성됨");
            }
        }

        if (restCreator == null)
        {
            restCreator = go.GetComponent<RestNoteCreator>();
            if (restCreator == null)
            {
                restCreator = go.AddComponent<RestNoteCreator>();
                if (showDebugInfo) Debug.Log("💤 RestNoteCreator 자동 생성됨");
            }
        }

        if (showDebugInfo) Debug.Log("✅ ModularNoteAssembler 컴포넌트 초기화 완료");
    }

    /// <summary>
    /// 일반 음표 생성 (완전한 조립)
    /// </summary>
    public GameObject CreateNote(Vector2 position, float noteIndex, int duration)
    {
        if (showDebugInfo) Debug.Log($"🎵 음표 생성 시도: 위치={position}, noteIndex={noteIndex}, duration={duration}");

        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 null입니다!");
            return null;
        }

        if (headCreator == null)
        {
            Debug.LogError("❌ headCreator가 null입니다!");
            return null;
        }

        // 1. 머리 생성
        GameObject prefab = headCreator.GetHeadPrefab(duration);
        if (prefab == null)
        {
            Debug.LogError($"❌ duration {duration}에 대한 프리팹을 찾을 수 없습니다!");
            return null;
        }

        GameObject head = headCreator.CreateNoteHead(prefab, position, staffPanel);
        
        if (head == null)
        {
            Debug.LogError("❌ 음표 머리 생성 실패!");
            return null;
        }

        if (showDebugInfo) Debug.Log($"✅ 음표 머리 생성 성공: {head.name}");

        // 2. 스템 붙이기 (2분음표 이상)
        GameObject stem = null;
        if (duration >= 2 && stemCreator != null)
        {
            stem = stemCreator.AttachStem(head, noteIndex, staffPanel);
            if (showDebugInfo && stem != null) Debug.Log($"✅ 스템 생성 성공: {stem.name}");
        }

        // 3. 플래그 붙이기 (8분음표 이상)
        if (duration >= 8 && stem != null && flagCreator != null)
        {
            GameObject flag = flagCreator.AttachFlag(stem, duration, noteIndex, staffPanel);
            if (showDebugInfo && flag != null) Debug.Log($"✅ 플래그 생성 성공: {flag.name}");
        }

        if (showDebugInfo) Debug.Log($"🎉 음표 생성 완료: {head.name}");
        return head;
    }

    /// <summary>
    /// 잇단음표용 음표 생성 (flag 없이)
    /// </summary>
    public GameObject CreateTupletNote(Vector2 position, float noteIndex, int duration)
    {
        if (showDebugInfo) Debug.Log($"🎼 잇단음표용 음표 생성: 위치={position}, noteIndex={noteIndex}, duration={duration}");

        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 null입니다!");
            return null;
        }

        if (headCreator == null)
        {
            Debug.LogError("❌ headCreator가 null입니다!");
            return null;
        }

        // 1. 머리 생성
        GameObject prefab = headCreator.GetHeadPrefab(duration);
        if (prefab == null)
        {
            Debug.LogError($"❌ duration {duration}에 대한 프리팹을 찾을 수 없습니다!");
            return null;
        }

        GameObject head = headCreator.CreateNoteHead(prefab, position, staffPanel);
        
        if (head == null)
        {
            Debug.LogError("❌ 음표 머리 생성 실패!");
            return null;
        }

        if (showDebugInfo) Debug.Log($"✅ 잇단음표 머리 생성 성공: {head.name}");

        // 2. 스템 붙이기 (2분음표 이상) - flag는 붙이지 않음
        GameObject stem = null;
        if (duration >= 2 && stemCreator != null)
        {
            stem = stemCreator.AttachStem(head, noteIndex, staffPanel);
            if (showDebugInfo && stem != null) Debug.Log($"✅ 잇단음표 스템 생성 성공: {stem.name}");
        }

        // 3. 플래그는 잇단음표에서 생략 (beam으로 대체)

        if (showDebugInfo) Debug.Log($"🎉 잇단음표용 음표 생성 완료: {head.name} (flag 없음)");
        return head;
    }

    /// <summary>
    /// 점음표 생성
    /// </summary>
    public GameObject CreateDottedNote(Vector2 position, float noteIndex, int duration, bool isOnLine)
    {
        if (showDebugInfo) Debug.Log($"🎯 점음표 생성 시도: 위치={position}, noteIndex={noteIndex}, duration={duration}, isOnLine={isOnLine}");

        GameObject head = CreateNote(position, noteIndex, duration);
        
        if (head != null && dotCreator != null)
        {
            GameObject dot = dotCreator.AttachDot(head, isOnLine, staffPanel);
            if (showDebugInfo && dot != null) Debug.Log($"✅ 점 생성 성공: {dot.name}");
        }

        return head;
    }

    /// <summary>
    /// 잇단음표용 점음표 생성 (flag 없이)
    /// </summary>
    public GameObject CreateTupletDottedNote(Vector2 position, float noteIndex, int duration, bool isOnLine)
    {
        if (showDebugInfo) Debug.Log($"🎯🎼 잇단음표용 점음표 생성: 위치={position}, noteIndex={noteIndex}, duration={duration}, isOnLine={isOnLine}");

        GameObject head = CreateTupletNote(position, noteIndex, duration);
        
        if (head != null && dotCreator != null)
        {
            GameObject dot = dotCreator.AttachDot(head, isOnLine, staffPanel);
            if (showDebugInfo && dot != null) Debug.Log($"✅ 잇단음표 점 생성 성공: {dot.name}");
        }

        return head;
    }

    /// <summary>
    /// 쉼표 생성
    /// </summary>
    public GameObject CreateRest(Vector2 position, int duration, bool isDotted = false)
    {
        if (showDebugInfo) Debug.Log($"💤 쉼표 생성 시도: 위치={position}, duration={duration}, isDotted={isDotted}");

        if (restCreator == null)
        {
            Debug.LogError("❌ RestNoteCreator가 없습니다");
            return null;
        }

        if (staffPanel == null)
        {
            Debug.LogError("❌ staffPanel이 null입니다!");
            return null;
        }

        GameObject rest = restCreator.CreateRestNote(position, duration, staffPanel);
        
        if (rest != null && isDotted && dotCreator != null)
        {
            GameObject dot = dotCreator.AttachRestDot(rest, duration, staffPanel);
            if (showDebugInfo && dot != null) Debug.Log($"✅ 쉼표 점 생성 성공: {dot.name}");
        }

        if (showDebugInfo && rest != null) Debug.Log($"✅ 쉼표 생성 완료: {rest.name}");
        return rest;
    }

    /// <summary>
    /// 컴포넌트 유효성 검사
    /// </summary>
    private void ValidateComponents()
    {
        bool isValid = true;

        if (staffPanel == null) 
        { 
            Debug.LogError("❌ staffPanel이 할당되지 않았습니다"); 
            isValid = false; 
        }

        if (headCreator == null) 
        { 
            Debug.LogWarning("⚠️ NoteHeadCreator가 없습니다"); 
            isValid = false; 
        }
        else if (!headCreator.ValidatePrefabs())
        {
            Debug.LogWarning("⚠️ NoteHeadCreator 프리팹이 할당되지 않았습니다");
            isValid = false;
        }

        if (stemCreator == null) 
        { 
            Debug.LogWarning("⚠️ NoteStemCreator가 없습니다"); 
        }
        else if (!stemCreator.ValidatePrefab())
        {
            Debug.LogWarning("⚠️ NoteStemCreator 프리팹이 할당되지 않았습니다");
        }

        if (flagCreator == null) 
        { 
            Debug.LogWarning("⚠️ NoteFlagCreator가 없습니다"); 
        }
        else if (!flagCreator.ValidatePrefabs())
        {
            Debug.LogWarning("⚠️ NoteFlagCreator 프리팹이 할당되지 않았습니다");
        }

        if (dotCreator == null) 
        { 
            Debug.LogWarning("⚠️ NoteDotCreator가 없습니다"); 
        }
        else if (!dotCreator.ValidatePrefab())
        {
            Debug.LogWarning("⚠️ NoteDotCreator 프리팹이 할당되지 않았습니다");
        }

        if (restCreator == null) 
        { 
            Debug.LogWarning("⚠️ RestNoteCreator가 없습니다"); 
        }
        else if (!restCreator.ValidatePrefabs())
        {
            Debug.LogWarning("⚠️ RestNoteCreator 프리팹이 할당되지 않았습니다");
        }

        if (isValid)
        {
            Debug.Log("✅ ModularNoteAssembler 초기화 완료");
        }
        else
        {
            Debug.LogWarning("⚠️ ModularNoteAssembler 초기화 시 일부 문제 발견 - 프리팹을 할당해주세요");
        }
    }

    [ContextMenu("컴포넌트 상태 확인")]
    public void CheckComponentStatus()
    {
        Debug.Log("🔍 === ModularNoteAssembler 컴포넌트 상태 ===");
        Debug.Log($"StaffPanel: {(staffPanel != null ? "✅" : "❌")}");
        Debug.Log($"HeadCreator: {(headCreator != null ? "✅" : "❌")}");
        Debug.Log($"StemCreator: {(stemCreator != null ? "✅" : "❌")}");
        Debug.Log($"FlagCreator: {(flagCreator != null ? "✅" : "❌")}");
        Debug.Log($"DotCreator: {(dotCreator != null ? "✅" : "❌")}");
        Debug.Log($"RestCreator: {(restCreator != null ? "✅" : "❌")}");
        ValidateComponents();
    }

    [ContextMenu("컴포넌트 강제 재초기화")]
    public void ForceReinitialize()
    {
        InitializeComponents();
        ValidateComponents();
    }

    [ContextMenu("테스트 음표 생성")]
    public void TestCreateNote()
    {
        Vector2 testPos = new Vector2(0, 0);
        float testNoteIndex = 0f; // B4
        int testDuration = 4;

        Debug.Log($"🧪 테스트 음표 생성: 위치={testPos}, noteIndex={testNoteIndex}, duration={testDuration}");
        GameObject testNote = CreateNote(testPos, testNoteIndex, testDuration);
        
        if (testNote != null)
        {
            Debug.Log($"✅ 테스트 성공: {testNote.name} 생성됨");
        }
        else
        {
            Debug.LogError("❌ 테스트 실패: 음표 생성되지 않음");
        }
    }

    [ContextMenu("테스트 잇단음표 생성")]
    public void TestCreateTupletNote()
    {
        Vector2 testPos = new Vector2(100, 0);
        float testNoteIndex = 0f; // B4
        int testDuration = 8;

        Debug.Log($"🧪 테스트 잇단음표 생성: 위치={testPos}, noteIndex={testNoteIndex}, duration={testDuration}");
        GameObject testNote = CreateTupletNote(testPos, testNoteIndex, testDuration);
        
        if (testNote != null)
        {
            Debug.Log($"✅ 잇단음표 테스트 성공: {testNote.name} 생성됨");
        }
        else
        {
            Debug.LogError("❌ 잇단음표 테스트 실패: 음표 생성되지 않음");
        }
    }
}
