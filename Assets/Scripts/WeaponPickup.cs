using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon type")]
    public bool pistol;
    public bool smg;
    public bool sniper;
    public bool grenadelauncher;

    [Header("Ammo to give / max ammo for this weapon")]
    public int ammoToAdd = 30;

    [Header("Projectile & viewmodel")]
    public GameObject bulletPrefab;
    public float projectileSpeed = 50f;
    public GameObject viewmodelPrefab;

    [Header("Optional name")]
    public string weaponName = "Weapon";

   private void OnTriggerEnter(Collider other) //if tag is player, add gun to the list of pick up weapons
{
    if (!other.CompareTag("Player"))
        return;

    PlayerWeaponManager manager =
        other.GetComponent<PlayerWeaponManager>();

    if (manager != null)
    {
        manager.NotifyWeaponInRange(this);
    }
}

private void OnTriggerExit(Collider other) //when exiting hitbox, remove gun from list of pick up weapons
{
    if (!other.CompareTag("Player"))
        return;

    PlayerWeaponManager manager =
        other.GetComponent<PlayerWeaponManager>();

    if (manager != null)
    {
        manager.NotifyWeaponOutOfRange(this);
    }
}
}
