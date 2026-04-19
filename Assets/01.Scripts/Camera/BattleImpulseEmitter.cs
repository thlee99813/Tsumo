using Unity.Cinemachine;
using UnityEngine;

public class BattleImpulseEmitter : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private float _force = 1f;

    public void EmitHitImpulse()
    {
        _impulseSource.GenerateImpulse(_force);
    }
}

