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

//replaces currently displayed weapon model
    public void SetWeaponModel(GameObject weaponModelPrefab)
{
    Debug.Log(
        "Changing viewmodel to: " +
        (weaponModelPrefab == null
            ? "NULL"
            : weaponModelPrefab.name)
    );

    if (currentWeaponModel != null)
    {
        Destroy(currentWeaponModel);
        currentWeaponModel = null;
    }

    if (weaponModelPrefab == null) //no prefab to use
    {
        Debug.LogError(
            "WeaponViewModelDisplay: Viewmodel prefab is null."
        );
        return;
    }

    if (weaponHolder == null) //no weapon holder to use
    {
        Debug.LogError(
            "WeaponViewModelDisplay: Weapon Holder is not assigned."
        );
        return;
    }
    //replace current weapon with new model
    currentWeaponModel = Instantiate(
        weaponModelPrefab,
        weaponHolder
    );

        //position the weapon properly
        currentWeaponModel.transform.localPosition = Vector3.zero;
    currentWeaponModel.transform.localRotation =
        Quaternion.identity;
    currentWeaponModel.transform.localScale =
        Vector3.one;
}
    public GameObject GetCurrentWeaponModel()
    {
        return currentWeaponModel;
    }

    
}