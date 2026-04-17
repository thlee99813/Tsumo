using UnityEngine;

public class TempStorageDropZone : MonoBehaviour
{
    [SerializeField] private TempStorageController _tempStorageController;
    public TempStorageController TempStorageController => _tempStorageController;
}
