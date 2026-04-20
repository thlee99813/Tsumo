using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string gameSceneName;
    [SerializeField] private string TutorialSceneName;

    public void OnClickGameStart()
    {
        // 씬 이름 비어있으면 빌드 인덱스 다음 씬으로 진행함
        if (!string.IsNullOrWhiteSpace(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        int nextSceneBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
            return;
        }

        Debug.LogWarning("시작할 게임 씬을 찾지 못함");
    }

    public void OnClickTutorialStart()
    {
        SceneManager.LoadScene(TutorialSceneName);
    }

    public void OnClickGameQuit()
    {
#if UNITY_EDITOR
        // 에디터 재생 중엔 플레이 모드 종료로 동작 맞춤
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
