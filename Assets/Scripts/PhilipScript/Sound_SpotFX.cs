using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class Sound_SpotFX : MonoBehaviour
{
    
    public AudioClip clip;

    [Header("SpotFX Source Parameters")]
    [SerializeField] AudioSource source;
    public float maxDist;
    public float minDist;
    public float spatial;
    public bool setLoop;

    [Header("Set Random Start Time")]
    public bool randomSourceTime;

    [Header("Sound Parameters: ")]
    public bool set_randomVolume;
    public float volLowRange = 0.8f;
    public float volHighRange = 1f;
    public bool set_randomPitch;
    public float pitchLowRange = 0.75f;
    public float pitchHighRange = 1f;

    void Start()
    {
        source.GetComponent<AudioSource>();
        source.clip = clip;
        source.loop = setLoop;
        source.minDistance = minDist;
        source.maxDistance = maxDist;
        source.spatialBlend = spatial;

        if (set_randomPitch == true)
        {
            source.pitch = randomPitch();
        }
        if(set_randomVolume == true)
        {
            source.volume = randomVolume();
        }
        
        
        

       

        if(randomSourceTime == true)
        {
            source.time = Random.Range(0, clip.length);
        }
        
        source.Play();
    }

    private float randomVolume()
    {
        float vol = Random.Range(volLowRange, volHighRange);
        return vol;
    }

    private float randomPitch()
    {
        float pitch = Random.Range(pitchLowRange, pitchHighRange);
        return pitch;
    }

   
}
