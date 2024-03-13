using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]


/**
 * First AudioSource RequiredComponent is dedicated for the Main Ambient Clip
 * Every other Audio Source added can be used as a new player for 1 audio clip
 * Make sure to add more Audio Sources if More Audio Clips are used
 * Expand Fill the Array with the new Audio Sources
 * AudioSources[] should ALWAYS be 1 more than AudioClip[]
 */
public class Sound_AmbianceController : MonoBehaviour
{
    public AudioSource[] sources;

    // Fill the array with the AudioClips to play, for example thunderclaps
    public AudioClip[] ambientOneShots;

    // If you want a main overhead Ambient sound, like rain, slot here, or leave empty
    // Dont forget setting bools
    [Header("Main Ambient Clip: ")]
    public AudioClip AmbientMainSound;
    public bool isMainAmbLooping = true;
    public float mainAmbVolume = 0.3f;
      

    // Dont forget setting bools
    [Header("ambientOneShots] Parameters: ")]
    public bool hasRandomInterval = true;
    public float minInterval;
    public float maxInterval;
    public float ambClipVolume = 0.5f;

    [Header("Audio Mixer Parameters: ")]
    public bool useMixer;
    [SerializeField] AudioMixerGroup mixer;

    public static Sound_AmbianceController instance;

    public void Awake()
    {
        if(instance == null){
            instance = this;
        }

        HandleMainAmbiance();
        HandleAudioSources();

    }

  
    void HandleAudioSources()
    {
        // Makes a childObject with the new AudioSources that you use and fill them with the AudioClips
        sources = GetComponents<AudioSource>();
        for (int i = 1; i < sources.Length; i++)
        {

            GameObject child = new GameObject("AudioSource " + i);
            child.transform.parent = gameObject.transform;
            sources[i] = child.AddComponent<AudioSource>(); // lägger till en ny audiousource
            sources[i].clip = ambientOneShots[i-1];
            //sources[0].clip = sounds[0];
            if (useMixer == true)
            {
                sources[i].outputAudioMixerGroup = mixer;
            }
            

        }

        
    }

    void HandleMainAmbiance()
    {
        sources[0].clip = AmbientMainSound;
        sources[0].volume = mainAmbVolume;
        sources[0].loop = isMainAmbLooping;
        sources[0].Play();
    }

    void Update()
    {
        randomDelayedSounds();
        sources[0].volume = mainAmbVolume;  // update volume
    }

    void randomDelayedSounds()
    {
        for(int i = 1; i < sources.Length; i++)
        {
            if (!sources[i].isPlaying && hasRandomInterval)
            {
                sources[i].volume = ambClipVolume;
                sources[Random.Range(0, ambientOneShots.Length+1)].PlayDelayed(Random.Range(minInterval, maxInterval));
                               
                //sources[1].PlayDelayed(Random.Range(minDelay, maxDelay));
                //sources[1].PlayOneShot(sounds[Random.Range(1, sounds.Length)]);


            }
        }
        
    }
}
