using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_ResourceAware : MonoBehaviour
{
    [SerializeField] GameObject componentWithAudioListener;
    private AudioSource audioSource;
    private float maxDistance; // Sätts via AudioSource'n i Inspector
   


    void Start()
    {
        
        componentWithAudioListener = GameObject.Find("FPSController");
        audioSource = GetComponent<AudioSource>();
        maxDistance = audioSource.maxDistance; // läs av ljudkällans maxDistance (3D-sfär)
        //maxDistance = 5f;                     // För debugging
    }

    
    void Update()
    {
        // beräkna avståndet mellan ljudkälla och lyssnare
        float dist = CheckDistance(componentWithAudioListener);

        if (  dist < maxDistance )
        {
            // ljudet återstartas när lyssnaren är inom ljudkällans hörbara område
            if( audioSource.isPlaying == false ) audioSource.Play();
            Debug.Log("distance = " + dist + ": Audio Restarted");
        }
        else
        {
            // Stäng av uppspelning när lyssnaren är utanför ljudkällans hörbara område (sparar resurser)
            audioSource.Stop(); // Vid kommande återstart - ska uppspelning börja därifrån den är eller ta det från början?
            Debug.Log("distance = " + dist + ": Audio Stopped");
        }
    }

    // obj är det objekt som har scenens AudioListener
    // metoden returnerar avståndet till scenens AudioListener
    private float CheckDistance(GameObject obj)
    {
        float dist = Vector3.Distance(obj.transform.position, transform.position);

        return dist;
    }

}

