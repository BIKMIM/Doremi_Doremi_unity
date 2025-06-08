using System.Collections;
using UnityEngine;

/// <summary>
/// 간단한 메트로놈 시스템
/// 어드밴스드 모드에서 박자에 맞춰 게임을 진행할 때 사용
/// </summary>
public class SimpleMetronome : MonoBehaviour
{
    [Header("Metronome Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickSound;
    [SerializeField] private AudioClip tockSound; // 강박용 소리
    [SerializeField] private int bpm = 120; // beats per minute
    [SerializeField] private int beatsPerMeasure = 4; // 한 마디당 박자 수
    
    [Header("Visual Feedback")]
    [SerializeField] private UnityEngine.UI.Text countdownText; // 3, 2, 1, Go! 표시용
    
    // State
    private bool isPlaying = false;
    private int currentBeat = 0;
    private float beatInterval = 0.5f; // seconds per beat
    private Coroutine metronomeCoroutine;
    
    // Events
    public System.Action<int> OnBeat; // 박자마다 호출 (1, 2, 3, 4)
    public System.Action OnDownbeat; // 강박(첫 박자)마다 호출
    public System.Action OnGameStart; // 카운트다운 후 게임 시작
    
    private void Start()
    {
        // AudioSource 설정
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;
        
        // BPM을 초 간격으로 변환
        UpdateBeatInterval();
        
        Debug.Log($"🥁 메트로놈 초기화 완료 - BPM: {bpm}, 박자: {beatsPerMeasure}/4");
    }
    
    private void UpdateBeatInterval()
    {
        beatInterval = 60f / bpm; // BPM을 초 단위로 변환
    }
    
    public void SetBPM(int newBPM)
    {
        bpm = Mathf.Clamp(newBPM, 60, 200); // 60-200 BPM 범위
        UpdateBeatInterval();
        Debug.Log($"🥁 BPM 변경: {bpm}");
    }
    
    public void SetTimeSignature(int beats)
    {
        beatsPerMeasure = Mathf.Clamp(beats, 2, 8); // 2/4 ~ 8/4 박자
        Debug.Log($"🥁 박자 변경: {beatsPerMeasure}/4");
    }
    
    public void StartMetronome()
    {
        if (isPlaying) return;
        
        isPlaying = true;
        currentBeat = 0;
        
        Debug.Log($"🥁 메트로놈 시작 - BPM: {bpm}");
        
        metronomeCoroutine = StartCoroutine(MetronomeCoroutine());
    }
    
    public void StopMetronome()
    {
        isPlaying = false;
        
        if (metronomeCoroutine != null)
        {
            StopCoroutine(metronomeCoroutine);
            metronomeCoroutine = null;
        }
        
        Debug.Log("🥁 메트로놈 정지");
    }
    
    public void StartWithCountdown()
    {
        if (isPlaying) return;
        
        StartCoroutine(CountdownCoroutine());
    }
    
    private IEnumerator CountdownCoroutine()
    {
        Debug.Log("🥁 카운트다운 시작");
        
        // 3, 2, 1, Go! 카운트다운
        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();
                
            // 카운트다운 소리 (높은 톤)
            if (tockSound != null)
                audioSource.PlayOneShot(tockSound);
                
            Debug.Log($"🥁 카운트다운: {i}");
            
            yield return new WaitForSeconds(beatInterval);
        }
        
        // "Go!" 표시
        if (countdownText != null)
            countdownText.text = "Go!";
            
        // 시작 소리 (더 높은 톤)
        if (tockSound != null)
            audioSource.PlayOneShot(tockSound, 1.2f);
            
        Debug.Log("🥁 게임 시작!");
        
        yield return new WaitForSeconds(beatInterval * 0.5f);
        
        // 카운트다운 텍스트 숨기기
        if (countdownText != null)
            countdownText.text = "";
            
        // 게임 시작 이벤트 호출
        OnGameStart?.Invoke();
        
        // 메트로놈 시작
        StartMetronome();
    }
    
    private IEnumerator MetronomeCoroutine()
    {
        while (isPlaying)
        {
            currentBeat++;
            
            // 박자 순환 (1, 2, 3, 4, 1, 2, 3, 4...)
            if (currentBeat > beatsPerMeasure)
                currentBeat = 1;
            
            // 소리 재생
            PlayBeatSound();
            
            // 이벤트 호출
            OnBeat?.Invoke(currentBeat);
            
            if (currentBeat == 1) // 강박 (첫 박자)
                OnDownbeat?.Invoke();
            
            Debug.Log($"🥁 박자: {currentBeat}/{beatsPerMeasure}");
            
            yield return new WaitForSeconds(beatInterval);
        }
    }
    
    private void PlayBeatSound()
    {
        if (currentBeat == 1)
        {
            // 강박 - 높은 소리
            if (tockSound != null)
                audioSource.PlayOneShot(tockSound);
            else if (tickSound != null)
                audioSource.PlayOneShot(tickSound, 1.2f);
        }
        else
        {
            // 약박 - 낮은 소리
            if (tickSound != null)
                audioSource.PlayOneShot(tickSound);
        }
    }
    
    // 외부에서 호출할 수 있는 메서드들
    public bool IsPlaying => isPlaying;
    public int CurrentBeat => currentBeat;
    public float BeatInterval => beatInterval;
    public int BPM => bpm;
    
    // 디버그용 메서드들
    [ContextMenu("테스트: 메트로놈 시작")]
    public void TestStart()
    {
        StartMetronome();
    }
    
    [ContextMenu("테스트: 메트로놈 정지")]
    public void TestStop()
    {
        StopMetronome();
    }
    
    [ContextMenu("테스트: 카운트다운 시작")]
    public void TestCountdown()
    {
        StartWithCountdown();
    }
    
    [ContextMenu("테스트: BPM 변경 (빠르게)")]
    public void TestFastBPM()
    {
        SetBPM(160);
    }
    
    [ContextMenu("테스트: BPM 변경 (느리게)")]
    public void TestSlowBPM()
    {
        SetBPM(80);
    }
}
