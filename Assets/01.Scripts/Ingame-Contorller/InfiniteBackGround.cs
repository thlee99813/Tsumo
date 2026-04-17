using UnityEngine;

public class InfiniteBackGround : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 2f;
    [SerializeField] private Transform[] _backGround;   // 배경 연결
    private float _bgWidth;
    private Camera _cam;

    private void Awake()
    {
        //SpriteRenderer 기준으로 너비 계산
        _bgWidth = _backGround[0].GetComponent<SpriteRenderer>().bounds.size.x;
        _cam = Camera.main;
    }

    private void Update()
    {
        // 카메라 왼쪽 끝 월드 좌표
        float camLeftEdge = _cam.transform.position.x - _cam.orthographicSize * _cam.aspect;

        //전체 배경 왼쪽으로 이동
        foreach(Transform bg in _backGround)
        {
            bg.Translate(Vector3.left * _scrollSpeed * Time.deltaTime);

            // 배경의 오른쪽 끝이 카메라 왼쪽 끝을 벗어나면 오른쪽으로 재배치
            if(bg.position.x + _bgWidth * 0.5f < camLeftEdge)
            {
                Reposition(bg);
            }
        }
    }

    private void Reposition(Transform bg)
    {
        bg.position = new Vector3(
            bg.position.x + _bgWidth * _backGround.Length,
            bg.position.y,
            bg.position.z
        );
    }

    public void SetSpeed(float speed)
    {
        _scrollSpeed = speed;
    }
}
