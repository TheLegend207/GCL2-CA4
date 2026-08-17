using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeaponManager : MonoBehaviour
{
    [Header("References")]
    public WeaponViewModelDisplay weaponViewModelDisplay;

    [Header("Pickup settings")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("Weapon swap sound")]
    public AudioClip weaponSwapSound;

    [Range(0f, 1f)]
    public float weaponSwapVolume = 1f;

    private readonly List<WeaponPickup> weaponsInRange =
        new List<WeaponPickup>();

    private WeaponPickup currentWeaponInRange;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Prevent the swap sound from playing automatically.
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (weaponViewModelDisplay == null)
        {
            weaponViewModelDisplay =
                GetComponent<WeaponViewModelDisplay>();
        }

        if (weaponViewModelDisplay == null)
        {
            Debug.LogWarning(
                "PlayerWeaponManager: WeaponViewModelDisplay " +
                "is not assigned."
            );
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            TryPickupWeapon();
        }

        RemoveInvalidPickups();
    }

    public void NotifyWeaponInRange(WeaponPickup weapon)
    {
        if (weapon == null)
        {
            return;
        }

        if (!weaponsInRange.Contains(weapon))
        {
            weaponsInRange.Add(weapon);
        }

        currentWeaponInRange = GetClosestWeapon();
    }

    public void NotifyWeaponOutOfRange(WeaponPickup weapon)
    {
        if (weapon == null)
        {
            return;
        }

        weaponsInRange.Remove(weapon);

        if (currentWeaponInRange == weapon)
        {
            currentWeaponInRange = GetClosestWeapon();
        }
    }

    private void TryPickupWeapon()
    {
        RemoveInvalidPickups();

        if (currentWeaponInRange == null)
        {
            Debug.Log("No weapon is currently in pickup range.");
            return;
        }

        WeaponPickup pickup = currentWeaponInRange;

        if (pickup.viewmodelPrefab == null)
        {
            Debug.LogWarning(
                "WeaponPickup has no viewmodel prefab assigned."
            );
            return;
        }

        if (weaponViewModelDisplay == null)
        {
            Debug.LogWarning(
                "WeaponViewModelDisplay is missing."
            );
            return;
        }

        // Change the visible first-person weapon.
        weaponViewModelDisplay.SetWeaponModel(
            pickup.viewmodelPrefab
        );

        // Update the existing PlayerShoot script.
        UpdatePlayerShoot(pickup);

        // Play the swap sound after the swap succeeds.
        PlayWeaponSwapSound();

        weaponsInRange.Remove(pickup);
        currentWeaponInRange = GetClosestWeapon();

        Destroy(pickup.gameObject);
    }

    private void UpdatePlayerShoot(WeaponPickup pickup)
    {
        PlayerShoot playerShoot =
            GetComponent<PlayerShoot>();

        if (playerShoot == null)
        {
            Debug.LogWarning(
                "PlayerWeaponManager: PlayerShoot was not found."
            );
            return;
        }

        playerShoot.SetWeaponType(
            pickup.pistol,
            pickup.smg,
            pickup.sniper,
            pickup.grenadelauncher,
            pickup.bulletPrefab,
            pickup.projectileSpeed
        );
    }

    private void PlayWeaponSwapSound()
    {
        if (weaponSwapSound == null)
        {
            Debug.LogWarning(
                "PlayerWeaponManager: Weapon swap sound is not assigned."
            );
            return;
        }

        audioSource.PlayOneShot(
            weaponSwapSound,
            weaponSwapVolume
        );
    }

    private WeaponPickup GetClosestWeapon()
    {
        WeaponPickup closestWeapon = null;
        float closestDistance = Mathf.Infinity;

        foreach (WeaponPickup weapon in weaponsInRange)
        {
            if (weapon == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                transform.position,
                weapon.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestWeapon = weapon;
            }
        }

        return closestWeapon;
    }

    private void RemoveInvalidPickups()
    {
        weaponsInRange.RemoveAll(
            weapon => weapon == null
        );

        if (currentWeaponInRange == null ||
            !weaponsInRange.Contains(currentWeaponInRange))
        {
            currentWeaponInRange = GetClosestWeapon();
        }
    }

    public bool HasWeaponInRange()
    {
        RemoveInvalidPickups();
        return currentWeaponInRange != null;
    }
}