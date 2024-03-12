using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound_TriggerZones : MonoBehaviour
{
    [SerializeField] AudioSource source;
    public AudioClip clip;

    [Header("Trigger Parameters")]
    public bool PlayOnce;
    public float volume;
    public float minDistance = 3f;
    public float maxDistance = 10f;
    public float spatialBlend;
    

    void Start()
    {
        source.GetComponent<AudioSource>();
        source.clip = clip;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.spatialBlend = spatialBlend;
        source.volume = volume;

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && !source.isPlaying){

            source.PlayOneShot(clip);

            if (PlayOnce)
            {
                StartCoroutine(DestroyAfterClip());
            }

        }
    }

    private IEnumerator DestroyAfterClip()
    {
        yield return new WaitForSeconds(clip.length);
        Destroy(gameObject);
    }
}
