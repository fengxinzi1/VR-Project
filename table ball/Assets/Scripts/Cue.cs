using UnityEngine;
using UnityEngine.UI;

public class CueController : MonoBehaviour
{   
    [Header("UI 引用")]
    public Image chargeFillImage;
    public Transform cueTipVisual;
    public LineRenderer cueToBallLine;

    [Header("References")]
    public Transform cue;


    public float maxHitForce = 60f;

    // public Vector3 localLongitudinalAxis = new Vector3(1, 1, 1); // 球杆的局部纵向轴，默认为X轴
    [Header("蓄力设置")]
    public float maxChargeTime = 2f;
    public float minHitForce = 10f;
    public AnimationCurve chargeCurve;
    [Header("击球设置")]
    public float strikeDistance=7f ; // 前推距离
    public float strikeSpeed = 100f; // 前推速度
    public float strikeReturnSpeed = 10f; // 回位速度
    [Header("组件引用")]
    public Transform cueBall;
    public Camera fpCamera;
    [Header("XR 控制器")]
    public Transform controllerTransform;
    public float pitchSpeed = 60f; // 翘杆旋转速度（可在 Inspector 中调节）
    public float maxPitchAngle = 180f; // 最大上下角度限制（可调）

    private float pitchAngle = 0f;
    private Quaternion extraRotation = Quaternion.identity;
    private Rigidbody cueRb;
    private Rigidbody cueBallRb;

    private bool isHoldingCue = true;
    private bool isGrounded;
    
    // 蓄力相关变量
    private bool isCharging = false;
    private float chargeStartTime;
    private float currentChargeTime;
    private float currentHitForce;
    private Vector3 cueOriginalLocalPos;
    private float maxPullBackDistance = 3f;
    
    // 击球相关变量
    private bool isStriking = false;
    private bool isStrikingForward = false;
    private bool hasStruck = false;
    private Vector3 strikeDirection;
    private Vector3 strikeStartPos;
    private Vector3 cueTipPos;

    void Start()
    {
        cueOriginalLocalPos = transform.localPosition;

        Collider col = GetComponent<Collider>();
        
        cueRb = GetComponent<Rigidbody>();
        if (cueRb == null)
        {
            Debug.LogError($"Rigidbody missing! Object: {gameObject.name}");
            enabled = false;
            return;
        }
        
        if (cueBall != null) cueBallRb = cueBall.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        
        if (col != null)
        {
            PhysicMaterial mat = new PhysicMaterial { dynamicFriction = 0.3f };
            col.material = mat;
        }
        else
        {
            Debug.LogWarning("Collider component missing!");
        }
        if (chargeCurve == null || chargeCurve.length == 0)
        {
            chargeCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 1f));
        }
    }
    void Update()
    {
        if (!isHoldingCue || cueRb == null) return;

        if (controllerTransform != null && !isCharging && !isStriking)
        {
            transform.position = controllerTransform.position;

            transform.rotation = controllerTransform.rotation;
        }


        HandleCharge();
        HandleStrike();
        UpdateUI();
        // HandlePitchInput();

    }
    void UpdateUI()
    {
        // 更新蓄力 UI
        if (isCharging && chargeFillImage != null)
        {
            float chargeRatio = Mathf.Clamp01(currentChargeTime / maxChargeTime);
            float evaluated = chargeCurve.Evaluate(chargeRatio);
            chargeFillImage.fillAmount = evaluated;
            chargeFillImage.color = Color.Lerp(Color.green, Color.red, evaluated);
        }
        else if (chargeFillImage != null)
        {
            chargeFillImage.fillAmount = 0f;
        }

        // 更新杆头位置球心连线
        // if (cueToBallLine != null && cueBall != null)
        // {
        //     cueToBallLine.SetPosition(0, cue.position);
        //     cueToBallLine.SetPosition(1, cueBall.position);
        // }

        // 显示 cueTipVisual
        if (cueTipVisual != null)
        {
            cueTipPos = cue.position - cue.right * (cue.localScale.x / 2f);
            cueTipVisual.position = cueTipPos;
        }
    }



    void HandleCharge()
{
    
    // if (controllerTransform != null)
    // {
    //     transform.rotation = controllerTransform.rotation;
    // }
    
    if (Input.GetKeyDown(KeyCode.Space))
    {
        isCharging = true;
        chargeStartTime = Time.time;
        currentChargeTime = 0f;
        
        // 保存蓄力开始时的位置（作为回位目标）
        strikeStartPos = transform.localPosition;
        
        // 初始后拉
        transform.localPosition = strikeStartPos + cue.right * 0.5f;
        
    }

    if (isCharging)
    {
        currentChargeTime = Time.time - chargeStartTime;
        float chargeRatio = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        currentHitForce = Mathf.Lerp(minHitForce, maxHitForce, chargeCurve.Evaluate(chargeRatio));
        
        // 持续后拉（相对于蓄力开始位置）
        float pullDistance = Mathf.Lerp(0.1f, maxPullBackDistance, chargeRatio);
        transform.localPosition = strikeStartPos + cue.right * pullDistance;
        
    }

        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            isCharging = false;
            strikeDirection = -cue.right;
            isStriking = true;
            isStrikingForward = true;
            hasStruck = false;
            // strikeDistance = Vector3.Distance(cueTip.position, cueBall.position);
        }
}

    void HandleStrike()
    {
        
        // if (controllerTransform != null)
        // {
        //     transform.rotation = controllerTransform.rotation;
        // }
        
        if (isStrikingForward)
        {
            // 计算前推目标位置（基于当前杆头位置）
            Vector3 strikeTargetPos = strikeStartPos + strikeDirection * strikeDistance;

            // 向前推动球杆
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                strikeTargetPos,
                strikeSpeed * Time.deltaTime
            );

            // 在接近最大前推位置时执行击球
            float ballRadius = cueBall.GetComponent<SphereCollider>().radius * cueBall.localScale.x;
            cueTipPos = cue.position - cue.right * (cue.localScale.x / 2f);

            float cueToBallDist = Vector3.Distance(cueTipPos, cueBall.position) ;
            // Debug.Log($"Cue to Ball Distance: {cueToBallDist}");
            // Debug.Log($"Cue Tip Position: {cueTipPos}");
            // Debug.Log($"Cue Ball Position: {cueBall.position}");
            // Debug.Log(cueBall.GetComponent<SphereCollider>().radius);
            // Debug.Log(cue.localScale.x);
            // Debug.Log(cueLength);
            // Debug.Log(cue.position);
            if (cueToBallDist < 4.81f && !hasStruck && cueToBallDist > 4.8f)  
            {
                // Debug.Log("Executing hit");
                ExecuteHit();
                hasStruck = true;
            }

            // 到达目标位置后转为回位阶段
            if (Vector3.Distance(transform.localPosition, strikeTargetPos) < 0.01f)
            {
                isStrikingForward = false;
            }
        }
        else if (isStriking) // 回位阶段
        {
            // 回到蓄力开始时的位置
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                strikeStartPos,
                strikeReturnSpeed * Time.deltaTime
            );

            // 完全回到原位后结束击球状态
            if (Vector3.Distance(transform.localPosition, strikeStartPos) < 0.01f)
            {
                isStriking = false;
            }
        }
    }
    void ExecuteHit()
    {
        
        if (Physics.Raycast(cue.position, strikeDirection, out RaycastHit hit, 20.0f))
        {
            // Debug.Log($"Hit object: {hit.collider.gameObject.name}, Tag: {hit.collider.tag}");
            if (hit.collider.CompareTag("CueBall"))
            {
                // Debug.Log("??");
                ApplyHitForce(hit.point);
                isStrikingForward = false;
            }
        }
    }
    void ApplyHitForce(Vector3 hitPoint)
    {
        Vector3 forceDirection = (cueBall.position - cueTipPos).normalized;
        float distanceFactor = Mathf.Clamp(1 - Vector3.Distance(cueTipPos, cueBall.position) / 2f, 0.8f, 1f);
        float finalForce = currentHitForce * distanceFactor;
        
        
        if (cueBallRb != null)
        {
            cueBallRb.AddForce(forceDirection * finalForce, ForceMode.Impulse);
            // Debug.Log($"击球力度: {finalForce}, 方向: {forceDirection}");
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("CueBall"))
        {
            Vector3 hitForce = collision.impulse / Time.fixedDeltaTime;
            const float minForce = 0.1f;
            const float maxForce = 10f;
            if (!(hitForce.magnitude > minForce)) return;
            if (hitForce.magnitude > maxForce)
            {
                hitForce = hitForce.normalized * maxForce;
            }
            if (cueBallRb != null) cueBallRb.AddForce(hitForce, ForceMode.Impulse);
        }
    }
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     if (fpCamera != null)
    //     {
    //         Gizmos.DrawRay(fpCamera.transform.position, fpCamera.transform.forward * 2f);
    //     }
        
    //     // 绘制击球方向
    //     if (isStriking)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawRay(transform.position, strikeDirection * strikeDistance);
    //     }

    //     // 绘制 localLongitudinalAxis 方向
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawRay(transform.position, transform.TransformDirection(cue.right) * 2f);
    // }
}