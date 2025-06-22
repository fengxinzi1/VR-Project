using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 targetOffset;
    private float transitionSpeed;
    
    [Header("Settings")]
    public float smoothTime = 0.3f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    
    private Vector3 velocity = Vector3.zero;

    public void SetTarget(Transform newTarget, Vector3 offset, float speed = 5f)
    {
        target = newTarget;
        targetOffset = offset;
        transitionSpeed = speed;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 计算期望位置
        Vector3 desiredPosition = target.position + targetOffset;
        
        // 平滑移动
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity, 
            smoothTime,
            transitionSpeed
        );
        
        // 始终看向目标
        transform.LookAt(target);
    }
}