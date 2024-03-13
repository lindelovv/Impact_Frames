using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * In ECS put this script on object outside SubScene and reference it in the AudioTriggerSystem.cs
 * Handle onPlay() and other things in AudioTriggerSystem.sc
 */
[RequireComponent(typeof(AudioSource))]
public class AudioTriggerZone_Source : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip;

    [Header("Trigger Parameters")]
    public bool PlayOnce;
    public float volume;
    public float audioSourceMinDistance = 3f;
    public float audioSourceMaxDistance = 10f;
    public float spatialBlend;
    

    void Start()
    {
        source.GetComponent<AudioSource>();
        source.clip = clip;
        source.minDistance = audioSourceMinDistance;
        source.maxDistance = audioSourceMaxDistance;
        source.spatialBlend = spatialBlend;
        source.volume = volume;

    }



    /**
     * This is used for gameObj collisions. 
     */
   /* private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && !source.isPlaying){
            Debug.Log("Player Collided");
            source.PlayOneShot(clip);

            if (PlayOnce)
            {
                StartCoroutine(DestroyAfterClip());
            }

        }
    }*/

    public IEnumerator DestroyAfterClip()
    {
        yield return new WaitForSeconds(clip.length);
        Destroy(gameObject);
    }
}
