using UnityEngine;

public class BulletClass : MonoBehaviour
{
    public int damage;
    public float slow;
    public int pierce;

    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))
        {
            if (pierce <= 0)
            {
                Destroy(gameObject);
                Debug.Log("Bullet destroyed on zombie.");
            }
            pierce = -1;
        }

       if (other.CompareTag("Environment"))
        {
            Destroy(gameObject);
            Debug.Log("Bullet destroyed on environment.");
        }
    }
}
