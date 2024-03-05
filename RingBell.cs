using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBell : MonoBehaviour
{
    public GameObject bell;
    public AudioClip clip;
    private Animator anim;
    private AudioSource audioSource;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip; // Set the audio clip to play
    }

    private void Update()
    {
        //print(anim.GetParameter(0));
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && Input.GetKey("e"))
        {
            anim.SetBool("IsRung", true);
            audioSource.Play(); // Play the audio clip
            StartCoroutine(ResetIsRungAfterAnimation());

            // Notify the enemy to go to the recently rung bell
            GameObject enemy = GameObject.FindGameObjectWithTag("AI");
            if (enemy != null)
            {
                Patroller patroller = enemy.GetComponent<Patroller>();
                if (patroller != null)
                {
                    patroller.GoToBell(bell.transform);
                }
            }
        }
    }

    private IEnumerator ResetIsRungAfterAnimation()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        anim.SetBool("IsRung", false);
    }
}
