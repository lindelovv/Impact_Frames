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
    private float maxDistance; // S�tts via AudioSource'n i Inspector


    // FOR OCCLUSION, ADD COLLIDER(Not triggered) AND TAGG WALL

    void Start()
    {
        AudioListnerGameObject = GameObject.Find("FPSController"); // Find GameObject With AudioListener
        audioSource = GetComponent<AudioSource>();
        lpFilter = GetComponent<AudioLowPassFilter>();
        
        

        
        maxDistance = audioSource.maxDistance; // l�s av ljudk�llans maxDistance (3D-sf�r)
        
        if(DebugDistance == true)
        {
            maxDistance = 5f;                     // F�r debugging
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

        // N�r en v�gg identifierats mellan ljudk�lla och lyssnare s� "aktiveras" l�gpassfiltret.
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

        // Riktning f�r raycast [mellan scenens AudioListener och ljudk�llan, d.v.s. v�r position]
        Vector3 raycastDir = obj.transform.position - transform.position;

        // Markera raycast med linje 
        // https://docs.unity3d.com/ScriptReference/Debug.DrawRay.html
        Debug.DrawRay(transform.position, raycastDir, Color.green);

        RaycastHit hit;
        // Utf�r raycast.
        // Metoden Physics.Raycast returnerar true vid hinder mellan ljudk�lla och lyssnare
        // N�r raycasten identifierat hinder anges info om identifierat hinder i objektet occluderRayHit
        bool obstacle = Physics.Raycast(transform.position, raycastDir, out hit, distance);
        if (obstacle)
        {
            // s�kerst�ll att det �r ett giltigt hinder och inte bara en liten mygga eller projektil
            if (hit.collider.gameObject.tag == "Wall") // Tag with apropriate tag "SoundBarrier" and put colliders with soundbarrier-tag where needed
            {
                return true; // lpf-frekvens
            }
        }

        return false;
    }

    void RunDistanceAwareAudioSource()
    {

        // ber�kna avst�ndet mellan ljudk�lla och lyssnare
        float dist = CheckDistance(AudioListnerGameObject);

        if (dist < maxDistance)
        {
            // ljudet �terstartas n�r lyssnaren �r inom ljudk�llans h�rbara omr�de
            if (audioSource.isPlaying == false) audioSource.Play();
            //Debug.Log("distance = " + dist + ": Audio Restarted");
        }
        else
        {
            // St�ng av uppspelning n�r lyssnaren �r utanf�r ljudk�llans h�rbara omr�de (sparar resurser)
            audioSource.Stop(); // Vid kommande �terstart - ska uppspelning b�rja d�rifr�n den �r eller ta det fr�n b�rjan?
            //Debug.Log("distance = " + dist + ": Audio Stopped");
        }
    }

    
    private float CheckDistance(GameObject obj)
    {
        // obj �r det objekt som har scenens AudioListener
        // metoden returnerar avst�ndet till scenens AudioListener

        float dist = Vector3.Distance(obj.transform.position, transform.position);

        return dist;
    }


}



