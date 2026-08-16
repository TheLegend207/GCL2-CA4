using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("References")]
    public WeaponViewModelDisplay weaponViewModelDisplay;

    [Header("Pickup settings")]
    public KeyCode pickupKey = KeyCode.E;

    private readonly List<WeaponPickup> weaponsInRange =
        new List<WeaponPickup>();

    private WeaponPickup currentWeaponInRange;

    private void Start()
    {
        if (weaponViewModelDisplay == null)
        {
            weaponViewModelDisplay =
                GetComponent<WeaponViewModelDisplay>();
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
            return;

        if (!weaponsInRange.Contains(weapon))
        {
            weaponsInRange.Add(weapon);
        }

        currentWeaponInRange = GetClosestWeapon();
    }

    public void NotifyWeaponOutOfRange(WeaponPickup weapon)
    {
        if (weapon == null)
            return;

        weaponsInRange.Remove(weapon);

        if (currentWeaponInRange == weapon)
        {
            currentWeaponInRange = GetClosestWeapon();
        }
    }

    private void TryPickupWeapon()
{
    if (currentWeaponInRange == null)
        return;

    WeaponPickup pickup = currentWeaponInRange;

    if (weaponViewModelDisplay == null)
    {
        Debug.LogError(
            "PlayerWeaponManager: Weapon View Model Display is not assigned."
        );
        return;
    }

    if (pickup.viewmodelPrefab == null)
    {
        Debug.LogError(
            "WeaponPickup: Viewmodel Prefab is not assigned."
        );
        return;
    }

    weaponViewModelDisplay.SetWeaponModel(
        pickup.viewmodelPrefab
    );

    UpdatePlayerShoot(pickup);

    weaponsInRange.Remove(pickup);
    currentWeaponInRange = null;

    Destroy(pickup.gameObject);
}

    private void UpdatePlayerShoot(WeaponPickup pickup)
    {
        PlayerShoot playerShoot =
            GetComponent<PlayerShoot>();

        if (playerShoot == null)
        {
            Debug.LogWarning(
                "PlayerWeaponManager: PlayerShoot was not found on the player."
            );
            return;
        }

        playerShoot.pistol = pickup.pistol;
        playerShoot.smg = pickup.smg;
        playerShoot.sniper = pickup.sniper;
        playerShoot.grenadelauncher =
            pickup.grenadelauncher;

        if (pickup.bulletPrefab != null)
        {
            playerShoot.bulletprefab =
                pickup.bulletPrefab;
        }

        playerShoot.projectileSpeed =
            pickup.projectileSpeed;

        Debug.Log("PlayerShoot updated to weapon: " + pickup.name);
    }

    private WeaponPickup GetClosestWeapon()
    {
        WeaponPickup closestWeapon = null;
        float closestDistance = Mathf.Infinity;

        foreach (WeaponPickup weapon in weaponsInRange)
        {
            if (weapon == null)
                continue;

            float distance =
                Vector3.Distance(transform.position, weapon.transform.position);

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
        weaponsInRange.RemoveAll(weapon => weapon == null);

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