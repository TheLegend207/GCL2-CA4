using UnityEngine;

public class WeaponViewModelDisplay : MonoBehaviour
{
    [Header("Viewmodel parent")]
    [Tooltip("Usually an empty GameObject positioned in front of the player camera.")]
    public Transform weaponHolder;

    [Header("Starting weapon")]
    public GameObject startingWeaponModel;

    private GameObject currentWeaponModel;

    private void Start()
    {
        if (startingWeaponModel != null)
        {
            SetWeaponModel(startingWeaponModel);
        }
    }

    /// <summary>
    /// Replaces the currently displayed weapon model.
    /// </summary>
    public void SetWeaponModel(GameObject weaponModelPrefab)
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            currentWeaponModel = null;
        }

        if (weaponModelPrefab == null)
        {
            Debug.LogWarning("WeaponViewModelDisplay: No weapon model was provided.");
            return;
        }

        if (weaponHolder == null)
        {
            Debug.LogError("WeaponViewModelDisplay: Weapon Holder is not assigned.");
            return;
        }

        currentWeaponModel = Instantiate(
            weaponModelPrefab,
            weaponHolder
        );

        // Reset local transform so the prefab uses the holder's position.
        currentWeaponModel.transform.localPosition = Vector3.zero;
        currentWeaponModel.transform.localRotation = Quaternion.identity;
        currentWeaponModel.transform.localScale = Vector3.one;
    }

    public GameObject GetCurrentWeaponModel()
    {
        return currentWeaponModel;
    }
}