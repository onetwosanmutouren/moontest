using UnityEngine;
using UnityEngine.EventSystems;

// 实现这三个接口来处理：按下、拖动、抬起
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI 组件绑定")]
    public RectTransform joystickBase;   // 拖入 JoystickBG
    public RectTransform joystickThumb;  // 拖入 JoystickThumb
    
    [Header("摇杆参数")]
    public float maxRadius = 60f;        // 摇杆把手能偏离中心的最大距离

    // 供外部读取的方向向量 (已经归一化，范围在 -1 到 1 之间)
    public Vector2 Direction { get; private set; }

    // 记录当前正在操作的手指ID，防止多点触控时混乱
    private int currentPointerId = -1;

    void Start()
    {
        // 初始状态下隐藏摇杆
        if (joystickBase != null) joystickBase.gameObject.SetActive(false);
    }

    // 1. 手指按下时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        // 如果已经有其他手指在操作，忽略新的手指
        if (currentPointerId != -1) return;

        currentPointerId = eventData.pointerId;

        // 显示摇杆，并将底座中心移动到手指按下的位置
        joystickBase.gameObject.SetActive(true);
        joystickBase.position = eventData.position;
        
        // 把手回到中心
        joystickThumb.position = eventData.position;
        
        Direction = Vector2.zero;
    }

    // 2. 手指拖动时触发
    public void OnDrag(PointerEventData eventData)
    {
        // 只响应正在操作的那根手指
        if (eventData.pointerId != currentPointerId) return;

        // 将屏幕坐标转换为摇杆底座的局部坐标
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBase, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint);

        // 计算从中心到当前触摸点的向量
        Vector2 offset = localPoint;

        // 限制把手的移动范围在最大半径内 (利用向量长度的限制)
        if (offset.magnitude > maxRadius)
        {
            offset = offset.normalized * maxRadius;
        }

        // 更新把手的位置 (因为把手是底座的子物体，直接用局部坐标偏移即可)
        joystickThumb.localPosition = offset;

        // 计算归一化的方向输出
        Direction = offset / maxRadius;
    }

    // 3. 手指抬起时触发
    public void OnPointerUp(PointerEventData eventData)
    {
        // 只响应正在操作的那根手指抬起
        if (eventData.pointerId != currentPointerId) return;

        currentPointerId = -1;

        // 隐藏摇杆，方向归零
        joystickBase.gameObject.SetActive(false);
        Direction = Vector2.zero;
    }
}
