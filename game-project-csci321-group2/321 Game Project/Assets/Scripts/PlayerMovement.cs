using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float lookSpeed = 12f;
    public int health = 10; // Add a health variable to track player's health
    public GameObject startPoint; // Add a reference to the start point game object
    public HealthBar healthBar;

    public Cinemachine.CinemachineFreeLook normalCamera;
    public Cinemachine.CinemachineVirtualCamera aimCamera;
    public GameObject arrowObj;
    public Transform arrowPos;

    private Animator anim;
    private bool IsAiming;
    public static event Action OnPlayerRespawn; // Add this event


    void Start()
    {
        healthBar.SetMaxHealth(health);
        IsAiming = false;

        anim = GetComponent<Animator>();
    }

    void Update()
    {

        if (!IsAiming && Input.GetKey("left shift") && Input.GetAxis("Vertical") != 0f) // Run
        {
            Running();
        }
        else if (!IsAiming && Input.GetAxis("Vertical") != 0f) // Walk Forward
        {
            Walking();
        }
        else if (Input.GetMouseButton(1))
        {
            Aiming();
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
        anim.SetFloat("Speed", Input.GetAxis("Vertical"), 0.2f, Time.deltaTime);
        rotatePlayer(20f);
    }

    private void Idle()
    {
        anim.SetBool("AimStart", false);
        anim.SetBool("Shoot", false);
        anim.SetFloat("Speed", 0f, 0.3f, Time.deltaTime);
        IsAiming = false;
        transform.Rotate(0f, 0f, 0f);
        normalCamera.enabled = true;
        aimCamera.enabled = false;
        rotatePlayer(20f);
    }

    private void Aiming()
    {
        if (!IsAiming)
        {
            IsAiming = true;
            anim.SetBool("AimStart", true);
        }
        normalCamera.enabled = false;
        aimCamera.enabled = true;

        float rotateY = Input.GetAxis("Mouse X");
        transform.Rotate(0f, rotateY, 0f);

        if (Input.GetMouseButton(0))
        {
            anim.SetBool("AimStart", false);
            anim.SetBool("Shoot", true);
        }
    }

    public void Shoot()
    {
        GameObject arrow = Instantiate(arrowObj, arrowPos.position, transform.rotation);
        arrow.GetComponent<Rigidbody>().AddForce(transform.forward * 25f, ForceMode.Impulse);

        // Destroy the arrow 3 seconds after it has been shot
        Destroy(arrow, 3f);
    }


    private void rotatePlayer(float sharpness)
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float rotate = (moveHorizontal * lookSpeed) / sharpness;
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
