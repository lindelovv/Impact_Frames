// Grundlig introduktion till raycasts (informationsfilm finns på sidan!:-)
// https://gamedevbeginner.com/raycasts-in-unity-made-easy/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_Occlusion_LowPass : MonoBehaviour
{
    private GameObject componentWithAudioListener;
    private AudioSource audioSource;
    private AudioLowPassFilter lpFilter;
    private float rayCastLength = 15f;

    // Start is called before the first frame update
    void Start()
    {
        componentWithAudioListener = GameObject.Find("FPSController");
        audioSource = GetComponent<AudioSource>();
        lpFilter = GetComponent<AudioLowPassFilter>();
        audioSource.Play();

        
    }


    void Update()
    {
        
        // När en vägg identifierats mellan ljudkälla och lyssnare så "aktiveras" lågpassfiltret.
        bool occlusion = GetOcclusion(componentWithAudioListener, rayCastLength);
        float lpfFreq;
        if (occlusion)
        {
            lpfFreq = 1000f;
        }
        else
        {
            lpfFreq = 22000f;
        }
        lpFilter.cutoffFrequency = lpfFreq;
        Debug.Log("lpf_freq = " + lpfFreq);
        
    }

    // obj är det objekt som har scenens AudioListener
    // distance är längden på RayCast'en (max avstånd för när ett ljud ska kunna höras/uppfattas)
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
            if (hit.collider.gameObject.tag == "Wall")
            {
                return true; // lpf-frekvens
            }
        }

        return false;
    }

}

