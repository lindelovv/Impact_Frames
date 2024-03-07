using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Checks what direction the players comes from via a collisionCheckTrigger (boxcollider) 
// And let them pass throuh one way, depending on what settings. 
// local direction lets the colliders follow moving objects

[RequireComponent(typeof(BoxCollider))]

public class OneWayBoxCollider : MonoBehaviour
{

    [SerializeField] private Vector3 entryDirection = Vector3.up;
    [SerializeField] private Boolean hasLocalDirection = false;
    [SerializeField, Range (1.0f, 2.0f)] private float triggerScale = 1.25f;
    private BoxCollider collisionBox = null;
    private BoxCollider collisionCheckTrigger = null;


    private void Awake()
    {
        collisionBox = GetComponent<BoxCollider>();
        collisionBox.isTrigger = false;

        //Physics.IgnoreCollision.Add(boxCollider);

        collisionCheckTrigger = gameObject.AddComponent<BoxCollider>();
        collisionCheckTrigger.size = collisionBox.size * triggerScale;
        collisionCheckTrigger.center = collisionBox.center;
        collisionCheckTrigger.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)      //Trigger stay always happens
    {
        if(Physics.ComputePenetration(
            collisionCheckTrigger, transform.position, transform.rotation, 
            other, other.transform.position, other.transform.rotation,
            out Vector3 collisionDirection, out float penetrationDepth))
        {

            Vector3 direction;
            // Checking if direction has Local Direction bool set to true
            if (hasLocalDirection)
            {
                direction = transform.TransformDirection(entryDirection.normalized);
            }
            else
            {
                direction = entryDirection;
            }


            float dot = Vector3.Dot(direction, collisionDirection);
            // Opposite direction, passing not allowed
            if(dot < 0) {

                Physics.IgnoreCollision(collisionBox, other, false);
            }
            else
            {
                Physics.IgnoreCollision(collisionBox, other, true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {

        Vector3 direction;
        // Checking if direction has Local Direction bool set to true
        if (hasLocalDirection)
        {
            direction = transform.TransformDirection(entryDirection.normalized);
        }
        else { 
            direction = entryDirection; 
        }
        

        Gizmos.color = Color.red;    
        Gizmos.DrawRay(transform.position, direction);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -direction);

       
    }

}
