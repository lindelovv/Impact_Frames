using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class Getters : MonoBehaviour
{
    public VisualEffect Block;
    public VisualEffect DashSmoke;
    public VisualEffect DashTrail;
    public VisualEffect SmokeTrail;
    public VisualEffect LandingImpact;

    public AudioSource AudioSource;
    public List<AudioClip> AudioClips;
    public AudioClip DashAudioClip;
    public AudioClip LandingHard;
    
    public void PlayClip(AudioClip audioClip)
    {
        AudioSource.PlayOneShot(audioClip);
    }

    private void Start()
    {
        DashTrail = GameObject.FindWithTag("DashTrail").GetComponent<VisualEffect>();
        if(!DashTrail) { Debug.Log("Dash null"); }
        DashTrail.enabled = false;
    }
}
