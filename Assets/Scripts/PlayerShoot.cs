using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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

    [Header("Ammo reserve")]
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

    [Header("Bullet limit per weapon")]
    public int pistolBulletLimit = 12;
    public int smgBulletLimit = 30;
    public int sniperBulletLimit = 5;
    public int grenadeBulletLimit = 1;

    [Header("Cooldown")]
    public float cooldownDuration = 2f;
    public bool cooldownAutomatically = true;

    [Header("Ammo UI")]
    public TMP_Text ammoText;

    [Tooltip("Example: Ammo: 30 / 120")]
    public string ammoTextFormat = "Ammo: {0} / {1}";

    [Tooltip("Text shown while the weapon is cooling down.")]
    public string cooldownText = "Cooling Down...";

    [Header("Shooting sound")]
    public AudioClip shootingSound;

    [Range(0f, 1f)]
    public float shootingVolume = 1f;

    [Header("Cooldown sound")]
    public AudioClip cooldownSound;

    [Range(0f, 1f)]
    public float cooldownVolume = 1f;

    [Header("Optional")]
    public bool infiniteAmmo = false;

    private float nextShotTime;
    private int bulletsFiredSinceCooldown;
    private bool isCoolingDown;
    private AudioSource audioSource;
    private Coroutine cooldownCoroutine;

    public bool IsCoolingDown
    {
        get { return isCoolingDown; }
    }

    public int BulletsFiredSinceCooldown
    {
        get { return bulletsFiredSinceCooldown; }
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (!pistol &&
            !smg &&
            !sniper &&
            !grenadelauncher)
        {
            pistol = true;
        }

        UpdateAmmoCounter();
        ResetBulletLimit();
        UpdateAmmoUI();
    }

    private void Update()
    {
        if (!isCoolingDown)
        {
            if (IsSMG())
            {
                HandleSMGFire();
            }
            else
            {
                HandleSingleShotFire();
            }
        }

        UpdateAmmoCounter();
        UpdateAmmoUI();
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
        if (Input.GetButton("Shoot"))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (isCoolingDown)
        {
            return;
        }

        if (shootPoint == null)
        {
            Debug.LogWarning(
                "PlayerShoot: Shoot Point is not assigned."
            );
            return;
        }

        if (bulletprefab == null)
        {
            Debug.LogWarning(
                "PlayerShoot: Bullet Prefab is not assigned."
            );
            return;
        }

        if (Time.time < nextShotTime)
        {
            return;
        }

        if (!infiniteAmmo &&
            GetCurrentAmmo() <= 0)
        {
            UpdateAmmoUI();
            return;
        }

        if (ReachedBulletLimit())
        {
            BeginCooldown();
            return;
        }

        Shoot();
        PlayShootingSound();

        bulletsFiredSinceCooldown++;

        if (!infiniteAmmo)
        {
            ReduceCurrentAmmo();
        }

        if (IsSMG())
        {
            if (smgRoundsPerSecond <= 0f)
            {
                smgRoundsPerSecond = 1f;
            }

            nextShotTime =
                Time.time + (1f / smgRoundsPerSecond);
        }
        else
        {
            nextShotTime =
                Time.time + 0.15f;
        }

        UpdateAmmoUI();

        if (ReachedBulletLimit() &&
            cooldownAutomatically)
        {
            BeginCooldown();
        }
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(
            bulletprefab,
            shootPoint.position,
            shootPoint.rotation
        );

        Rigidbody rb =
            projectile.GetComponent<Rigidbody>();

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
        {
            return pistolammo;
        }

        if (smg)
        {
            return smgammo;
        }

        if (sniper)
        {
            return sniperammo;
        }

        if (grenadelauncher)
        {
            return grenadeammo;
        }

        return 0;
    }

    private int GetCurrentBulletLimit()
    {
        if (pistol)
        {
            return pistolBulletLimit;
        }

        if (smg)
        {
            return smgBulletLimit;
        }

        if (sniper)
        {
            return sniperBulletLimit;
        }

        if (grenadelauncher)
        {
            return grenadeBulletLimit;
        }

        return 0;
    }

    private bool ReachedBulletLimit()
    {
        int currentLimit =
            GetCurrentBulletLimit();

        if (currentLimit <= 0)
        {
            return false;
        }

        return bulletsFiredSinceCooldown >= currentLimit;
    }

    private void BeginCooldown()
    {
        if (isCoolingDown)
        {
            return;
        }

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        cooldownCoroutine =
            StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;

        PlayCooldownSound();
        UpdateAmmoUI();

        float safeCooldownDuration =
            Mathf.Max(0f, cooldownDuration);

        yield return new WaitForSeconds(
            safeCooldownDuration
        );

        bulletsFiredSinceCooldown = 0;
        nextShotTime = 0f;
        isCoolingDown = false;
        cooldownCoroutine = null;

        UpdateAmmoUI();
    }

    private void ResetBulletLimit()
    {
        bulletsFiredSinceCooldown = 0;
        isCoolingDown = false;
        nextShotTime = 0f;
    }

    private void ReduceCurrentAmmo()
    {
        if (pistol)
        {
            pistolammo =
                Mathf.Max(0, pistolammo - 1);
        }
        else if (smg)
        {
            smgammo =
                Mathf.Max(0, smgammo - 1);
        }
        else if (sniper)
        {
            sniperammo =
                Mathf.Max(0, sniperammo - 1);
        }
        else if (grenadelauncher)
        {
            grenadeammo =
                Mathf.Max(0, grenadeammo - 1);
        }
    }

    private void UpdateAmmoCounter()
    {
        ammocounter = GetCurrentAmmo();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null)
        {
            return;
        }

        if (isCoolingDown)
        {
            ammoText.text = cooldownText;
            return;
        }

        int currentAmmo = GetCurrentAmmo();
        int currentLimit = GetCurrentBulletLimit();

        if (infiniteAmmo)
        {
            ammoText.text =
                string.Format(
                    ammoTextFormat,
                    "∞",
                    currentLimit
                );

            return;
        }

        ammoText.text =
            string.Format(
                ammoTextFormat,
                currentAmmo,
                currentLimit
            );
    }

    private void PlayShootingSound()
    {
        if (shootingSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            shootingSound,
            shootingVolume
        );
    }

    private void PlayCooldownSound()
    {
        if (cooldownSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            cooldownSound,
            cooldownVolume
        );
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
        grenadelauncher =
            newGrenadeLauncher;

        if (newBulletPrefab != null)
        {
            bulletprefab = newBulletPrefab;
        }

        if (newProjectileSpeed > 0f)
        {
            projectileSpeed =
                newProjectileSpeed;
        }

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        isCoolingDown = false;
        bulletsFiredSinceCooldown = 0;
        nextShotTime = 0f;

        UpdateAmmoCounter();
        UpdateAmmoUI();
    }

    public void ForceCooldown()
    {
        BeginCooldown();
    }
}