using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class Sound_AmbianceController : MonoBehaviour
{
    public AudioSource[] sources;   // Add audioSources in INSPECTOR
   

    public AudioClip[] sounds;
    [Header("Audio Source Parameters: ")]
    public float mainVol = 0.3f;
    public float otherVol = 0.5f;
    

    [Header("Random Delay Timers: ")]
    public bool activeRandomSounds;
    public float minDelay;
    public float maxDelay;

    [Header("Audio Mixer Parameters: ")]
    public bool useMixer;
    [SerializeField] AudioMixerGroup mixer;

    public static Sound_AmbianceController instance;

    public void Awake()
    {
        if(instance == null){
            instance = this;
        } 
    }

    void Start()
    {
        
        HandleAudioSources();

    }

    void HandleAudioSources()
    {

        sources = GetComponents<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        {

            GameObject child = new GameObject("AudioSource " + i);
            child.transform.parent = gameObject.transform;
            sources[i] = child.AddComponent<AudioSource>();
            sources[i].clip = sounds[i];
            if (useMixer == true)
            {
                sources[i].outputAudioMixerGroup = mixer;
            }

            sources[0].volume = mainVol;
            sources[0].loop = true;
            sources[0].Play();
                       
            

        }
    }

    void Update()
    {
        randomDelayedSounds();
    }

    void randomDelayedSounds()
    {
        for(int i = 1; i < sources.Length; i++)
        {
            if (!sources[i].isPlaying && activeRandomSounds)  // Fel för alla kan spela samtidigt som varandra men inte som sig själva.
            {

                sources[i].volume = otherVol;
                sources[Random.Range(1, sources.Length)].PlayDelayed(Random.Range(minDelay, maxDelay));
                //sources[1].PlayDelayed(Random.Range(minDelay, maxDelay));
                //sources[1].PlayOneShot(sounds[Random.Range(1, sounds.Length)]);


            }
        }
        
    }
}
