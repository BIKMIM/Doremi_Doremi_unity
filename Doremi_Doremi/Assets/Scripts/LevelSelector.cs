using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public int difficultyLevel; // 0: 쉬움, 1: 중간, ...

    public void OnLevelSelected()
    {
        PlayerPrefs.SetInt("SelectedLevel", difficultyLevel); // 다음 씬에서도 사용할 수 있게 저장
        // 다음 화면(예: 연습 화면)으로 전환
        SceneManager.LoadScene("PracticeScene");
    }
}
