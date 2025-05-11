using UnityEngine;
using UnityEngine.EventSystems;


// 버튼 클릭 시 애니메이션과 사운드를 트리거하는 컴포넌트
// 이 스크립트를 UI 버튼 오브젝트에 붙이면 클릭 시 지정한 애니메이션과 사운드를 실행함
public class ButtonAnimationTrigger : MonoBehaviour, IPointerDownHandler
{

    // Inspector에 노출될(퍼블릭) 클릭 사운드 클립
    public AudioClip clickSound;

    // 사운드 재생을 위한 AudioSource 컴포넌트 참조
    private AudioSource audioSource;

    // 버튼 애니메이션을 제어할 Animator 컴포넌트 참조
    private Animator animator;



    // 게임 오브젝트가 활성화된 직후 한 번 실행하는 스타트 함수
    void Start()
    {
        // 동일 오브젝트에 붙은 Animator 컴포넌트를 찾아 변수에 저장
        animator = GetComponent<Animator>();

        // AudioSource 컴포넌트를 추가하고 변수에 저장
        // PlayOneShot을 통해 클릭 사운드를 쉽게 재생할 수 있음
        audioSource = gameObject.AddComponent<AudioSource>();
    }



    // IPointerDownHandler 인터페이스 구현 메서드
    // 마우스/터치로 UI 요소를 누르는 순간 호출됨
    public void OnPointerDown(PointerEventData eventData)
    {

        // Animator의 "Pressed" 트리거를 발동시켜 클릭 애니메이션 실행
        animator.SetTrigger("Pressed");
        // clickSound에 사운드 클립이 할당되어 있을 때만 재생
        if (clickSound != null)
        {
            // 지정된 오디오 클립을 한 번 재생
            audioSource.PlayOneShot(clickSound);
        }
    }
}
