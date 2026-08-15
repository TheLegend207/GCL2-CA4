using UnityEngine;


//DO NOT USE THIS, will be removed once guns and bullets are working properly and tested
//this script has been deprecated, left for reference in case of bugs, playershoot and bulletclass are the new gun scripts
//this script uses 4 separate prefabs each with their own tag and damage/mechanics are based on tags instead of a bulletclass

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
