using UnityEngine;

public class PlayNote : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayC4() => Play("C4");
    public void PlayC4s() => Play("C#4");
    public void PlayD4() => Play("D4");
    public void PlayD4s() => Play("D#4");
    public void PlayE4() => Play("E4");
    public void PlayF4() => Play("F4");
    public void PlayF4s() => Play("F#4");
    public void PlayG4() => Play("G4");
    public void PlayG4s() => Play("G#4");
    public void PlayA4() => Play("A4");
    public void PlayA4s() => Play("A#4");
    public void PlayB4() => Play("B4");
    public void PlayC5() => Play("C5");

    void Start()
    {
        Debug.Log("▶ PlayNote 스크립트가 실행됨");
    }

    private void Play(string noteName)
    {
        if (audioSource != null)
        {
            Debug.Log($"✅ {noteName} 눌림");
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"🔇 {noteName} - AudioSource가 연결되지 않았어요!");
        }
    }
}
