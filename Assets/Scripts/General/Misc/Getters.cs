using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using Random = UnityEngine.Random;

public class Getters : MonoBehaviour
{
    public GameObject Body;
    public GameObject Face;
    public GameObject Hair;
    
    public VisualEffect Block;
    public VisualEffect DashSmoke;
    [HideInInspector] public VisualEffect DashTrail;
    public VisualEffect SmokeTrail;
    public VisualEffect LandingImpact;

    public AudioSource AudioSource;
    public AudioSource BlockAudioSource;
    public List<AudioClip> AudioClips;
    public AudioClip DashAudioClip;
    public AudioClip LandingHard;
    public AudioClip BlockClip;

    public float DashTime;
    public bool isTimerRunning;
    public bool timerCompleted;

    public Material[] BodyMaterials;
    public Material[] FaceMaterials;
    public Material[] HairMaterials;
    public Material[] EtherealMat;

    public List<AudioClip> FootstepSounds;
    
    public bool _hasHit;

    private void Start()
    {
        DashTrail = GameObject.FindWithTag("DashTrail").GetComponent<VisualEffect>();
        if(!DashTrail) { Debug.Log("Dash null"); }
        DashTrail.Stop();
        DashSmoke.Stop();
        BodyMaterials = Body.GetComponent<SkinnedMeshRenderer>().materials;
        FaceMaterials = Face.GetComponent<SkinnedMeshRenderer>().materials;
        HairMaterials = Hair.GetComponent<SkinnedMeshRenderer>().materials;
    }

    public void ToggleEthereal(bool etherealState)
    {
        if (etherealState)
        {
            Body.GetComponent<SkinnedMeshRenderer>().materials = EtherealMat;
            Face.GetComponent<SkinnedMeshRenderer>().materials = EtherealMat;
            Hair.GetComponent<SkinnedMeshRenderer>().materials = EtherealMat;
        }
        else
        {
            Body.GetComponent<SkinnedMeshRenderer>().materials = BodyMaterials;
            Face.GetComponent<SkinnedMeshRenderer>().materials = FaceMaterials;
            Hair.GetComponent<SkinnedMeshRenderer>().materials = HairMaterials;
        }
    }
    
    public void PlayRandomFootsteps()
    {
        var index = Random.Range(0, FootstepSounds.Count);
        AudioSource.PlayOneShot(FootstepSounds[index]);
    }
    
    public void PlayClip(AudioClip audioClip)
    {
        AudioSource.PlayOneShot(audioClip);
    }
    
    public void PlayHitClip(AudioClip audioClip)
    {
        //if (_hasHit)
        {
            AudioSource.PlayOneShot(audioClip);
        }
    }

    // Använd i där man klickar och sätt timerCompleted till false efter tiden är klar
    private IEnumerator TimerCoroutine()
    {
        isTimerRunning = true; // Mark the timer as running.
        yield return new WaitForSeconds(DashTime); // Wait for {DashTime} seconds.
        timerCompleted = true; // Mark the timer as completed.
        isTimerRunning = false; // Reset the timer running flag.
    }

    public void Timer()
    {
        var co = TimerCoroutine();
        StartCoroutine(co);
    }
}
