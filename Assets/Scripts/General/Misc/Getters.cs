using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Getters : MonoBehaviour
{
    public VisualEffect Block;
    public VisualEffect DashSmoke;
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
}
