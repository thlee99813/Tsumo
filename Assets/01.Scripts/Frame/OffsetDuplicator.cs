using UnityEngine;

public class OffsetDuplicator : MonoBehaviour
{
    [Header("Duplicate Settings")]
    [Min(1)] public int copyCount = 5;
    public Vector3 offset = new Vector3(1f, 0f, 0f);
    public Space offsetSpace = Space.Self;
    public bool keepSameParent = true;
    public string nameSuffix = "";
}
