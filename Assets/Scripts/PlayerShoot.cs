using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Existing references")]
    public BulletClass bulletclass;
    public GameObject bulletprefab;

    [Header("Weapon type")]
    public bool pistol;
    public bool smg;
    public bool sniper;
    public bool grenadelauncher;

    [Header("Ammo")]
    public int ammocounter;
    public int pistolammo = 30;
    public int smgammo = 120;
    public int sniperammo = 30;
    public int grenadeammo = 10;

    [Header("Shooting")]
    public Transform shootPoint;
    public float projectileSpeed = 50f;

    [Header("SMG settings")]
    public float smgRoundsPerSecond = 12f;

    [Header("Optional")]
    public bool infiniteAmmo = false;

    private float nextShotTime;

    private void Start()
    {
        // If no weapon type was assigned, start with the pistol.
        if (!pistol && !smg && !sniper && !grenadelauncher)
        {
            pistol = true;
        }

        UpdateAmmoCounter();
    }

    private void Update()
    {
        if (IsSMG())
        {
            HandleSMGFire();
        }
        else
        {
            HandleSingleShotFire();
        }

        UpdateAmmoCounter();
    }

    private void HandleSingleShotFire()
    {
        if (Input.GetButtonDown("Shoot"))
        {
            TryShoot();
        }
    }

    private void HandleSMGFire()
    {
        // GetButton remains true while the Shoot button is held.
        if (Input.GetButton("Shoot"))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (shootPoint == null)
        {
            Debug.LogWarning("PlayerShoot: Shoot Point is not assigned.");
            return;
        }

        if (bulletprefab == null)
        {
            Debug.LogWarning("PlayerShoot: Bullet Prefab is not assigned.");
            return;
        }

        if (Time.time < nextShotTime)
        {
            return;
        }

        if (!infiniteAmmo && GetCurrentAmmo() <= 0)
        {
            return;
        }

        Shoot();

        if (!infiniteAmmo)
        {
            ReduceCurrentAmmo();
        }

        if (IsSMG())
        {
            nextShotTime = Time.time + (1f / smgRoundsPerSecond);
        }
        else
        {
            // Small delay prevents accidental repeated shots.
            nextShotTime = Time.time + 0.15f;
        }
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(
            bulletprefab,
            shootPoint.position,
            shootPoint.rotation
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                shootPoint.forward * projectileSpeed;
        }
        else
        {
            Debug.LogWarning(
                "PlayerShoot: Bullet prefab has no Rigidbody."
            );
        }
    }

    private bool IsSMG()
    {
        return smg &&
               !pistol &&
               !sniper &&
               !grenadelauncher;
    }

    private int GetCurrentAmmo()
    {
        if (pistol)
            return pistolammo;

        if (smg)
            return smgammo;

        if (sniper)
            return sniperammo;

        if (grenadelauncher)
            return grenadeammo;

        return 0;
    }

    private void ReduceCurrentAmmo()
    {
        if (pistol)
        {
            pistolammo = Mathf.Max(0, pistolammo - 1);
        }
        else if (smg)
        {
            smgammo = Mathf.Max(0, smgammo - 1);
        }
        else if (sniper)
        {
            sniperammo = Mathf.Max(0, sniperammo - 1);
        }
        else if (grenadelauncher)
        {
            grenadeammo = Mathf.Max(0, grenadeammo - 1);
        }
    }

    private void UpdateAmmoCounter()
    {
        ammocounter = GetCurrentAmmo();
    }

    public void SetWeaponType(
        bool newPistol,
        bool newSmg,
        bool newSniper,
        bool newGrenadeLauncher,
        GameObject newBulletPrefab,
        float newProjectileSpeed
    )
    {
        pistol = newPistol;
        smg = newSmg;
        sniper = newSniper;
        grenadelauncher = newGrenadeLauncher;

        if (newBulletPrefab != null)
        {
            bulletprefab = newBulletPrefab;
        }

        if (newProjectileSpeed > 0f)
        {
            projectileSpeed = newProjectileSpeed;
        }

        nextShotTime = 0f;
        UpdateAmmoCounter();
    }
}