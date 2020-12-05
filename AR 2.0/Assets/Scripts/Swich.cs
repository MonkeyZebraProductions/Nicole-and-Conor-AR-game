using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Swich : MonoBehaviour
{
    public Transform StartPos, EndPos;

    private bool _switch;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = StartPos.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_switch)
        {
            transform.position = EndPos.position;
        }
        else
        {
            transform.position = StartPos.position;     
        }
    }

    private void OnMouseDown()
    {
        _switch = !_switch;
    }

}
