using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/*
 * Set Tags on Occlusion Objects that match the OcclusionDetectionTag
 * And Add a Collider (non Trigger) on the Object for the Raycast to work
 * Set RaycastLength in Inspector
 * set MaxDistance for AudioSource Sphere if enableDistanceCulling is true
 * The SoundClip is set in the Objects AudioSource
 */
[RequireComponent(typeof(AudioLowPassFilter))]
[RequireComponent(typeof(AudioSource))]
public class Sound_SmartSpotFX : MonoBehaviour
{
    [SerializeField] GameObject AudioListnerGameObject;
    private AudioSource audioSource;
    
    [Header("Audio Source Params: ")]
    public float spatialBlend;
    public float audioSourceMaxDistance = 10f;

    [Header("Occlusion: ")]
    public float lpfFreq_Low = 1000f;
    public float lpfFreq_High = 22000f;
    public bool enableOcclusionDetection;
    public string OcclusionDetectionTag;

    private AudioLowPassFilter lpFilter;
    public float rayCastLength = 15f;

    [Header("Rescource Aware Audio Source: ")]
    public bool enableDistanceCulling;
    public bool setRandomPlaybackStart;    
    public bool DebugDistance;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.maxDistance = audioSourceMaxDistance;
        audioSource.spatialBlend = Mathf.Clamp(spatialBlend, 0, 1);

        lpFilter = GetComponent<AudioLowPassFilter>();
    }
    void Start()
    {
        
        if(DebugDistance == true)
        {
            audioSourceMaxDistance = 5f;                     // Debug
            Debug.Log("Debuging max distance at: " + audioSourceMaxDistance);
        }
    }


    void Update()
    {

        audioSource.maxDistance = audioSourceMaxDistance;
        if (enableOcclusionDetection == true)
        {
            setLowpassFreq();
        }

        if(enableDistanceCulling == true)
        {
            setDistanceCulling();
        }

        Debug.Log(lpFilter.cutoffFrequency);  // Debug the used Freq

    }

    /**
     * When an object is identified the method handels proper lowPassFrequency for "Occlusion"
     */

    void setLowpassFreq()
    {

        // Can be set in Inspector if Default Value isnt wished for
        float lpfFreq;
        if (isSoundOccluded(AudioListnerGameObject, rayCastLength))
        {
            lpfFreq = lpfFreq_Low;
        }
        else
        {
            lpfFreq = lpfFreq_High; 
        }
        lpFilter.cutoffFrequency = lpfFreq;
        // Debug.Log("lpf_freq = " + lpfFreq);  //Debug on/off

    }

    /**
     * Bool check if the sound is occluded with raycast
     * Checks from the in Object (Audio Listener) with the set distance (In Inspector)
     * 1.Direction for raycast [between the scene's AudioListener and the sound source, i.e., our position]
     * 2. Mark raycast with line https://docs.unity3d.com/ScriptReference/Debug.DrawRay.html
     * 3.The Physics.Raycast method returns true if there is an obstacle between the sound source and listener
     * When the raycast identifies an obstacle, information about the identified obstacle is stored in the object occluderRayHit
     * 4. Check your tag name OcclusionDetectionTag to ensure that it is a valid obstacle
     */
    private bool isSoundOccluded(GameObject obj, float distance)
    {

        // 1
        Vector3 raycastDir = obj.transform.position - transform.position;

        // 2
        Debug.DrawRay(transform.position, raycastDir, Color.green);

        RaycastHit hit;

        // 3
        if (Physics.Raycast(transform.position, raycastDir, out hit, distance))
        {
            // 4
            if (hit.collider.gameObject.tag == OcclusionDetectionTag) 
            {
                return true; // isSoundOccluded true
            }
        }

        return false;
    }


    /*
     * CheckDistance() to the AudioListenerGameObj and compare with the max distance set
     * on the AudioSource Sphere.
     * Play sound if AudioListener isInside the range  
     * // MÅSTE ÄNDRAS TILL ATT DET ÄR PLAYER IN RANGE FÖR VI ÄR INTE KAMERAN
     * else stop/cull playback to save resources
     */
    void setDistanceCulling()
    {

        float dist = CheckDistance(AudioListnerGameObject);

        if (dist < audioSourceMaxDistance)
        {
            
            if (audioSource.isPlaying == false)
            {
                if (setRandomPlaybackStart)
                {
                    audioSource.time = Random.Range(0f, audioSource.clip.length);
                }

                audioSource.Play();
            }
            //Debug.Log("distance = " + dist + ": Audio Restarted");
        }
        else
        {
            
            audioSource.Stop(); 
            //Debug.Log("distance = " + dist + ": Audio Stopped");
        }
    }

    /*
     * Checks Obj with Audio Listener, return distance to that obj.
     */
    private float CheckDistance(GameObject obj)
    {
        float dist = Vector3.Distance(obj.transform.position, transform.position);

        return dist;
    }


}



