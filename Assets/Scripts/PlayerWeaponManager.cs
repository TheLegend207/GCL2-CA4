using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon display")]
    public WeaponViewModelDisplay weaponViewModelDisplay;

    [Header("Optional: update existing weapon scripts")]
    public bool updateWeaponScriptFlags = true;

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

        // Display the starting model assigned to WeaponViewModelDisplay.
        if (weaponViewModelDisplay != null &&
            weaponViewModelDisplay.startingWeaponModel != null)
        {
            weaponViewModelDisplay.SetWeaponModel(
                weaponViewModelDisplay.startingWeaponModel
            );
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupWeapon();
        }
    }

    public void NotifyWeaponInRange(WeaponPickup weapon)
    {
        if (weapon == null)
            return;

        if (!weaponsInRange.Contains(weapon))
        {
            weaponsInRange.Add(weapon);
        }

        // Select the most recently entered weapon.
        currentWeaponInRange = weapon;
    }

    public void NotifyWeaponOutOfRange(WeaponPickup weapon)
    {
        if (weapon == null)
            return;

        weaponsInRange.Remove(weapon);

        if (currentWeaponInRange == weapon)
        {
            if (weaponsInRange.Count > 0)
            {
                currentWeaponInRange =
                    weaponsInRange[weaponsInRange.Count - 1];
            }
            else
            {
                currentWeaponInRange = null;
            }
        }
    }

    private void TryPickupWeapon()
    {
        if (currentWeaponInRange == null)
            return;

        WeaponPickup pickup = currentWeaponInRange;

        if (weaponViewModelDisplay != null)
        {
            weaponViewModelDisplay.SetWeaponModel(
                pickup.viewmodelPrefab
            );
        }

        if (updateWeaponScriptFlags)
        {
            UpdateExistingWeaponScripts(pickup);
        }

        weaponsInRange.Remove(pickup);
        currentWeaponInRange = null;

        Destroy(pickup.gameObject);
    }

    private void UpdateExistingWeaponScripts(WeaponPickup pickup)
    {
        PlayerShoot playerShoot = GetComponent<PlayerShoot>();

        if (playerShoot != null)
        {
            playerShoot.pistol = pickup.pistol;
            playerShoot.smg = pickup.smg;
            playerShoot.sniper = pickup.sniper;
            playerShoot.grenadelauncher =
                pickup.grenadelauncher;

            playerShoot.bulletprefab =
                pickup.bulletPrefab;

            playerShoot.projectileSpeed =
                pickup.projectileSpeed;
        }

        Gun gun = GetComponent<Gun>();

        if (gun != null)
        {
            gun.pistol = pickup.pistol;
            gun.smg = pickup.smg;
            gun.sniper = pickup.sniper;
            gun.grenadelauncher =
                pickup.grenadelauncher;

            gun.projectileSpeed =
                pickup.projectileSpeed;

            // Assign the pickup's bullet prefab based on its type.
            if (pickup.pistol)
            {
                gun.pistolbullet = pickup.bulletPrefab;
            }
            else if (pickup.smg)
            {
                gun.smgbullet = pickup.bulletPrefab;
            }
            else if (pickup.sniper)
            {
                gun.sniperbullet = pickup.bulletPrefab;
            }
            else if (pickup.grenadelauncher)
            {
                gun.grenade = pickup.bulletPrefab;
            }
        }
    }
}