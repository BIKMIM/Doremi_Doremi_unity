using UnityEngine;
using UnityEngine.EventSystems;


// ��ư Ŭ�� �� �ִϸ��̼ǰ� ���带 Ʈ�����ϴ� ������Ʈ
// �� ��ũ��Ʈ�� UI ��ư ������Ʈ�� ���̸� Ŭ�� �� ������ �ִϸ��̼ǰ� ���带 ������
public class ButtonAnimationTrigger : MonoBehaviour, IPointerDownHandler
{

    // Inspector�� �����(�ۺ�) Ŭ�� ���� Ŭ��
    public AudioClip clickSound;

    // ���� ����� ���� AudioSource ������Ʈ ����
    private AudioSource audioSource;

    // ��ư �ִϸ��̼��� ������ Animator ������Ʈ ����
    private Animator animator;



    // ���� ������Ʈ�� Ȱ��ȭ�� ���� �� �� �����ϴ� ��ŸƮ �Լ�
    void Start()
    {
        // ���� ������Ʈ�� ���� Animator ������Ʈ�� ã�� ������ ����
        animator = GetComponent<Animator>();

        // AudioSource ������Ʈ�� �߰��ϰ� ������ ����
        // PlayOneShot�� ���� Ŭ�� ���带 ���� ����� �� ����
        audioSource = gameObject.AddComponent<AudioSource>();
    }



    // IPointerDownHandler �������̽� ���� �޼���
    // ���콺/��ġ�� UI ��Ҹ� ������ ���� ȣ���
    public void OnPointerDown(PointerEventData eventData)
    {

        // Animator�� "Pressed" Ʈ���Ÿ� �ߵ����� Ŭ�� �ִϸ��̼� ����
        animator.SetTrigger("Pressed");
        // clickSound�� ���� Ŭ���� �Ҵ�Ǿ� ���� ���� ���
        if (clickSound != null)
        {
            // ������ ����� Ŭ���� �� �� ���
            audioSource.PlayOneShot(clickSound);
        }
    }
}
