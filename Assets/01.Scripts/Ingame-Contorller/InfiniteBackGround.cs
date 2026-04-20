using UnityEngine;

public class InfiniteBackGround : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 2f;
    [SerializeField] private Transform[] _backGround;   // 배경 연결
    [SerializeField] private Transform[] _ground;   // 지면 연결
    [SerializeField] private float _repositionMargin = 0.2f;
    [SerializeField] private float _seamOverlap = 0.02f;

    [SerializeField] private float _bgParallax = 0.6f;
    [SerializeField] private float _groundParallax = 1.0f;

    private float _bgWidth;
    private float _gWidth;
    private Camera _cam;
    private float _speedMultiplier = 1f;


    private void Awake()
    {
        //SpriteRenderer 기준으로 너비 계산
        _bgWidth = _backGround[0].GetComponent<SpriteRenderer>().bounds.size.x;
        _gWidth = _ground[0].GetComponent<SpriteRenderer>().bounds.size.x;
        _cam = Camera.main;
    }

    private void Update()
    {
        // 카메라 왼쪽 끝 월드 좌표
        float camLeftEdge = _cam.transform.position.x - _cam.orthographicSize * _cam.aspect;
        float recycleThreshold = camLeftEdge - _repositionMargin;
        float baseDelta = _scrollSpeed * _speedMultiplier * Time.unscaledDeltaTime;

        //전체 배경 왼쪽으로 이동 (Time.timeScale 비의존 - 언스케일 타임 사용)
        foreach (Transform bg in _backGround)
        {
            bg.Translate(Vector3.left * baseDelta * _bgParallax, Space.World);

            // 배경의 오른쪽 끝이 카메라 왼쪽 끝을 벗어나면 오른쪽으로 재배치
            if (bg.position.x + _bgWidth * 0.5f < recycleThreshold)
            {
                Reposition(bg, _bgWidth, _backGround.Length);
            }
        }

        foreach (Transform g in _ground)
        {
            g.Translate(Vector3.left * baseDelta * _groundParallax, Space.World);

            // 배경의 오른쪽 끝이 카메라 왼쪽 끝을 벗어나면 오른쪽으로 재배치
            if (g.position.x + _gWidth * 0.5f < recycleThreshold)
            {
                Reposition(g, _gWidth, _ground.Length);
            }
        }
    }

    private void Reposition(Transform target, float width, int length)
    {
        float overlap = Mathf.Clamp(_seamOverlap, 0f, width * 0.5f);
        target.position = new Vector3(
            target.position.x + width * length - overlap,
            target.position.y,
            target.position.z
        );
    }

    public void SetSpeed(float speed)
    {
        _scrollSpeed = speed;
    }

    // Time.timeScale 없이 배경 속도 배율 직접 제어
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }
}
