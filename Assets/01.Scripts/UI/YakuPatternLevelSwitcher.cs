using UnityEngine;

public class YakuPatternLevelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject _yakuPatternLevel1;
    [SerializeField] private GameObject _yakuPatternLevel2;
    [SerializeField] private GameObject _yakuPatternLevel3;
    [SerializeField] private int _defaultLevel = 1;

    private void Awake()
    {
        ShowLevel(_defaultLevel);
    }

    public void ShowLevel1()
    {
        ShowLevel(1);
    }

    public void ShowLevel2()
    {
        ShowLevel(2);
    }

    public void ShowLevel3()
    {
        ShowLevel(3);
    }

    public void ShowLevel(int level)
    {
        _yakuPatternLevel1.SetActive(level == 1);
        _yakuPatternLevel2.SetActive(level == 2);
        _yakuPatternLevel3.SetActive(level == 3);
    }
}
