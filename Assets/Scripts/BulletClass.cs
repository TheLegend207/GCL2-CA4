using UnityEngine;

public class BulletClass : MonoBehaviour
{
    public int damage;
    public float slow;
    public int pierce;

    void Start()
    {
    }

    private void OnTriggerEnter(Collider other) // when colliding with another object
    {
        if (other.CompareTag("Zombie")) // if tag of other object is zombie
        {
            if (pierce <= 0) //if piercing is equals or less than 0
            {
                Destroy(gameObject); // destroy bullet
                Debug.Log("Bullet destroyed on zombie.");
            }
            pierce = -1; // bullet -1 pierce, for sniper rifle piercing
        }

       if (other.CompareTag("Environment")) // when colliding with environment tage
        {
            Destroy(gameObject); // destroy bullet
            Debug.Log("Bullet destroyed on environment.");
        }
    }
}
