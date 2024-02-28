using System.Collections;
using System.Collections.Generic;
using Unity.NetCode;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    private Transform coord;
    [SerializeField] GameObject fallingObject;

    private void Start()
    {
        fallingObject = GetComponent<GameObject>();
        coord = GetComponent<Transform>();
    }
    void Fall()
    {
        Quaternion spawnrotation = Quaternion.Euler(0, 0, 0);
        Instantiate(fallingObject, coord.position, spawnrotation);
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hallooo");
        if (other.gameObject.tag == "Player")
        {
            
            Fall();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Hallooo");
        if (other.gameObject.tag == "Player")
        {

            Fall();
        }
    }


}
