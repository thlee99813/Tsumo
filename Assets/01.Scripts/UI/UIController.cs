using UnityEngine;
using TMPro;
using System;


public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text _rerollCountText;
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _restartStageText;

    [Header("Panels")]
    [SerializeField] private GameObject _gameOverImage;

    public event Action OnRestartClicked;

    public void SetRerollCount(int remainingCount, int maxCount)
    {
        if (_rerollCountText == null)
        {
            return;
        }

        int clampedRemaining = Mathf.Clamp(remainingCount, 0, maxCount);
        _rerollCountText.text = $"{clampedRemaining}";
    }
    public void SetStageText(string stageCode)
    {
        if (_stageText == null)
        {
            return;
        }

        _stageText.text = $"{stageCode}";
    }

    public void ShowGameOver(string restartStageCode)
    {
        if (_restartStageText != null)
        {
            _restartStageText.text = $"Restart : {restartStageCode}";
        }

        if (_gameOverImage != null)
        {
            _gameOverImage.SetActive(true);
        }
    }

    public void HideGameOver()
    {
        if (_gameOverImage != null)
        {
            _gameOverImage.SetActive(false);
        }
    }

    

    public void OnClickRestartButton()
    {
        OnRestartClicked?.Invoke();
    }



}
