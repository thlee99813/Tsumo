using UnityEngine;
using UnityEngine.EventSystems;

public class HoverYakuInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private GameObject _targetObject;

    private bool _isPinned;

    private void Awake()
    {
        _isPinned = false;
        SetVisible(false);
    }

    private void OnDisable()
    {
        _isPinned = false;
        SetVisible(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isPinned)
        {
            SetVisible(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isPinned)
        {
            SetVisible(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _isPinned = true;
        SetVisible(true);
    }

    public void ClosePinnedInfo()
    {
        _isPinned = false;
        SetVisible(false);
    }

    private void SetVisible(bool isVisible)
    {
        if (_targetObject == null)
        {
            return;
        }

        if (_targetObject.activeSelf == isVisible)
        {
            return;
        }

        _targetObject.SetActive(isVisible);
    }
}
