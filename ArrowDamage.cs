using UnityEngine;

public class ArrowDamage : MonoBehaviour
{
    public int damage = 1; // The amount of damage the arrow will do

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the arrow hit a patroller
        if (collision.gameObject.CompareTag("AI"))
        {
            // Get the Patroller2 script and call the TakeDamage function
            Patroller2 patroller = collision.gameObject.GetComponent<Patroller2>();
            if (patroller != null)
            {
                patroller.TakeDamage(damage);
            }

            // Destroy the arrow after it hits something
            Destroy(gameObject);
        }
    }
}
