using UnityEngine;  // Unity 엔진의 핵심 클래스 및 기능 제공
using UnityEngine.EventSystems;  // UI 이벤트 인터페이스(IPointerDownHandler) 제공

/// <summary>
/// ButtonAnimationTrigger 클래스는 UI 버튼 클릭 시 애니메이터 트리거와 사운드를 재생하는 기능을 제공합니다.
/// </summary>
public class ButtonAnimationTrigger : MonoBehaviour, IPointerDownHandler
{
    [Header("🔊 클릭 사운드")]  // 인스펙터에서 클릭 사운드 필드 그룹화
    public AudioClip clickSound;  // 버튼 클릭 시 재생할 오디오 클립

    private AudioSource audioSource;  // 사운드 재생용 AudioSource 컴포넌트
    private Animator animator;        // 버튼 애니메이션 제어용 Animator 컴포넌트

    /// <summary>
    /// 컴포넌트 활성화 시 초기화 작업 수행
    /// </summary>
    void Start()
    {
        animator = GetComponent<Animator>();  // 동일 GameObject에 연결된 Animator 컴포넌트 가져오기
        // AudioSource 컴포넌트를 동적으로 추가하여 사운드 재생에 사용
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// IPointerDownHandler 인터페이스 구현 메서드
    /// 포인터(마우스/터치)가 버튼 위에서 눌린 순간 호출됩니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 관련 정보</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 'Pressed' 트리거 파라미터를 Animator에 설정하여 버튼 클릭 애니메이션 실행
        animator.SetTrigger("Pressed");

        // 클릭 사운드가 할당된 경우 한 번만 재생
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}