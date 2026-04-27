 
using System;
using Cysharp.Threading.Tasks; 

using UnityEngine;

 

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static GameManager Instance; 
    public GameObject N1;
    public GameObject N2;
    public GameObject N3;


    
    public AnimationCurve BuildingWarehouseCurve;
    public AnimationCurve WarehouseCharacterCurve;
    public AnimationCurve CharacterWarehouseCurve;
    public AnimationCurve WarehouseBuildingCurve;
    private void Awake()
    {
        Instance = this;
    }


  
    
    
    public GameObject Creat(Resource resource)
    {
        GameObject go=null ;

      
       switch (resource)
       {
           case Resource.N1:
               go =  GameObject.Instantiate(GameManager.Instance.N1);
     
               break;
           case Resource.N2:
               go =GameObject.Instantiate(GameManager.Instance.N2); 
               break;
           case Resource.N3:
               go = GameObject.Instantiate(GameManager.Instance.N3); 
               break;
       }
       

   
       go.transform.localPosition = Vector3.zero+GetResourceOffset(resource);
       return go;
    
    }

    public void Destroy(Resource resource,Transform Creattransform)
    {
        GameObject.Destroy(Creattransform.gameObject);
    }
    
    public GameObject GetResourceChiled(Resource resource,Transform Creattransform)
    {
        GameObject go=null ;


        if (Creattransform.GetChild((int)resource).childCount<1)
        {
            return null;
        }
        
        switch (resource)
        {
            case Resource.N1:
                go =  Creattransform.GetChild(0).GetChild( Creattransform.GetChild(0).childCount - 1).gameObject;
                
                break;
            case Resource.N2:
                go =  Creattransform.GetChild(1).GetChild( Creattransform.GetChild(1).childCount - 1).gameObject;

                break;
            case Resource.N3:
                go =  Creattransform.GetChild(2).GetChild( Creattransform.GetChild(2).childCount - 1).gameObject;

                break;
        }

        if (go == null)
        {
            Debug.LogError("空");
        }
    
        return go;
    
    }
    public static Vector3 GetResourceOffset(Resource resource)
    {
        Vector3 offset=Vector3.zero;
        switch (resource)
        {
            case Resource.N1:
                offset = Vector3.forward * 0.4f;
                break;
            case Resource.N2:
                offset = Vector3.forward * -0.4f;
                break;
            case Resource.N3:
                offset = Vector3.forward * 0f;
                break;
        }
        return offset;
    }
   

  
    
    
    
    
    public async UniTask PlayAnimation2(
        Transform goTransform, 
        Transform TargetTrans, 
        Vector3 offset, 
        Action onComplete = null, 
        float time = 1, 
        float height = 6)
    {
        if (goTransform == null || TargetTrans == null)
        {
            onComplete?.Invoke();
            return;
        }

        // 1. 记录起飞点的【世界坐标】
        Vector3 worldStart = goTransform.position;

        float timePassed = 0f;

        while (timePassed < time)
        {
            if (goTransform == null) return;
            if (TargetTrans == null) return;

            timePassed += Time.deltaTime;
            float t = Mathf.Clamp01(timePassed / time);

            // 2. ★ 修正 Offset 的空间问题 ★
            // TransformPoint 会把 "目标的局部偏移" 转换成 "世界坐标的绝对位置"
            // 这样无论 Target 怎么转身，offset(比如头顶、身前)都会死死跟着它
            Vector3 worldTargetNow = TargetTrans.TransformPoint(offset);

            // 3. 在绝对静止的【世界坐标】下算轨迹 (XZ水平追踪)
            Vector3 startXZ = new Vector3(worldStart.x, 0, worldStart.z);
            Vector3 targetXZ = new Vector3(worldTargetNow.x, 0, worldTargetNow.z);
            Vector3 currentXZ = Vector3.Lerp(startXZ, targetXZ, t);

            // 4. 在【世界坐标】下算高度 (先上后下)
            float baseY = Mathf.Lerp(worldStart.y, worldTargetNow.y, t);
            float arcOffset = height * 4f * t * (1f - t); 
            float currentY = baseY + arcOffset;

            // 5. 组合成最终的【世界坐标】落点
            Vector3 worldFinalPos = new Vector3(currentXZ.x, currentY, currentXZ.z);

            // 6. ★ 核心：将世界落点，翻译成旋转中的父物体的局部坐标 ★
            // 注意：这里必须用父物体的 InverseTransformPoint
            Vector3 localFinalPos = goTransform.parent != null 
                ? goTransform.parent.InverseTransformPoint(worldFinalPos) 
                : worldFinalPos;

            // 7. 赋值
            goTransform.localPosition = localFinalPos; 
            await UniTask.Yield();
        }

        // 8. 结束时强制命中
        if (goTransform == null) return;

        if (TargetTrans != null)
        {
            // 结束时同样用 TransformPoint 算出准确的世界坐标
            Vector3 worldEndPos = TargetTrans.TransformPoint(offset);
            
            // 转换回局部坐标赋值
            Vector3 localEndPos = goTransform.parent != null 
                ? goTransform.parent.InverseTransformPoint(worldEndPos) 
                : worldEndPos;

            goTransform.localPosition = localEndPos;
        }

        onComplete?.Invoke();
    }
    
    
    
     public async UniTask PlayAnimation(
        Transform goTransform, 
        Transform TargetTrans, 
        Vector3 offset, 
        AnimationCurve heightCurve, 
        // 新增：曲线参数
        Action onComplete = null, 
        float time = 1, 
        float height = 6)
    {
        // 1. 安全校验（加上曲线的校验）
        if (goTransform == null || TargetTrans == null )
        {
            onComplete?.Invoke();
            return;
        }

        Vector3 worldStart = goTransform.position;
        float timePassed = 0f;

        while (timePassed < time)
        {
            if (goTransform == null) return;
            if (TargetTrans == null) return;

            timePassed += Time.deltaTime;
            float t = Mathf.Clamp01(timePassed / time);

            // 2. 获取目标实时世界坐标
            Vector3 worldTargetNow = TargetTrans.TransformPoint(offset);

            // 3. XZ 水平追踪 (保持不变)
            Vector3 startXZ = new Vector3(worldStart.x, 0, worldStart.z);
            Vector3 targetXZ = new Vector3(worldTargetNow.x, 0, worldTargetNow.z);
            Vector3 currentXZ = Vector3.Lerp(startXZ, targetXZ, t);

            // 4. ★ Y 轴高度计算：使用 AnimationCurve 替代固定的数学公式 ★
            // baseY: 平滑过渡起点和终点本身的高度差（防止目标在悬崖上飞不过去）
            float baseY = Mathf.Lerp(worldStart.y, worldTargetNow.y, t);
            
            // 通过曲线获取当前时间的高度比例 (0~1之间，或者超出)
            float curveValue = heightCurve.Evaluate(t);
            
            // 实际抛物线偏移量 = 设定的最大高度 * 曲线比例
            float arcOffset = height * curveValue; 
            
            float currentY = baseY + arcOffset;

            // 5. 组合最终世界坐标
            Vector3 worldFinalPos = new Vector3(currentXZ.x, currentY, currentXZ.z);

            // 6. 转换为局部坐标赋值
            Vector3 localFinalPos = goTransform.parent != null 
                ? goTransform.parent.InverseTransformPoint(worldFinalPos) 
                : worldFinalPos;

            goTransform.localPosition = localFinalPos;

            await UniTask.Yield();
        }

        // 7. 强制精准命中
        if (goTransform == null) return;

        if (TargetTrans != null)
        {
            Vector3 worldEndPos = TargetTrans.TransformPoint(offset);
            Vector3 localEndPos = goTransform.parent != null 
                ? goTransform.parent.InverseTransformPoint(worldEndPos) 
                : worldEndPos;

            goTransform.localPosition = localEndPos;
        }

        onComplete?.Invoke();
    }
 
}
