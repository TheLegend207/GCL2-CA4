using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("References")]
    public Transform weaponHolder; // Where viewmodels are parented

    // Overlap tracking for weapon pickups
    private readonly List<WeaponPickup> weaponsInRange = new();
    private WeaponPickup currentWeaponInRange;

    // Viewmodel management
    private GameObject currentViewmodel;

    void Update()
    {
        // Pickup / swap weapon when overlapping and pressing E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupWeapon();
        }
    }

    /// <summary>
    /// Called by WeaponPickup when the player enters its trigger.
    /// </summary>
    public void NotifyWeaponInRange(WeaponPickup weapon)
    {
        if (!weaponsInRange.Contains(weapon))
            weaponsInRange.Add(weapon);

        // Simple policy: most recently entered weapon is the one you can pick up
        currentWeaponInRange = weapon;
    }

    /// <summary>
    /// Called by WeaponPickup when the player exits its trigger.
    /// </summary>
    public void NotifyWeaponOutOfRange(WeaponPickup weapon)
    {
        weaponsInRange.Remove(weapon);

        if (currentWeaponInRange == weapon)
        {
            currentWeaponInRange = weaponsInRange.Count > 0 ? weaponsInRange[weaponsInRange.Count - 1] : null;
        }
    }

    private void TryPickupWeapon()
    {
        if (currentWeaponInRange == null)
            return;

        PickupWeapon(currentWeaponInRange);
        // The pickup destroys itself, so clear reference
        currentWeaponInRange = null;
    }

    /// <summary>
    /// Handles picking up or swapping a weapon from a WeaponPickup.
    /// This only handles viewmodel swapping and destroying the pickup.
    /// Your PlayerShoot / Gun scripts keep controlling shooting and ammo.
    /// </summary>
    public void PickupWeapon(WeaponPickup pickup)
    {
        if (pickup == null)
            return;

        // Swap viewmodel
        SwapViewmodel(pickup.viewmodelPrefab);

        // Optional: if you want PlayerShoot/Gun to know which weapon is active,
        // you can set flags here by accessing their references.
        // Example:
        // var playerShoot = GetComponent<PlayerShoot>();
        // if (playerShoot != null)
        // {
        //     playerShoot.pistol = pickup.pistol;
        //     playerShoot.smg = pickup.smg;
        //     playerShoot.sniper = pickup.sniper;
        //     playerShoot.grenadelauncher = pickup.grenadelauncher;
        // }

        // Remove the world pickup
        Destroy(pickup.gameObject);
    }

    /// <summary>
    /// Instantiates a new viewmodel under weaponHolder and destroys the old one.
    /// </summary>
    public void SwapViewmodel(GameObject newViewmodelPrefab)
    {
        if (currentViewmodel != null)
        {
            Destroy(currentViewmodel);
            currentViewmodel = null;
        }

        if (newViewmodelPrefab != null && weaponHolder != null)
        {
            currentViewmodel = Instantiate(
                newViewmodelPrefab,
                weaponHolder.position,
                weaponHolder.rotation,
                weaponHolder
            );
        }
    }
}