using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class BaseBuilding : MonoBehaviour
{


    public Warehouse Inputwarehouse;


    public Warehouse Outputwarehouse;

    public List<Resource> InputResources;
    public List<Resource> OutputResources;

    public float ProductionTime=0.1f;

    [SerializeField]
    private TextMeshProUGUI text;
    async UniTaskVoid Product()
    {
        if(this==null)
            return;
        bool HasResource=true; 

        foreach (Resource resource in InputResources) {
            if (Inputwarehouse[resource] <= 0)
            {
                HasResource = false; 
                break;
            }
        }

        
        bool HasOutputResourcesFull = Outputwarehouse.HasFull(OutputResources.Count);
        if (HasResource==false)
        {
            text.text = gameObject.name + "Empty:";
            foreach (Resource resource in InputResources)
            {
                text.text += resource + " ";
            }

            if (HasOutputResourcesFull==false)
            {
                text.text +=  "And Outputwarehouse Full ";
            }
            
            
            return;
        }
       
        if (HasOutputResourcesFull==false)
        {
            text.text =gameObject.name+  " Outputwarehouse Full ";
            return;
        }

        text.text = gameObject.name;
       await Input();
       await Output();
    }

 


    float timer=0.0f;
    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >ProductionTime)
        {
            Product();
            timer = 0;
        }
    }

    public async virtual UniTask Input()
    {

      
        foreach (Resource resource in InputResources)
        {
            if(GameManager.Instance==null)
                return;
     
             Inputwarehouse.SubWarehouseChangeResourceAndAnimation(transform, resource);
          
           
        }
        
        foreach (Resource resource in OutputResources)
        { 
            if(GameManager.Instance==null)
                return;
            Outputwarehouse.AddFlyCount(1);
          
           
        }
        
         await UniTask.Delay(1000);
    }

    public async virtual UniTask Output()
    {
        
       
        foreach (Resource resource in OutputResources)
        { 
            if(GameManager.Instance==null)
                return;

            Outputwarehouse.AddWarehouseChangeResourceAndAnimation(transform,resource);
 
           
        }
        
     
    }



    
    

}
