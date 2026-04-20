using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : Singleton<SceneManager>
{
    [SerializeField] private string _lobbySceneName = "LobbyScene";
    [SerializeField] private string _tutorialSceneName = "TutorialScene";
    [SerializeField] private string _mainSceneName = "01.MainScene";

    protected override void Init()
    {
    }

    public void MoveSceneToLobby()
    {
        UnitySceneManager.LoadScene(_lobbySceneName);
    }

    public void MoveSceneToTutorial()
    {
        UnitySceneManager.LoadScene(_tutorialSceneName);
    }

    public void MoveSceneToMain()
    {
        UnitySceneManager.LoadScene(_mainSceneName);
    }
}
