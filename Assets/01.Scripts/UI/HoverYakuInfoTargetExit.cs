using UnityEngine;
using UnityEngine.EventSystems;

public class HoverYakuInfoTargetExit : MonoBehaviour, IPointerExitHandler
{
    [SerializeField] private HoverYakuInfo _hoverYakuInfo;

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverYakuInfo == null)
        {
            return;
        }

        _hoverYakuInfo.OnTargetPointerExit();
    }
}
