using UnityEngine;
using UnityEngine.UI;

public class PianoKey : MonoBehaviour
{
    [Header("Key Info")]
    [SerializeField] private string noteName; // "C", "C#", "D" 등
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button keyButton;
    
    private DynamicPianoMapper pianoMapper;
    
    public string NoteName => noteName;
    
    public void Initialize(string note, AudioSource audio, DynamicPianoMapper mapper)
    {
        noteName = note;
        audioSource = audio;
        pianoMapper = mapper;
        
        // 버튼 컴포넌트 가져오기
        keyButton = GetComponent<Button>();
        if (keyButton == null)
        {
            keyButton = gameObject.AddComponent<Button>();
        }
        
        // 클릭 이벤트 연결
        keyButton.onClick.RemoveAllListeners();
        keyButton.onClick.AddListener(PlaySound);
    }
    
    public void UpdateAudioClip(AudioClip newClip)
    {
        if (audioSource != null)
        {
            audioSource.clip = newClip;
        }
    }
    
    public void PlaySound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop(); // 이전 소리 정지
            audioSource.Play(); // 새 소리 재생
            
            // 시각적 피드백 (선택사항)
            StartCoroutine(KeyPressEffect());
        }
        else
        {
            Debug.LogWarning($"AudioSource or AudioClip is missing for key: {noteName}");
        }
    }
    
    private System.Collections.IEnumerator KeyPressEffect()
    {
        // 건반을 누른 효과 (색상 변경 등)
        Image keyImage = GetComponent<Image>();
        if (keyImage != null)
        {
            Color originalColor = keyImage.color;
            keyImage.color = Color.gray;
            
            yield return new WaitForSeconds(0.1f);
            
            keyImage.color = originalColor;
        }
    }
    
    // 외부에서 직접 호출할 수 있는 메서드
    public void TriggerKey()
    {
        PlaySound();
    }
}
