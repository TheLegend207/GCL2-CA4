using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public BulletClass bulletclass;
    public GameObject bulletprefab;
    public bool pistol;
    public bool smg;
    public bool sniper;
    public bool grenadelauncher;
    public int ammocounter;
    public int pistolammo;
    public int smgammo;
    public int sniperammo;
    public int grenadeammo;
    public Transform shootPoint;
    public float projectileSpeed;
    //above 4 bool waiting for a code that updates currently held weapon
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Shoot"))
        {
            Shoot();
        }

//        ChangeGun();
    }
    void Shoot()
    {
        GameObject projectile = Instantiate(bulletprefab, shootPoint.position, shootPoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = shootPoint.forward * projectileSpeed;
    }
    void ChangeGun()
    {
        if (pistol == true)
        ammocounter = pistolammo;
        if (smg == true)
        ammocounter = smgammo;
        if (sniper == true)
        ammocounter = sniperammo;
        if (grenadelauncher == true)
        ammocounter = grenadeammo;
        //to be optimised and shortened when gun change is implemented
        //optimise by referencing this in gun model change script (probably player controller)
    }

}
