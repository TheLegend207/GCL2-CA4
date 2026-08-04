using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject pistolbullet;
    public GameObject smgbullet;
    public GameObject sniperbullet;
    public GameObject grenade;
    public Transform shootPoint;
    public float projectileSpeed;
    public bool pistol;
    public bool smg;
    public bool sniper;
    public bool grenadelauncher;
    public int pistolammo;
    public int sniperammo;
    public int smgammo;
    public int grenadeammo;
    void Start()
    {
        pistol = true;
        smg = false;
        sniper = false;
        grenadelauncher = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Shoot"))
        {
            Shoot();
        }
    }
    void Shoot()
    {
        GameObject projectile = Instantiate(pistolbullet, shootPoint.position, shootPoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = shootPoint.forward * projectileSpeed;
    }
}
