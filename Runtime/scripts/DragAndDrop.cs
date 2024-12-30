using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
     [Header("General Settings")]
    private Camera mainCamera;
    [SerializeField] private float fingerFollowSpeed;
    [SerializeField] private float snapRange;
    [SerializeField] private int objIndex;
    [SerializeField] private SpriteRenderer objectSpriteRenderer;
    public SpriteRenderer ObjectSpriteRenderer
    {
        get => objectSpriteRenderer;
        set => objectSpriteRenderer = value;
    }
    public int ObjIndex
    {
        get => objIndex;
        set => objIndex = value;
    }

    [Header("Dragging Settings")]
    [SerializeField] private List<Transform> targetTransforms = new();
    [SerializeField] private List<Vector2> targetPositions = new();
    [SerializeField] private bool canDrag = true;
    [SerializeField] private bool isSnapped;
    [SerializeField] private int dragLayerIndex;
    [SerializeField] private int snappedLayerIndex;
    [SerializeField] private int startLayerIndex;

    public bool IsSnapped 
    {
        get => isSnapped;
        set => isSnapped = value;
    }
    public bool CanDrag 
    {
        get => canDrag;
        set => canDrag = value;
    }
    public List<Vector2> TargetPositions
    {
        get => targetPositions;
        set => targetPositions = value;
    }
    
    public List<Transform> TargetTransforms
    {
        get => targetTransforms;
        set => targetTransforms = value;
    }

    public int DragLayerIndex
    {
        get => dragLayerIndex;
        set => dragLayerIndex = value;
    }

    public int SnappedLayerIndex
    {
        get => snappedLayerIndex;
        set => snappedLayerIndex = value;
    }

    public int StartLayerIndex
    {
        get => startLayerIndex;
        set => startLayerIndex = value;
    }
    [Header("Visual Feedback")]
    [SerializeField] private float dragScale;
    [SerializeField] private float normalScale;
    [SerializeField] private float tweenAnimationDuration;
    [SerializeField] private float delayBeforeReturnStartPos;
    public float NormalScale
    {
        get => normalScale;
        set => normalScale = value;
    }

    public float DelayBeforeReturnStartPos
    {
        get => delayBeforeReturnStartPos;
        set => delayBeforeReturnStartPos = value;
    }
    
    private Vector3 touchOffset;
    public Vector3 StartPosition { get; set; }

    private bool isDragging;
    private int activeTouchIndex = -1;

    public event Action OnDragStart;
    public event Action OnDragEnd;
    public event Action<Transform, int> OnCorrectSnap;
    public event Action<int> OnIncorrectSnap;

    private void Start()
    {
        mainCamera ??= Camera.main;
        StartPosition = transform.position;
    }

    private void Update()
    {
        // Check if mouse is being used for input
        if (Input.GetMouseButtonDown(0))
        {
            HandleFingerDown(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            HandleFingerMove(Input.mousePosition);
            
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            HandleFingerUp();
        }

        // Handle touch input (mobile)
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleFingerDown(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                HandleFingerMove(touch.position);
                
            }
            else if (touch.phase == TouchPhase.Ended && isDragging)
            {
                HandleFingerUp();
            }
        }
    }

    private void HandleFingerDown(Vector3 touchPosition)
    {
        if (activeTouchIndex == -1 && IsTouchingObject(touchPosition) && canDrag)
        {
            activeTouchIndex = 0;
            touchOffset = transform.position - mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, mainCamera.nearClipPlane));
            OnDragStart?.Invoke();
            objectSpriteRenderer.sortingOrder = dragLayerIndex;
            transform.DOScale(dragScale, tweenAnimationDuration);
            isDragging = true;
        }
    }

    private void HandleFingerMove(Vector3 touchPosition)
    {
        if (!isDragging || activeTouchIndex != 0) return;
    
        var targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, mainCamera.nearClipPlane)) + touchOffset;
        var position = transform.position;
        targetPosition.z = position.z;
        position = Vector2.Lerp(position, targetPosition, fingerFollowSpeed);
        transform.position = position;

        var closestTransform = GetClosestTarget();
        var closestPosition = GetClosestPosition();
        if (closestTransform != null)
        {
            var distanceToClosest = Vector2.Distance(transform.position, closestTransform.position);
            if (distanceToClosest <= snapRange)
            {
                SnapToTarget(closestTransform);
                OnCorrectSnap?.Invoke(closestTransform, objIndex);
                isDragging = false;
                activeTouchIndex = -1;
            }
        } if (closestPosition != null)
        {
             SnapToTarget(closestPosition.Value);
            OnCorrectSnap?.Invoke(null, objIndex);
            isDragging = false;
            activeTouchIndex = -1;

        }
    }

    private void HandleFingerUp()
    {
        if (activeTouchIndex != 0) return;
        activeTouchIndex = -1;
        isDragging = false;
        HandleSnapOrReset();
        OnDragEnd?.Invoke();
        transform.DOScale(normalScale, tweenAnimationDuration);
    }

    private void HandleSnapOrReset()
    {
        var closestTransform = GetClosestTarget();
        var closestPosition = GetClosestPosition();
        
        if (closestTransform != null && 
            (closestPosition == null || 
             Vector2.Distance(transform.position, closestTransform.position) <= 
             Vector2.Distance(transform.position, closestPosition.Value)))
        {
            SnapToTarget(closestTransform);
            OnCorrectSnap?.Invoke(closestTransform, objIndex);
        }
        else if (closestPosition != null)
        {
            SnapToTarget(closestPosition.Value);
            OnCorrectSnap?.Invoke(null, objIndex);
        }
        else
        {
            ResetToStart();
            OnIncorrectSnap?.Invoke(objIndex);
        }
    }

    private Transform GetClosestTarget()
    {
        return targetTransforms
            .Where(target => Vector2.Distance(transform.position, target.position) <= snapRange)
            .OrderBy(target => Vector2.Distance(transform.position, target.position))
            .FirstOrDefault();
    }

    private void SnapToTarget(Transform target)
    {
        transform.DOMove(target.position, tweenAnimationDuration).SetEase(Ease.OutBack).OnComplete(()=>
        {
            transform.DOScale(normalScale, tweenAnimationDuration);
        });;
        objectSpriteRenderer.sortingOrder = snappedLayerIndex;
        isSnapped = true;
        canDrag = false;
    }

    private Vector2? GetClosestPosition()
    {
        var validPositions = targetPositions
            .Where(position => Vector2.Distance(transform.position, position) <= snapRange)
            .OrderBy(position => Vector2.Distance(transform.position, position))
            .ToList();
        return validPositions.Count > 0 ? validPositions[0] : null;
    }

    private void SnapToTarget(Vector2 position)
    {
        transform.DOMove(new Vector3(position.x, position.y, transform.position.z), tweenAnimationDuration).SetEase(Ease.OutBack).OnComplete(()=>
        {
            transform.DOScale(normalScale, tweenAnimationDuration);
        });

        objectSpriteRenderer.sortingOrder = snappedLayerIndex;
        isSnapped = true;
        canDrag = false;
    }

    private void ResetToStart()
    {
        StartCoroutine(ReturnToStartPosition());
    }

    private IEnumerator ReturnToStartPosition()
    {
        yield return new WaitForSecondsRealtime(delayBeforeReturnStartPos);
        transform.DOMove(StartPosition, tweenAnimationDuration).OnComplete(() =>
        { 
            objectSpriteRenderer.sortingOrder = startLayerIndex;
        });
    }

    private bool IsTouchingObject(Vector3 touchPosition)
    {
        var worldTouchPosition = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, mainCamera.nearClipPlane));
        var localScale = transform.localScale;
        var objectRadius = Mathf.Max(localScale.x, localScale.y);
        return Vector2.Distance(worldTouchPosition, transform.position) <= objectRadius;
    }
}