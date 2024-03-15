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

    public bool isTimerRunning;
    public bool timerCompleted;


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


    // Anv�nd i d�r man klickar och s�tt timerCompleted till false efter tiden �r klar
    private IEnumerator TimerCoroutine()
    {
        isTimerRunning = true; // Mark the timer as running.
        yield return new WaitForSeconds(3f); // Wait for 3 seconds.
        timerCompleted = true; // Mark the timer as completed.
        isTimerRunning = false; // Reset the timer running flag.
    }
}
