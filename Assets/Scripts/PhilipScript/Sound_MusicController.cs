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
    public bool isLooping = true;
    
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

    private void Update()
    {
        source.volume = volume;
    }

    void HandleAudioSource()
    {

        source = GetComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.loop = isLooping;
        source.Play();

        if (useMixer == true)
        {
            source.outputAudioMixerGroup = mixer;
            
        }

    }


  

}
