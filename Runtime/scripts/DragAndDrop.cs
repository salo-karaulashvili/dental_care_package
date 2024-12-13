using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class DragAndDrop : MonoBehaviour
{
    [Header("General Settings")]
    private Camera mainCamera;
    [SerializeField] private float fingerFollowSpeed;
    [SerializeField] private float snapRange;
    [SerializeField] private int objIndex;
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
    public bool IsSnapped => isSnapped;
    public bool CanDrag => canDrag;

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
    private Finger activeFinger;

    public event Action OnDragStart;
    public event Action OnDragEnd;
    public event Action<Transform, int> OnCorrectSnap;
    public event Action<int> OnIncorrectSnap;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += HandleFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += HandleFingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += HandleFingerUp;
    }

    private void OnDisable()
    {
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= HandleFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= HandleFingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= HandleFingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        mainCamera ??= Camera.main;
        StartPosition = transform.position;
    }

    private void HandleFingerDown(Finger touchedFinger)
    {
        if (activeFinger == null && IsTouchingObject(touchedFinger) && canDrag)
        {
            activeFinger = touchedFinger;
            touchOffset = transform.position - mainCamera.ScreenToWorldPoint(new Vector3(touchedFinger.screenPosition.x, touchedFinger.screenPosition.y, mainCamera.nearClipPlane));
            OnDragStart?.Invoke();
            transform.DOScale(dragScale, tweenAnimationDuration);
        }
    }

    private void HandleFingerMove(Finger movedFinger)
    {
        if (movedFinger != activeFinger || !canDrag) return;
        var targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(movedFinger.screenPosition.x, movedFinger.screenPosition.y, mainCamera.nearClipPlane)) + touchOffset;
        var position = transform.position;
        targetPosition.z = position.z;
        position = Vector2.Lerp(position, targetPosition, fingerFollowSpeed);
        transform.position = position;
    }

    private void HandleFingerUp(Finger liftedFinger)
    {
        if (liftedFinger != activeFinger) return;
        activeFinger = null;
        HandleSnapOrReset();
        OnDragEnd?.Invoke();
        transform.DOScale(normalScale, tweenAnimationDuration);
    }

    private void HandleSnapOrReset()
    {
        var closestTransform = GetClosestTarget();
        var closestPosition = GetClosestPosition();
        
        // Check for closest snapping option
        if (closestTransform != null && 
            (closestPosition == null || 
             Vector2.Distance(transform.position, closestTransform.position) <= 
             Vector2.Distance(transform.position, closestPosition.Value)))
        {
            // Snap to closest transform
            SnapToTarget(closestTransform);
            OnCorrectSnap?.Invoke(closestTransform, objIndex);
        }
        else if (closestPosition != null)
        {
            // Snap to closest position
            SnapToTarget(closestPosition.Value);
            OnCorrectSnap?.Invoke(null, objIndex); // Pass null for transform if snapping to position
        }
        else
        {
            // Return to start position
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
        transform.DOMove(target.position, tweenAnimationDuration).SetEase(Ease.OutBack);
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
        transform.DOMove(new Vector3(position.x, position.y, transform.position.z), tweenAnimationDuration).SetEase(Ease.OutBack);
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
        transform.DOMove(StartPosition, tweenAnimationDuration);
        isSnapped = false;
    }

    private bool IsTouchingObject(Finger touchedFinger)
    {
        var worldTouchPosition = mainCamera.ScreenToWorldPoint(new Vector3(touchedFinger.screenPosition.x, touchedFinger.screenPosition.y, mainCamera.nearClipPlane));
        var localScale = transform.localScale;
        var objectRadius = Mathf.Max(localScale.x, localScale.y);
        return Vector2.Distance(worldTouchPosition, transform.position) <= objectRadius;
    }
}