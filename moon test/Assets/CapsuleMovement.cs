using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CapsuleMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    [Header("摄像机跟随设置")]
    [Tooltip("摄像机在世界坐标下的偏移 (例如：Y向上6，Z向后-8)")]
    public Vector3 cameraOffset = new Vector3(0f, 6f, -8f);
    [Tooltip("摄像机跟随的平滑速度")]
    public float cameraSmoothSpeed = 10f;
    [Tooltip("摄像机看的偏移高度")]
    public Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);

    private CharacterController controller;
    private Camera mainCam;
    private Vector3 velocity;

    public float PutTime = 0.1f;
    public VirtualJoystick joystick; // 在 Inspector 里把 JoystickArea 拖进来

    private Warehouse currentWarehouse;
    
    [SerializeField]
    private Warehouse packedWarehouse;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
        
        if (mainCam != null)
        {
            // 初始化摄像机位置 (注意这里直接加，不用TransformPoint了)
            mainCam.transform.position = transform.position + cameraOffset;
            mainCam.transform.LookAt(transform.position + lookAtOffset);
        }
    }

    float timer = 0.0f;
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > PutTime && currentWarehouse != null)
        { 
            timer = 0;
            GetOrPutResource();
        }

        Move();
    }

    void Move()
    {
            
        float horizontal = joystick != null ? joystick.Direction.x : Input.GetAxis("Horizontal"); 
        float vertical = joystick != null ? joystick.Direction.y : Input.GetAxis("Vertical"); 

        // ★ 核心修改：直接使用 X 和 Z 轴构建“绝对世界方向”
        // horizontal 控制左右 (X轴)，vertical 控制前后 (Z轴)
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

        if (moveDirection.magnitude >= 0.1f)
        {
            // 角色面朝他正在走的方向
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
             
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // 重力逻辑保持不变
        if (controller.isGrounded)
        {
            velocity.y = -2f; 
        }
        else
        {
            velocity.y += gravity * Time.deltaTime; 
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (mainCam == null) return;

        // ★ 核心修改：摄像机只跟随位置，不再参与角色旋转
        // 直接用角色的世界坐标 + 固定的偏移量
        Vector3 targetPosition = transform.position + cameraOffset;

        // 平滑移动到目标位置
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, targetPosition, cameraSmoothSpeed * Time.deltaTime);

        // 摄像机始终看着角色
        Vector3 lookTarget = transform.position + lookAtOffset;
        mainCam.transform.LookAt(lookTarget);
    }

    void GetOrPutResource()
    {
        var buliding = currentWarehouse.gameObject.GetComponentInParent<BaseBuilding>();
        switch (currentWarehouse.Type)
        {
            case WarehouseType.Input:
                packedWarehouse.ResourceTransfer(currentWarehouse, buliding.InputResources, GameManager.Instance.CharacterWarehouseCurve);
                break;
            case WarehouseType.Output:
                currentWarehouse.ResourceTransfer(packedWarehouse, buliding.OutputResources, GameManager.Instance.WarehouseCharacterCurve);
                break;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var house = other.gameObject.GetComponent<Warehouse>();
        if (house != null)
        {
            currentWarehouse = house;
            timer = 0.0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var house = other.gameObject.GetComponent<Warehouse>();
        if (house != null)
        {
            currentWarehouse = null;
            timer = 0.0f;
        } 
    }
}
