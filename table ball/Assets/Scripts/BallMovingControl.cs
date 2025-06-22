using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PoolBall : MonoBehaviour
{
    [Header("Physics Settings")]
    public float minVelocity = 0.005f; // 最小速度，低于此值球将停止
    
    [Header("Audio")]
    public AudioClip collisionSound;
    public AudioClip railCollisionSound; // 新增：边框碰撞音效
    public float minCollisionVolumeSpeed = 0.5f;

    private Rigidbody rb;
    private AudioSource audioSource;
    private Vector3 inputForce; // 用于存储输入力
    private PhysicMaterial physicMaterial; // 将物理材质定义为成员变量

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        SphereCollider collider = GetComponent<SphereCollider>();
        physicMaterial = new PhysicMaterial
        {
            bounciness = 0.9f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            dynamicFriction = 0.1f,
            staticFriction = 0.1f,
            frictionCombine = PhysicMaterialCombine.Multiply
        };
        collider.material = physicMaterial;
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        inputForce = new Vector3(horizontal, 0, vertical);
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(inputForce.x) > 0.01f || Mathf.Abs(inputForce.z) > 0.01f)
        {
            rb.AddForce(inputForce * Time.fixedDeltaTime, ForceMode.Force);
        }
        
        CheckStopCondition();

        if (rb.velocity.magnitude > minVelocity)
        {
            float radius = GetComponent<SphereCollider>().radius * transform.localScale.x;
            float angularSpeed = rb.velocity.magnitude / radius;
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, rb.velocity.normalized);
            Quaternion deltaRotation = Quaternion.Euler(rotationAxis * angularSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
            Debug.Log(rb.rotation);
        }
    }

    private void CheckStopCondition()
{
    // 改为渐进停止，避免突变
    if (rb.velocity.magnitude < minVelocity * 2) 
    {
        rb.velocity *= 0.9f; // 线性衰减
        rb.angularVelocity *= 0.9f;
    }
    if (rb.velocity.magnitude < minVelocity) 
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}

    private void OnCollisionEnter(Collision collision)
    {
        // 通用碰撞音效
        PlayCollisionSound(collisionSound, rb.velocity.magnitude);
        
        // 检测与边框的碰撞
        if (collision.gameObject.CompareTag("Rail")) // 假设边框有"Rail"标签
        {
            HandleRailCollision(collision);
            PlayCollisionSound(railCollisionSound, rb.velocity.magnitude);
        }
        
        // 检测与其他球的碰撞
        PoolBall otherBall = collision.gameObject.GetComponent<PoolBall>();
        if (otherBall != null)
        {
            HandleBallCollision(collision, otherBall);
        }
    }

    private void PlayCollisionSound(AudioClip clip, float speed)
    {
        if (clip != null && speed > minCollisionVolumeSpeed)
        {
            float volume = Mathf.Clamp01(speed / 10f);
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void HandleRailCollision(Collision collision)
    {
        // if (collision.contactCount == 0) return;
        
        // ContactPoint contact = collision.contacts[0];
        // Vector3 normal = contact.normal;
        
        // // 参数配置
        // float railBounceFactor = 0.7f; // 根据手感调整
        // float minBounceSpeed = 0.1f;   // 最小反弹速度阈值
        
        // // 计算反弹方向
        // Vector3 reflectedDir = Vector3.Reflect(rb.velocity.normalized, normal);
        
        // // 动态衰减反弹强度
        // float speed = rb.velocity.magnitude;
        // float bounceStrength = Mathf.Clamp01(speed * 0.2f) * railBounceFactor;
        
        // // 应用新速度
        // Vector3 newVelocity = reflectedDir * speed * bounceStrength;
        
        // // 防止微小反弹
        // if (newVelocity.magnitude > minBounceSpeed)
        // {
        //     rb.velocity = newVelocity;
        // }
        // else
        // {
        //     rb.velocity = Vector3.zero;
        // }
        
        // Debug.Log($"碰撞处理: 原始速度={speed}, 反弹速度={rb.velocity.magnitude}");
    }

    private void HandleBallCollision(Collision collision, PoolBall otherBall)
    {
   
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;
        Vector3 relativeVelocity = rb.velocity - otherBall.rb.velocity;
        float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);
        
        if (velocityAlongNormal > 0) return;

        float impulse = -(1 + physicMaterial.bounciness) * velocityAlongNormal;
        impulse /= (1 / rb.mass + 1 / otherBall.rb.mass);
        Vector3 impulseVector = normal * impulse;
        
        rb.AddForce(impulseVector, ForceMode.Impulse);
        otherBall.rb.AddForce(-impulseVector, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pocket"))
        {
            HandleBallPocketed(); // 处理进洞逻辑
        }
    }

    private void HandleBallPocketed()
    {
        // 1. 禁用球的物理和渲染
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // // 2. 触发得分/游戏逻辑
        // GameManager.Instance.BallPocketed(this); // 假设有一个游戏管理器

        // // 3. 可选：播放音效
        // AudioSource.PlayClipAtPoint(pocketSound, transform.position);
    }

    public void HitBall(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }
}