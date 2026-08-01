using System.Collections;
using UnityEngine;

public class ShootPinkiePie : MonoBehaviour
{

    public GameObject pinkiePie;
    public Transform firePoint; //Where the pony spawns
    public float shootInterval; //How long it takes to shoot between shots

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        while (true) //Shoot forever
        {
            Shoot(); //Spawns pony
            yield return new WaitForSeconds(shootInterval); //Wait the shootInterval time before shooting again
        }
    }

    void Shoot()
    {
        GameObject spawnedPinkie = Instantiate(pinkiePie, firePoint.position, firePoint.rotation); //Create a copy of the pinkie pie prefab at firePoint's transformation and rotation
        Rigidbody rb = spawnedPinkie.GetComponent<Rigidbody>(); //Get pinkie pie's rigidbody
        rb.AddForce(firePoint.forward * 500f, ForceMode.Impulse); //To apply force instantly to pinkie pie so that she launch forward

        Destroy(spawnedPinkie, 15f); //To prevent infinite amount of pinkie pie lol
    }
}
