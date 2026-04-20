using Unity.Cinemachine;
using UnityEngine;

public class BattleImpulseEmitter : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private OverlayCameraShakeSync _overlayCameraShakeSync;
    [SerializeField] private float _force = 1f;
    [SerializeField] private Vector3 _impulseVelocity = new Vector3(1f, 0f, 0f);
    [SerializeField] private float _overlaySyncSeconds = 0.18f;

    public void EmitHitImpulse(bool shakeOverlayToo = false)
    {
        if (shakeOverlayToo)
        {
            _overlayCameraShakeSync.BeginSyncForSeconds(_overlaySyncSeconds);
        }
        _impulseSource.GenerateImpulse(_impulseVelocity * _force);
    }
    public void EmitHitImpulseAll(bool shakeOverlayToo = false)
    {
        if (shakeOverlayToo)
        {
            _overlayCameraShakeSync.BeginSyncForSeconds(_overlaySyncSeconds);
        }
        _impulseSource.GenerateImpulse(_force);
    }
}

