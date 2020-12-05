using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class TouchManager : MonoBehaviour
{
   

    public Camera cam;

    public NavMeshAgent agent;

    public LayerMask whatIsNav, whatIsSwitch;
    public UnityEvent Switch;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
   
            Debug.DrawLine(ray.origin, hit.point);
        }
        if (Input.touchCount>0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = cam.ScreenPointToRay(touch.position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }

            Debug.DrawLine(ray.origin, hit.point);
        }

    }

   

  
}
