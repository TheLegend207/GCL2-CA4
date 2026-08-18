using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerShoot : MonoBehaviour
{
    [Header("Existing references")]
    public BulletClass bulletclass;
    public GameObject bulletprefab;
    public GameObject grenadeprefab;
    public GameObject shovehitbox;

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
    public int grenadeammo = 5;

    [Header("Shooting")]
    public Transform shootPoint;
    public float projectileSpeed = 50f;

    [Header("Weapon fire rates")]
    public float pistolFireDelay = 0.2f;
    public float smgRoundsPerSecond = 12f;
    public float sniperFireDelay = 1f;
    public float grenadeFireDelay = 2f;

    [Header("Bullet limit per weapon")]
    public int pistolBulletLimit = 12;
    public int smgBulletLimit = 30;
    public int sniperBulletLimit = 5;
    public int grenadeBulletLimit = 1;

    [Header("Cooldown / reload")]
    public float cooldownDuration = 2f;
    public bool cooldownAutomatically = true;
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode shoveKey = KeyCode.Mouse1;
    public bool shoveCooldown;
    public float shoveCooldownTime = 1.5f;

    [Header("Ammo UI")]
    public TMP_Text ammoText;

    [Tooltip("The first value is bullets remaining. The second value is infinity.")]
    public string ammoTextFormat = "Ammo: {0} / ∞";

    [Tooltip("Text shown while reloading.")]
    public string cooldownText = "Reloading...";

    [Header("Shooting sound")]
    public AudioClip shootingSound;

    [Range(0f, 1f)]
    public float shootingVolume = 1f;

    [Header("Cooldown sound")]
    public AudioClip cooldownSound;

    [Range(0f, 1f)]
    public float cooldownVolume = 1f;

    [Header("Grenade sound")]
    public AudioClip grenadeSound;

    [Range(0f, 1f)]
    public float grenadeVolume = 1f;

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
        audioSource =
            GetComponent<AudioSource>();

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
        // Press R to manually reload/start the cooldown.
        if (Input.GetKeyDown(reloadKey))
        {
            BeginCooldown();
        }

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
        if (Input.GetKeyDown(shoveKey) && shoveCooldown == false)
        {
            StartCoroutine(Shove());
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

        if (IsGL())
        {
            if (grenadeprefab == null)
            {
                Debug.LogWarning(
                    "PlayerShoot: Grenade Prefab is not assigned."
                );
                return;
            }
        }
        else if (bulletprefab == null)
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

        UpdateBullet();

        if (IsGL())
        {
            PlayGrenadeSound();
        }
        else
        {
            PlayShootingSound();
        }

        bulletsFiredSinceCooldown++;

        if (!infiniteAmmo)
        {
            ReduceCurrentAmmo();
        }

        SetNextShotTime();

        UpdateAmmoCounter();
        UpdateAmmoUI();

        if (ReachedBulletLimit() &&
            cooldownAutomatically)
        {
            BeginCooldown();
        }
    }

    private void UpdateBullet()
    {
        if (IsSMG())
        {
            projectileSpeed = 50f;

            ConfigureBullet(
                damage: 20,
                pierce: 0,
                slow: 2f
            );

            ShootBullet();
        }
        else if (IsPistol())
        {
            projectileSpeed = 50f;

            ConfigureBullet(
                damage: 40,
                pierce: 0,
                slow: 4.5f
            );

            ShootBullet();
        }
        else if (IsSniper())
        {
            projectileSpeed = 50f;

            ConfigureBullet(
                damage: 100,
                pierce: 2,
                slow: 3f
            );

            ShootBullet();
        }
        else if (IsGL())
        {
            projectileSpeed = 40f;

            ConfigureBullet(
                damage: 100,
                pierce: 0,
                slow: 0f
            );

            ShootGrenade();
        }
    }

    private void ConfigureBullet(
        int damage,
        int pierce,
        float slow
    )
    {
        if (bulletclass == null)
        {
            Debug.LogWarning(
                "PlayerShoot: BulletClass is not assigned."
            );
            return;
        }

        bulletclass.damage = damage;
        bulletclass.pierce = pierce;
        bulletclass.slow = slow;
    }

    private void ShootBullet()
    {
        GameObject projectile =
            Instantiate(
                bulletprefab,
                shootPoint.position,
                shootPoint.rotation
            );

        Rigidbody rb =
            projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                shootPoint.forward *
                projectileSpeed;
        }
        else
        {
            Debug.LogWarning(
                "PlayerShoot: Bullet prefab has no Rigidbody."
            );
        }
    }

    private void ShootGrenade()
    {
        GameObject projectile =
            Instantiate(
                grenadeprefab,
                shootPoint.position,
                shootPoint.rotation
            );

        Rigidbody rb =
            projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                shootPoint.forward *
                projectileSpeed;
        }
        else
        {
            Debug.LogWarning(
                "PlayerShoot: Grenade prefab has no Rigidbody."
            );
        }
    }

    private void SetNextShotTime()
    {
        if (IsSMG())
        {
            float safeRoundsPerSecond =
                Mathf.Max(
                    0.01f,
                    smgRoundsPerSecond
                );

            nextShotTime =
                Time.time +
                (1f / safeRoundsPerSecond);
        }
        else if (IsPistol())
        {
            nextShotTime =
                Time.time +
                pistolFireDelay;
        }
        else if (IsSniper())
        {
            nextShotTime =
                Time.time +
                sniperFireDelay;
        }
        else if (IsGL())
        {
            nextShotTime =
                Time.time +
                grenadeFireDelay;
        }
    }

    private bool IsSMG()
    {
        return smg &&
               !pistol &&
               !sniper &&
               !grenadelauncher;
    }

    private bool IsPistol()
    {
        return pistol &&
               !smg &&
               !sniper &&
               !grenadelauncher;
    }

    private bool IsSniper()
    {
        return sniper &&
               !pistol &&
               !smg &&
               !grenadelauncher;
    }

    private bool IsGL()
    {
        return grenadelauncher &&
               !pistol &&
               !sniper &&
               !smg;
    }

    private int GetCurrentAmmo()
    {
        if (IsPistol())
        {
            return pistolammo;
        }

        if (IsSMG())
        {
            return smgammo;
        }

        if (IsSniper())
        {
            return sniperammo;
        }

        if (IsGL())
        {
            return grenadeammo;
        }

        return 0;
    }

    private int GetCurrentBulletLimit()
    {
        if (IsPistol())
        {
            return pistolBulletLimit;
        }

        if (IsSMG())
        {
            return smgBulletLimit;
        }

        if (IsSniper())
        {
            return sniperBulletLimit;
        }

        if (IsGL())
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

        return bulletsFiredSinceCooldown >=
               currentLimit;
    }

    private void BeginCooldown()
    {
        if (isCoolingDown)
        {
            return;
        }

        if (cooldownCoroutine != null)
        {
            StopCoroutine(
                cooldownCoroutine
            );
        }

        cooldownCoroutine =
            StartCoroutine(
                CooldownRoutine()
            );
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;

        PlayCooldownSound();
        UpdateAmmoUI();

        float safeCooldownDuration =
            Mathf.Max(
                0f,
                cooldownDuration
            );

        yield return new WaitForSeconds(
            safeCooldownDuration
        );

        bulletsFiredSinceCooldown = 0;
        nextShotTime = 0f;
        isCoolingDown = false;
        cooldownCoroutine = null;

        UpdateAmmoCounter();
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
        if (IsPistol())
        {
            pistolammo =
                Mathf.Max(
                    0,
                    pistolammo - 1
                );
        }
        else if (IsSMG())
        {
            smgammo =
                Mathf.Max(
                    0,
                    smgammo - 1
                );
        }
        else if (IsSniper())
        {
            sniperammo =
                Mathf.Max(
                    0,
                    sniperammo - 1
                );
        }
        else if (IsGL())
        {
            grenadeammo =
                Mathf.Max(
                    0,
                    grenadeammo - 1
                );
        }
    }

    private void UpdateAmmoCounter()
    {
        ammocounter =
            GetCurrentAmmo();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null)
        {
            return;
        }

        if (isCoolingDown)
        {
            ammoText.text =
                cooldownText;

            return;
        }

        int bulletsRemaining =
            Mathf.Max(
                0,
                GetCurrentBulletLimit() -
                bulletsFiredSinceCooldown
            );

        ammoText.text =
            string.Format(
                ammoTextFormat,
                bulletsRemaining
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

    private void PlayGrenadeSound()
    {
        if (grenadeSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            grenadeSound,
            grenadeVolume
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
        pistol =
            newPistol;

        smg =
            newSmg;

        sniper =
            newSniper;

        grenadelauncher =
            newGrenadeLauncher;

        if (newBulletPrefab != null)
        {
            bulletprefab =
                newBulletPrefab;
        }

        if (newProjectileSpeed > 0f)
        {
            projectileSpeed =
                newProjectileSpeed;
        }

        if (cooldownCoroutine != null)
        {
            StopCoroutine(
                cooldownCoroutine
            );

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
    IEnumerator Shove()
    {
        shoveCooldown = true;
        shovehitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        shovehitbox.SetActive(false);
        yield return new WaitForSeconds(shoveCooldownTime);
        shoveCooldown = false;
    }
}