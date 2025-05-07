using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimationTrigger : MonoBehaviour, IPointerDownHandler
{
    public AudioClip clickSound;
    private AudioSource audioSource;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetTrigger("Pressed");

        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
