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

    public bool IsCoolingDown // bool for if the gun is reloading
    {
        get { return isCoolingDown; }
    }

    public int BulletsFiredSinceCooldown // track bullets fired to calculate bullets left in 1 mag
    {
        get { return bulletsFiredSinceCooldown; }
    }

    private void Awake()
    {
        audioSource =
            GetComponent<AudioSource>(); //collect the audio from audio source

        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (!pistol && //if al 4 guns are null
            !smg &&
            !sniper &&
            !grenadelauncher)
        {
            pistol = true; //set pistol as base gun
        }

        UpdateAmmoCounter(); //update gun statistics and connect them to UI
        ResetBulletLimit(); 
        UpdateAmmoUI();
    }

    private void Update()
    {
        // Press R to manually reload/start the cooldown.
        if (Input.GetKeyDown(reloadKey))
        {
            BeginCooldown(); //begin reload
        }

        if (!isCoolingDown) //if not on reload
        {
            if (IsSMG())
            {
                HandleSMGFire(); //smg hold to fire 
            }
            else
            {
                HandleSingleShotFire(); //click to fire once
            }
        }
        if (Input.GetKeyDown(shoveKey) && shoveCooldown == false) //if shove key is pressed and not on shove cooldown
        {
            StartCoroutine(Shove()); //shove
        }

        UpdateAmmoCounter();
        UpdateAmmoUI();
    }

    private void HandleSingleShotFire()
    {
        if (Input.GetButtonDown("Shoot")) //called once only when button is pressed down (down behind)
        {
            TryShoot();
        }
    }

    private void HandleSMGFire()
    {
        if (Input.GetButton("Shoot")) //continuously called when button is held (no down behind)
        {
            TryShoot();
        }
    }

    private void TryShoot() //attempt to shoot
    {
        if (isCoolingDown) //if on reload
        {
            return; 
        }

        if (shootPoint == null) //if no shootpoint is assigned
        {
            Debug.LogWarning(
                "PlayerShoot: Shoot Point is not assigned."
            );
            return;
        }

        if (IsGL()) //if current gun is GL
        {
            if (grenadeprefab == null) //no grenade prefab
            {
                Debug.LogWarning(
                    "PlayerShoot: Grenade Prefab is not assigned."
                );
                return;
            }
        }
        else if (bulletprefab == null) //if current gun uses bullet prefab
        {
            Debug.LogWarning(
                "PlayerShoot: Bullet Prefab is not assigned."
            );
            return; //no bullet prefab
        }

        if (Time.time < nextShotTime) //if time passed is less than next shot time
        {
            return;
        }

        if (!infiniteAmmo &&
            GetCurrentAmmo() <= 0)
        {
            UpdateAmmoUI(); //update current ammo, UI, limit
            return;
        }

        if (ReachedBulletLimit()) //when out of ammo
        {
            BeginCooldown(); //reload
            return;
        }

        UpdateBullet(); //updates bulletclass before firing

        if (IsGL())
        {
            PlayGrenadeSound(); //grenade shooting sound
        }
        else
        {
            PlayShootingSound(); //normal bullet sound
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
        if (IsSMG()) //set configuration for smg
        {
            projectileSpeed = 50f;

            ConfigureBullet(
                damage: 20,
                pierce: 0,
                slow: 2f
            );

            ShootBullet();
        }
        else if (IsPistol()) //set configuration for pistol
        {
            projectileSpeed = 50f;

            ConfigureBullet(
                damage: 40,
                pierce: 0,
                slow: 4.5f
            );

            ShootBullet(); //shoot bullet prefab
        }
        else if (IsSniper()) //set configuration for sniper
        {
            projectileSpeed = 50f;

            ConfigureBullet(
                damage: 100,
                pierce: 3,
                slow: 3f
            );

            ShootBullet();
        }
        else if (IsGL()) //set configuration for GL
        {
            projectileSpeed = 40f;

            ConfigureBullet(
                damage: 100,
                pierce: 0,
                slow: 0f
            );

            ShootGrenade(); //shoot grenade prefab instead of bullet
        }
    }

    private void ConfigureBullet( //how to configure bulletclass when referenced
        int damage,
        int pierce,
        float slow
    )
    {
        if (bulletclass == null) //no bulletclass assigned
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

    private void ShootBullet() //shooting of the bullet
    {
        GameObject projectile =
            Instantiate(
                bulletprefab,
                shootPoint.position,
                shootPoint.rotation
            );

        Rigidbody rb =
            projectile.GetComponent<Rigidbody>(); //get bullets rigidbody

        if (rb != null)
        {
            rb.linearVelocity =
                shootPoint.forward *
                projectileSpeed;
        }
        else //warning of no rigidbody
        {
            Debug.LogWarning(
                "PlayerShoot: Bullet prefab has no Rigidbody."
            );
        }
    }

    private void ShootGrenade() //firing grenade instead of bullet prefab
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
        if (IsSMG()) //setting delay per shot for smg
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
        else if (IsPistol()) //setting base delay per shot for pistol
        {
            nextShotTime =
                Time.time +
                pistolFireDelay;
        }
        else if (IsSniper()) //base delay for sniper shots
        {
            nextShotTime =
                Time.time +
                sniperFireDelay;
        }
        else if (IsGL()) //base delay for grenade shots
        {
            nextShotTime =
                Time.time +
                grenadeFireDelay;
        }
    }

    private bool IsSMG() //test if smg is currently equipped
    {
        return smg &&
               !pistol &&
               !sniper &&
               !grenadelauncher;
    }

    private bool IsPistol() //test if pistol is currently equipped
    {
        return pistol &&
               !smg &&
               !sniper &&
               !grenadelauncher;
    }

    private bool IsSniper() //test if sniper is currently equipped
    {
        return sniper &&
               !pistol &&
               !smg &&
               !grenadelauncher;
    }

    private bool IsGL() //test if GL is currently equipped
    {
        return grenadelauncher &&
               !pistol &&
               !sniper &&
               !smg;
    }

    private int GetCurrentAmmo() //update current ammo for each gun
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

    private int GetCurrentBulletLimit() //update max ammo per mag for each gun
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

    private bool ReachedBulletLimit() //ran out of bullets
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

    private void BeginCooldown() //reloading script
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

    private IEnumerator CooldownRoutine() //coroutine for reloading
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

    private void ResetBulletLimit() //setting current ammo back to max ammo
    {
        bulletsFiredSinceCooldown = 0;
        isCoolingDown = false;
        nextShotTime = 0f;
    }

    private void ReduceCurrentAmmo() //using up current ammo when shot from the gun
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

    private void UpdateAmmoCounter() //update ammo
    {
        ammocounter =
            GetCurrentAmmo();
    }

    private void UpdateAmmoUI() //update ammo ui on player screen
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

    private void PlayShootingSound() //sound manager for firing guns
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

    public void SetWeaponType( //linking new weapon to the base weapon
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

    public void ForceCooldown() //forced reload when 0 ammo
    {
        BeginCooldown();
    }
    IEnumerator Shove() //shoving script
    {
        shoveCooldown = true;
        shovehitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        shovehitbox.SetActive(false);
        yield return new WaitForSeconds(shoveCooldownTime);
        shoveCooldown = false;
    }
}