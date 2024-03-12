using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class Sound_MusicController : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip;

    [Header("Audio Source Parameters: ")]
    public float volume = 0.3f;
    
    [Header("Audio Mixer Parameters: ")]
    public bool useMixer;
    [SerializeField] AudioMixerGroup mixer;



    public static Sound_MusicController instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {

        HandleAudioSource();

    }

    void HandleAudioSource()
    {

        source = GetComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        source.Play();

        if (useMixer == true)
        {
            source.outputAudioMixerGroup = mixer;
            
        }

    }


  

}
