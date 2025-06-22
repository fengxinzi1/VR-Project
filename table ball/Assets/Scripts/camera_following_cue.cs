using UnityEngine;

[RequireComponent(typeof(Camera))]
public class StickViewCamera : MonoBehaviour
{
    [Header("References")]
    public Transform cue;          // 球杆对象
    public Transform whiteBall;    // 白球（瞄准焦点）

    [Header("Camera Position")]
    public Vector3 shoulderOffset = new Vector3(-0.2f, 0.2f, 0); // 肩膀右上方偏移
    public float distanceFromCue = 0f;        // 相机与球杆的默认距离
    public float positionSmoothTime = 0.1f;     // 位置平滑时间

    [Header("Camera Rotation")]
    public float rotationSmoothTime = 0.05f;    // 旋转平滑时间

    [Header("Aiming Mode")]
    public float aimingFOV = 50f;               // 瞄准时的FOV
    public float aimingDistanceMultiplier = 0.7f;// 瞄准时相机靠近球杆的程度

    [Header("Initialization")]
    public bool snapOnStart = true;  // 是否在开始时立即对齐视角

    private Camera cam;
    private bool isAiming = false;
    private Vector3 positionVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;

        if (snapOnStart && cue != null)
        {
            Vector3 shoulderPosition = cue.TransformPoint(shoulderOffset);
            Vector3 cameraDirection = -cue.forward;
            transform.position = shoulderPosition + cameraDirection * distanceFromCue;
            transform.rotation = Quaternion.LookRotation(cue.forward);
        }
    }

    void Update()
    {
        isAiming = Input.GetMouseButton(1);
    }

    void LateUpdate()
    {
        if (cue == null) return;

        Vector3 shoulderPosition = cue.TransformPoint(shoulderOffset);
        Vector3 cameraDirection = -cue.forward;
        
        float currentDistance = isAiming ? 
            distanceFromCue * aimingDistanceMultiplier : 
            distanceFromCue;

        Vector3 targetPosition = shoulderPosition + cameraDirection * currentDistance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        Quaternion targetRotation = Quaternion.LookRotation(-cue.right, cue.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothTime * Time.deltaTime * 20f
        );

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            isAiming ? aimingFOV : 65f,
            Time.deltaTime * 5f
        );
    }
}