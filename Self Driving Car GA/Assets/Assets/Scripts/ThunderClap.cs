using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderClap : MonoBehaviour
{
    bool canFlicker = true;

    Light thunderLight;
    public AudioClip audioClip;

    private void Awake()
    {
        thunderLight = this.GetComponent<Light>();
    }

    void Update()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        if (canFlicker)
        {
            canFlicker = false;

            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.PlayOneShot(audioClip);
            thunderLight.enabled = true;

            yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            thunderLight.enabled = false;

            yield return new WaitForSeconds(Random.Range(0.1f, 4f));
            thunderLight.enabled = true;
            canFlicker = true;
        }
    }
}
