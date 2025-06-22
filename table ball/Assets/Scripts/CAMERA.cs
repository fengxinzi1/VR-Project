using UnityEngine;

public class PoolCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target; // 要跟随的桌球
    public float distance = 5f; // 相机与目标的距离
    public float height = 2f; // 相机相对于目标的高度
    public float smoothSpeed = 5f; // 跟随平滑度

    [Header("Rotation Settings")]
    public float rotationSpeed = 3f; // 旋转速度
    public float minVerticalAngle = 10f; // 最小俯仰角度
    public float maxVerticalAngle = 80f; // 最大俯仰角度

    [Header("Edge Avoidance")]
    public bool avoidObstacles = true;
    public LayerMask obstacleMask;
    public float obstacleCheckRadius = 0.5f;

    private Vector3 offsetDirection;
    private float currentXRotation;
    private float currentYRotation;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned to PoolCameraFollow!");
            return;
        }

        // 初始化相机位置和旋转
        offsetDirection = (transform.position - target.position).normalized;
        Vector3 angles = transform.eulerAngles;
        currentXRotation = angles.x;
        currentYRotation = angles.y;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleRotationInput();
        UpdateCameraPosition();
    }

    private void HandleRotationInput()
    {
        // 鼠标控制相机旋转
        if (Input.GetMouseButton(1)) // 右键按住旋转
        {
            currentYRotation += Input.GetAxis("Mouse X") * rotationSpeed;
            currentXRotation -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentXRotation = Mathf.Clamp(currentXRotation, minVerticalAngle, maxVerticalAngle);
        }
    }

    private void UpdateCameraPosition()
    {
        // 计算期望的相机位置
        Quaternion rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);
        Vector3 desiredPosition = target.position + rotation * new Vector3(0, height, -distance);

        // 障碍物检测
        if (avoidObstacles)
        {
            RaycastHit hit;
            Vector3 direction = desiredPosition - target.position;
            if (Physics.SphereCast(target.position, obstacleCheckRadius, direction.normalized, out hit, direction.magnitude, obstacleMask))
            {
                desiredPosition = hit.point - direction.normalized * obstacleCheckRadius;
            }
        }

        // 平滑移动相机
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // 确保相机始终看向目标
        transform.LookAt(target.position + Vector3.up * height * 0.5f); // 稍微向上看一点，避免直接看地面
    }

    // 可选的：重置相机到默认位置
    public void ResetCamera()
    {
        currentXRotation = 45f; // 默认俯仰角度
        currentYRotation = target.eulerAngles.y; // 与目标相同水平旋转
    }
}