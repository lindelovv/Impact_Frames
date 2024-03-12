using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


[RequireComponent(typeof(BoxCollider))]
public class Sound_Collider_Controlls : MonoBehaviour
{

    [SerializeField] private AudioMixerGroup mixGroup_from, mixGroup_to;
    [SerializeField] public float fadeTime = 1.2f;
    [SerializeField] private string exposedPar_From;
    [SerializeField] private string exposedPar_To;

    [Header("Set volume min: -80 max: 20: ")]
    [SerializeField] private float turnUpVol_From = 0f;
    [SerializeField] private float turnDownVol_From = -80f;
    [SerializeField] private float turnUpVol_To = 0f;
    [SerializeField] private float turnDownVol_To = -80f;


    public Sound_Collider_Controlls()
    {
        
    }

    private bool isFading = false;
    
    


    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFading)
        {

            
            StartCoroutine(Fade(mixGroup_from, turnDownVol_From, fadeTime, exposedPar_From));
            StartCoroutine(Fade(mixGroup_to, turnUpVol_To, fadeTime, exposedPar_To));
            

            isFading = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isFading)
        {

            StartCoroutine(Fade(mixGroup_from, turnUpVol_From, fadeTime, exposedPar_From));
            StartCoroutine(Fade(mixGroup_to, turnDownVol_To, fadeTime, exposedPar_To));
            isFading = true;
        }
    }

    private IEnumerator Fade(AudioMixerGroup mixerGroup, float endVolume, float fadeTime, string volumeExpParameterName)
    {
      
        float timeElapsed = 0f;
        float startVolume;
         
      
        mixerGroup.audioMixer.GetFloat(volumeExpParameterName, out startVolume);  // Save the volume float of the mixertrack into a variable
        

        


        while (timeElapsed < fadeTime)
        {
            float t = timeElapsed / fadeTime;
            mixerGroup.audioMixer.SetFloat(volumeExpParameterName, Mathf.Lerp(startVolume, endVolume, t));
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        mixerGroup.audioMixer.SetFloat(volumeExpParameterName, endVolume);  // Save the volume float of the mixertrack into a variable
        isFading = false;
    }
}
