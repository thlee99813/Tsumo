using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OverlayCameraShakeSync : MonoBehaviour
{
    [SerializeField] private Transform _sourceCameraTransform;
    [SerializeField] private Camera _sourceCamera;
    [SerializeField] private bool _syncRotation = true;
    [SerializeField] private bool _syncOrthoSize = true;
    [SerializeField] private bool _syncOnStart = false;

    private Camera _selfCamera;
    private bool _isSyncActive;
    private float _syncUntilTime;

    private void Awake()
    {
        _selfCamera = GetComponent<Camera>();
        _isSyncActive = _syncOnStart;
        _syncUntilTime = _syncOnStart ? float.PositiveInfinity : 0f;
    }

    private void LateUpdate()
    {
        if (!_isSyncActive)
        {
            return;
        }

        SyncNow();

        if (Time.unscaledTime >= _syncUntilTime)
        {
            _isSyncActive = false;
        }
    }

    public void BeginSyncForSeconds(float seconds)
    {
        _isSyncActive = true;
        _syncUntilTime = Time.unscaledTime + Mathf.Max(0f, seconds);
        SyncNow();
    }

    public void SetSyncEnabled(bool enabled)
    {
        _isSyncActive = enabled;
        _syncUntilTime = enabled ? float.PositiveInfinity : 0f;

        if (enabled)
        {
            SyncNow();
        }
    }

    private void SyncNow()
    {
        transform.position = _sourceCameraTransform.position;

        if (_syncRotation)
        {
            transform.rotation = _sourceCameraTransform.rotation;
        }

        if (_syncOrthoSize && _sourceCamera != null && _selfCamera.orthographic)
        {
            _selfCamera.orthographicSize = _sourceCamera.orthographicSize;
        }
    }
}
