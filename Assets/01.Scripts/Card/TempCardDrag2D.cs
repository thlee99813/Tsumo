using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class TempCardDrag2D : MonoBehaviour
{
    [SerializeField] private TempStorageController _tempStorageController;
    [SerializeField] private DeckController _deckController;
    [SerializeField] private int _tempIndex;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Collider2D _cardCollider;
    [SerializeField] private LayerMask _dragStartMask = ~0;
    [SerializeField] private LayerMask _dropCheckMask = ~0;

    private Vector3 _homePosition;
    private Vector3 _dragOffset;
    private bool _isDragging;

    private void Awake()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_cardCollider == null) _cardCollider = GetComponent<Collider2D>();
        _homePosition = transform.position;
    }

    private void OnEnable()
    {
        _homePosition = transform.position;
        _isDragging = false;
    }

    private void Update()
    {
        if (Mouse.current == null || _mainCamera == null) return;

        Vector2 pointer = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame) TryBeginDrag(pointer);
        if (_isDragging && Mouse.current.leftButton.isPressed) Drag(pointer);
        if (_isDragging && Mouse.current.leftButton.wasReleasedThisFrame) EndDrag(pointer);
    }

    private void TryBeginDrag(Vector2 screenPointer)
    {
        if (_tempStorageController == null || !_tempStorageController.HasCardAt(_tempIndex))
        {
            return;
        }

        Vector2 worldPoint = ScreenToWorld2D(screenPointer);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint, _dragStartMask);

        bool hitSelf = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == _cardCollider || hits[i].transform.IsChildOf(transform))
            {
                hitSelf = true;
                break;
            }
        }

        if (!hitSelf) return;

        _homePosition = transform.position;
        Vector3 pointerWorld = ScreenToWorld3D(screenPointer, _homePosition.z);
        _dragOffset = transform.position - pointerWorld;
        _isDragging = true;
    }

    private void Drag(Vector2 screenPointer)
    {
        Vector3 pointerWorld = ScreenToWorld3D(screenPointer, _homePosition.z);
        transform.position = pointerWorld + _dragOffset;
    }

    private void EndDrag(Vector2 screenPointer)
    {
        _isDragging = false;

        bool movedToSquad = false;
        bool droppedOnTempZone = false;

        Vector2 worldPoint = ScreenToWorld2D(screenPointer);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint, _dropCheckMask);

        for (int i = 0; i < hits.Length; i++)
        {
            SquadDropZone squadZone = hits[i].GetComponentInParent<SquadDropZone>();
            if (squadZone != null)
            {
                movedToSquad = _tempStorageController.TryRegisterCardToSquad(_tempIndex, squadZone);
                break;
            }

            TempStorageDropZone tempZone = hits[i].GetComponentInParent<TempStorageDropZone>();
            if (tempZone != null && tempZone.TempStorageController == _tempStorageController)
            {
                droppedOnTempZone = true;
            }
        }

        if (!movedToSquad && !droppedOnTempZone)
        {
            _tempStorageController.TryDiscardCard(_tempIndex, _deckController);
        }

        transform.position = _homePosition;
    }


    private Vector2 ScreenToWorld2D(Vector2 screenPoint)
    {
        float distance = Mathf.Abs(transform.position.z - _mainCamera.transform.position.z);
        Vector3 world = _mainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distance));
        return new Vector2(world.x, world.y);
    }

    private Vector3 ScreenToWorld3D(Vector2 screenPoint, float worldZ)
    {
        float distance = Mathf.Abs(worldZ - _mainCamera.transform.position.z);
        Vector3 world = _mainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distance));
        world.z = worldZ;
        return world;
    }
}
