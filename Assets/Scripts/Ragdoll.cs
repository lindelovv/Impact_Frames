using System;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public GameObject               rig;
    public Animator            animator;

    private Collider[]    limbColliders;
    private Rigidbody[] limbRigidbodies;

    public void Start()
    {
        limbColliders = rig.GetComponentsInChildren<Collider>();
        limbRigidbodies = rig.GetComponentsInChildren<Rigidbody>();
        SetRagdoll(false);
    }

    void SetRagdoll(bool shouldRagdoll)
    {
        if (shouldRagdoll)
        {
            foreach (var col in limbColliders)
            {
                col.enabled = true;
            }
            foreach (var rigid in limbRigidbodies)
            {
                rigid.isKinematic = false;
            }
            animator.enabled = false;
        }
        else
        {
            foreach (var col in limbColliders)
            {
                col.enabled = false;
            }
            foreach (var rigid in limbRigidbodies)
            {
                rigid.isKinematic = true;
            }
            animator.enabled = true;
        }
    }
}
