using Unity.Cinemachine;
using UnityEngine;

public class BattleImpulseEmitter : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private float _force = 1f;
    [SerializeField] private Vector3 _impulseVelocity = new Vector3(1f, 0f, 0f);

    public void EmitHitImpulse()
    {
        _impulseSource.GenerateImpulse(_impulseVelocity * _force);
    }
}

