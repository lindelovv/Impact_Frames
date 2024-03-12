using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(AudioLowPassFilter))]
public class Sound_SmartSpotFX : MonoBehaviour
{
    [SerializeField] GameObject AudioListnerGameObject;
    private AudioSource audioSource;
    
    [Header("Occlusion: ")]
    public float lpfFreq_Low = 1000f;
    public float lpfFreq_High = 22000f;
    public bool Set_Occlusion;

    private AudioLowPassFilter lpFilter;
    private float rayCastLength = 15f;

    [Header("Rescource Awareness: ")]
    public bool Set_AwareAudioListener;
    public bool DebugDistance;
    private float maxDistance; // Sätts via AudioSource'n i Inspector


    // FOR OCCLUSION, ADD COLLIDER(Not triggered) AND TAGG WALL

    void Start()
    {
        AudioListnerGameObject = GameObject.Find("FPSController"); // Find GameObject With AudioListener
        audioSource = GetComponent<AudioSource>();
        lpFilter = GetComponent<AudioLowPassFilter>();
        
        

        
        maxDistance = audioSource.maxDistance; // läs av ljudkällans maxDistance (3D-sfär)
        
        if(DebugDistance == true)
        {
            maxDistance = 5f;                     // För debugging
            Debug.Log("Debuging max distance at: " + maxDistance);
        }
        


    }


    void Update()
    {
        if(Set_Occlusion == true)
        {
            runOcclusion();
        }

        if(Set_AwareAudioListener == true)
        {
            RunDistanceAwareAudioSource();
        }
       


    }

    void runOcclusion()
    {

        // När en vägg identifierats mellan ljudkälla och lyssnare så "aktiveras" lågpassfiltret.
        bool occlusion = GetOcclusion(AudioListnerGameObject, rayCastLength);
        float lpfFreq;
        if (occlusion)
        {
            lpfFreq = lpfFreq_Low; // Set in Inspector
        }
        else
        {
            lpfFreq = lpfFreq_High; // Set in Inspector
        }
        lpFilter.cutoffFrequency = lpfFreq;
        // Debug.Log("lpf_freq = " + lpfFreq);  //Debug on/off

    }

    private bool GetOcclusion(GameObject obj, float distance)
    {

        // Riktning för raycast [mellan scenens AudioListener och ljudkällan, d.v.s. vår position]
        Vector3 raycastDir = obj.transform.position - transform.position;

        // Markera raycast med linje 
        // https://docs.unity3d.com/ScriptReference/Debug.DrawRay.html
        Debug.DrawRay(transform.position, raycastDir, Color.green);

        RaycastHit hit;
        // Utför raycast.
        // Metoden Physics.Raycast returnerar true vid hinder mellan ljudkälla och lyssnare
        // När raycasten identifierat hinder anges info om identifierat hinder i objektet occluderRayHit
        bool obstacle = Physics.Raycast(transform.position, raycastDir, out hit, distance);
        if (obstacle)
        {
            // säkerställ att det är ett giltigt hinder och inte bara en liten mygga eller projektil
            if (hit.collider.gameObject.tag == "Wall") // Tag with apropriate tag "SoundBarrier" and put colliders with soundbarrier-tag where needed
            {
                return true; // lpf-frekvens
            }
        }

        return false;
    }

    void RunDistanceAwareAudioSource()
    {

        // beräkna avståndet mellan ljudkälla och lyssnare
        float dist = CheckDistance(AudioListnerGameObject);

        if (dist < maxDistance)
        {
            // ljudet återstartas när lyssnaren är inom ljudkällans hörbara område
            if (audioSource.isPlaying == false) audioSource.Play();
            //Debug.Log("distance = " + dist + ": Audio Restarted");
        }
        else
        {
            // Stäng av uppspelning när lyssnaren är utanför ljudkällans hörbara område (sparar resurser)
            audioSource.Stop(); // Vid kommande återstart - ska uppspelning börja därifrån den är eller ta det från början?
            //Debug.Log("distance = " + dist + ": Audio Stopped");
        }
    }

    
    private float CheckDistance(GameObject obj)
    {
        // obj är det objekt som har scenens AudioListener
        // metoden returnerar avståndet till scenens AudioListener

        float dist = Vector3.Distance(obj.transform.position, transform.position);

        return dist;
    }


}



