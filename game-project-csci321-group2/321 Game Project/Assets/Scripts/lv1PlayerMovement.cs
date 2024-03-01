using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class lv1PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 15f;
    public int health = 10; // Add a health variable to track player's health
    public GameObject startPoint; // Add a reference to the start point game object
    public HealthBar healthBar;
    private Animator anim;
    public static event Action OnPlayerRespawn; // Add this event


    void Start()
    {
        healthBar.SetMaxHealth(health);

        anim = GetComponent<Animator>();
    }

    void Update()
    {

        if (Input.GetKey("left shift") && Input.GetAxis("Vertical") != 0f) // Run
        {
            Running();
        }
        else if (Input.GetAxis("Vertical") != 0f) // Walk Forward
        {
            Walking();
        }
        else
        {
            Idle();
        }

        // Check for death
        if (health <= 0)
        {
            Die();
        }
    }

    private void Running()
    {
        anim.SetFloat("Speed", 2 * Input.GetAxis("Vertical"), 0.2f, Time.deltaTime);
        rotatePlayer(30f);
    }

    private void Walking()
    {
        anim.SetFloat("Speed", Input.GetAxis("Vertical"), 0.1f, Time.deltaTime);
        rotatePlayer(20f);
    }

    private void Idle()
    {
        anim.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);

        rotatePlayer(20f);
    }

    private void rotatePlayer(float sharpness)
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float rotate = (moveHorizontal * turnSpeed) / sharpness;
        anim.SetFloat("Direction", moveHorizontal);
        transform.Rotate(0f, rotate, 0f);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthBar.SetHealth(health);
        Debug.Log("Player took " + damage + " damage. Remaining health: " + health);

        // If health falls below or equals zero after taking damage, the player dies
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Play the death animation
        anim.SetTrigger("IsDead");

        StartCoroutine(RespawnAfterAnimation());
    }

    private IEnumerator RespawnAfterAnimation()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(2f);
        // Respawn player at the start point
        health = 10; // Reset health to full
        healthBar.SetHealth(health); // Update the health bar
        transform.position = startPoint.transform.position; // Set player's position to the start point's position

        // Reset the death animation parameter so it can play again if needed
        anim.ResetTrigger("IsDead");
        anim.Play("Idle");

        // Invoke the OnPlayerRespawn event after the player has respawned
        OnPlayerRespawn?.Invoke();
    }


}
