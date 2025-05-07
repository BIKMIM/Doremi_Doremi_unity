using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public int difficultyLevel; // 0: ����, 1: �߰�, ...

    public void OnLevelSelected()
    {
        PlayerPrefs.SetInt("SelectedLevel", difficultyLevel); // ���� �������� ����� �� �ְ� ����
        // ���� ȭ��(��: ���� ȭ��)���� ��ȯ
        SceneManager.LoadScene("PracticeScene");
    }
}
