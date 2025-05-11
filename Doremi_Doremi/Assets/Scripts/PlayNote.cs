using UnityEngine;  // Unity 엔진의 핵심 클래스 및 메서드를 제공하는 네임스페이스

/// <summary>
/// PlayNote 클래스는 버튼 등의 UI 이벤트로 호출되어 지정된 음을 재생하는 기능을 제공하는 컴포넌트입니다.
/// </summary>
public class PlayNote : MonoBehaviour
{
    [Header("🔊 Audio Source")]
    public AudioSource audioSource;  // 음원을 재생할 AudioSource 컴포넌트를 인스펙터에서 연결

    // ********** 각 음을 재생하는 공개 메서드 **********
    // UI 버튼에 연결하여 호출하면, 해당 음의 이름을 Play 메서드로 전달합니다.
    public void PlayC4() => Play("C4");   // C4 음 재생
    public void PlayC4s() => Play("C#4");  // C#4 음 재생
    public void PlayD4() => Play("D4");   // D4 음 재생
    public void PlayD4s() => Play("D#4");  // D#4 음 재생
    public void PlayE4() => Play("E4");   // E4 음 재생
    public void PlayF4() => Play("F4");   // F4 음 재생
    public void PlayF4s() => Play("F#4");  // F#4 음 재생
    public void PlayG4() => Play("G4");   // G4 음 재생
    public void PlayG4s() => Play("G#4");  // G#4 음 재생
    public void PlayA4() => Play("A4");   // A4 음 재생
    public void PlayA4s() => Play("A#4");  // A#4 음 재생
    public void PlayB4() => Play("B4");   // B4 음 재생
    public void PlayC5() => Play("C5");   // C5 음 재생

    /// <summary>
    /// 컴포넌트가 활성화된 후 최초에 한 번 실행되는 콜백.
    /// PlayNote 스크립트가 제대로 동작 중임을 로그로 확인합니다.
    /// </summary>
    void Start()
    {
        Debug.Log("▶ PlayNote 스크립트가 실행됨");  // 시작 알림 로그
    }

    /// <summary>
    /// 전달된 음 이름(noteName)에 따라 AudioSource를 통해 음원을 재생합니다.
    /// AudioSource가 연결되지 않은 경우 경고 메시지를 출력합니다.
    /// </summary>
    /// <param name="noteName">재생할 음의 이름 (예: "C4", "D#4")</param>
    private void Play(string noteName)
    {
        if (audioSource != null)
        {
            Debug.Log($"✅ {noteName} 눌림");  // 어떤 음이 눌렸는지 로그
            audioSource.Play();              // AudioSource로 음원 재생
        }
        else
        {
            Debug.LogWarning($"🔇 {noteName} - AudioSource가 연결되지 않았어요!");  // AudioSource 미연결 경고
        }
    }
}
