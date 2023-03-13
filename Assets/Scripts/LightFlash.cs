using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlash : MonoBehaviour
{

    private Light light;

    [Header("Strike timing")]
    [Tooltip("Minimal value for the delay between strikes")]
    public float minStrikeDelay = 5;
    [Tooltip("Maximal value for the delay between strikes")]
    public float maxStrikeDelay = 10;
    [Tooltip("Minimal value for the delay between flashes")]
    public float minFlashDelay = 0.5f;
    [Tooltip("Maximal value for the delay between flashes")]
    public float maxFlashDelay = 1.0f;
    [Tooltip("Minimal value for the length of a flash")]
    public float minFlashLinger = 0.1f;
    [Tooltip("Maximal value for the length of a flash")]
    public float maxFlashLinger = 0.5f;
    [Tooltip("Minimal number of flashes in a strike")]
    public int minNumberOfFlashes = 1;
    [Tooltip("Maximal number of flashes in a strike")]
    public int maxNumberOfFlashes = 4;
    [Tooltip("Flash light intensity")]
    public float lightIntensity = 1.0f;
    [Tooltip("Particle system for flashes")]
    public ParticleSystem particleSystem;
    [Tooltip("Audio source for strike sounds")]
    public AudioSource audioSource;

    private void OnEnable()
    {
        light = GetComponent<Light>();
        light.enabled = false;
        light.intensity = lightIntensity;
        StartCoroutine(FlashCourutine());
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator FlashCourutine()
    {
        while(enabled)
        {
            yield return new WaitForSeconds(Random.Range(minStrikeDelay, maxStrikeDelay));

            
            var numberOfFlashes = Random.Range(minNumberOfFlashes, maxNumberOfFlashes);

            for (int i = 0; i < numberOfFlashes; i++)
            {
                if (audioSource != null)
                    audioSource.Play();
                particleSystem.Play();

                light.enabled = true;

                yield return new WaitForSeconds(Random.Range(minFlashLinger, maxFlashLinger));

                light.enabled = false;
     
                yield return new WaitForSeconds(Random.Range(minFlashDelay, maxFlashDelay));
            }
        }
    }
}
