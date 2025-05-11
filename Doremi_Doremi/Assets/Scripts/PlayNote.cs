using UnityEngine;  // Unity 엔진의 핵심 API 사용

// 다양한 음표 재생 메서드를 제공하는 컴포넌트
public class PlayNote : MonoBehaviour
{
    // 🎧 오디오 재생을 담당할 AudioSource 컴포넌트 참조
    public AudioSource audioSource;

    // ----------------------------------------------------------
    // 📌 인스펙터나 다른 스크립트에서 호출 가능한 음표별 재생 메서드
    //    C4 재생
    public void PlayC4() => Play("C4");
    // C#4 재생
    public void PlayC4s() => Play("C#4");
    // D4 재생
    public void PlayD4() => Play("D4");
    // D#4 재생
    public void PlayD4s() => Play("D#4");
    // E4 재생
    public void PlayE4() => Play("E4");
    // F4 재생
    public void PlayF4() => Play("F4");
    // F#4 재생
    public void PlayF4s() => Play("F#4");
    // G4 재생
    public void PlayG4() => Play("G4");
    // G#4 재생
    public void PlayG4s() => Play("G#4");
    // A4 재생
    public void PlayA4() => Play("A4");
    // A#4 재생
    public void PlayA4s() => Play("A#4");
    // B4 재생
    public void PlayB4() => Play("B4");
    // C5 재생
    public void PlayC5() => Play("C5");

    // ----------------------------------------------------------
    // Unity 플레이 시작 시 한 번 실행되어 스크립트 활성화 확인용 로그 출력
    void Start()
    {
        Debug.Log("▶ PlayNote 스크립트가 실행됨");
    }

    // ----------------------------------------------------------
    // 🛠 내부 음표 재생 로직
    // noteName에 해당하는 음표를 재생하고, 디버그 로그를 남김
    private void Play(string noteName)
    {
        if (audioSource != null)  // AudioSource가 할당되어 있으면
        {
            Debug.Log($"✅ {noteName} 눌림");  // 재생 로그
            audioSource.Play();  // AudioSource로 오디오 재생
        }
        else  // AudioSource가 null인 경우
        {
            Debug.LogWarning($"🔇 {noteName} - AudioSource가 연결되지 않았어요!");  // 경고 로그
        }
    }
}