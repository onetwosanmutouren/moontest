using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


public enum Resource
{
    N1 = 0,
    N2 = 1,
    N3 = 2,
}

public enum WarehouseType
{
    Input,
    Output,
    Package
}

[Serializable]
public class ResourceBeginCount
{
    public Resource resource;
    public int count;
}


public class Warehouse : MonoBehaviour
{
    public WarehouseType Type;
    int currentCapacityCount = 0;
    public int currentFlycount = 0;

    public int CapacityCount = 100;


    public List<ResourceBeginCount> ResourceBegin;

    Dictionary<Resource, int> WarehouseCount = new();

 

    private void Start()
    {
        foreach (ResourceBeginCount resourceBegin in ResourceBegin)
        {
            currentCapacityCount += resourceBegin.count;

            for (int i = 0; i < resourceBegin.count; i++)
            {
                this[resourceBegin.resource]++;
                var go = GameManager.Instance.Creat(resourceBegin.resource);
                go.transform.position = transform.position;
                go.transform.SetParent(transform.GetChild((int) resourceBegin.resource));
                go.transform.localPosition = GetWarehousePosition(resourceBegin.resource);
            }
        }
    }

    public int this[Resource resource]
    {
        get
        {
            // �����ھ��Զ���ʼ��Ϊ 0
            if (!WarehouseCount.ContainsKey(resource))
            {
                WarehouseCount.Add(resource, 0);
            }

            return WarehouseCount[resource];
        }

        set
        {
            // ֱ����������
            WarehouseCount[resource] = value;
        }
    }

    public void AddWarehouseChangeResourceAndAnimation(Transform TargetTransform, Resource resource, int count = 1)
    {
        var go = GameManager.Instance.Creat(resource);
      
        go.transform.position = TargetTransform.position;
        go.transform.SetParent(transform.GetChild((int)resource));
        GameManager.Instance.PlayAnimation(go.transform, transform, GetWarehousePosition(resource)
            , GameManager.Instance.BuildingWarehouseCurve,
            () =>
            {
                AddWarehouseResource(resource, count);
                           
              
                SubFlyCount(count);
            });
    }

    public void SubWarehouseChangeResourceAndAnimation(Transform TargetTransform, Resource resource, int count = 1)
    {
   
        
        var go = GameManager.Instance.GetResourceChiled(resource, transform);
        if(go==null)
            return;
        go.transform.SetParent(transform.GetChild((int)resource));
        go.transform.SetParent(transform);

        GameManager.Instance.PlayAnimation(go.transform, TargetTransform, Vector3.zero
            , GameManager.Instance.WarehouseBuildingCurve,
            () =>
            {
                GameManager.Instance.Destroy(resource, go.transform);
                SubWarehouseResource(resource, count);
            });
    }


    public void ResourceTransfer(Warehouse targetWarehouse,List<Resource> types,AnimationCurve curve)
    {
        if (!targetWarehouse.HasFull(types.Count))
        {
            return;
        }

        foreach (var VARIABLE in types)
        {
            if(this[VARIABLE]<=0|| transform.GetChild((int)VARIABLE).childCount<1)
                continue;
            targetWarehouse. AddFlyCount(1);
            var go = GameManager.Instance.GetResourceChiled(VARIABLE, transform);
           
            go.transform.SetParent(targetWarehouse.transform.GetChild((int)VARIABLE));
            
            this.SubWarehouseResource(VARIABLE,1);

            GameManager.Instance.PlayAnimation( go.transform, targetWarehouse.transform, targetWarehouse.GetWarehousePosition(VARIABLE)
                , curve,
                () =>
                {
                    targetWarehouse. AddWarehouseResource(VARIABLE, 1);
                    targetWarehouse. SubFlyCount(1);
                });
            
            
            
        }
    }
    
    int GetWarehouseResource(Resource resource)
    {
        if (WarehouseCount.ContainsKey(resource) == false)
        {
            WarehouseCount.Add(resource, 0);
        }

        return WarehouseCount[resource];
    }


    public bool HasFull(int count)
    {
    
       
        return currentCapacityCount + currentFlycount + count <= CapacityCount;
    }

   public void AddFlyCount(int count)
    {
        currentFlycount += count;
    }

    void SubFlyCount(int count)
    {
        currentFlycount -= count;
    }


    void AddWarehouseResource(Resource resource, int count)
    {
        if (WarehouseCount.ContainsKey(resource) == false)
        {
            WarehouseCount.Add(resource, 0);
        }


        WarehouseCount[resource] += count;
        currentCapacityCount += count;
    }

    void SubWarehouseResource(Resource resource, int count)
    {
        if (WarehouseCount.ContainsKey(resource) == false)
        {
            WarehouseCount.Add(resource, 0);
        }

        WarehouseCount[resource] -= count;
        currentCapacityCount -= count;
    }


    Vector3 GetWarehousePosition(Resource resource)
    {
        return Vector3.up * 0.1f * (transform.GetChild((int)resource).childCount + 3) + GameManager.GetResourceOffset(resource);
      return Vector3.up * 0.1f * (this[resource] + 3+currentFlycount) + GameManager.GetResourceOffset(resource);
    }
}