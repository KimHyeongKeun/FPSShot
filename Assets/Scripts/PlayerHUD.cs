using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    
    [Header("Components")]
    [SerializeField]
    private Weapon weapon;


    [Header("Weapon Base")]
    [SerializeField]
    private TextMeshProUGUI textWeaponName;
    [SerializeField]
    private Image           imageWeaponIcon;
    [SerializeField]
    private Sprite[]        spriteWeaponIcons;

    [Header("Ammo")]
    [SerializeField]
    private TextMeshProUGUI  textAmmo;

    [Header("Magazine")]
    [SerializeField]
    private GameObject       magazineUIPrehab;
    [SerializeField]
    private Transform       magazineParent;

    private List<GameObject> magazineList;

    private void Awake()
    {
        SetupWeapon();
        SetupMagazine();
        weapon.onAmmoEvent.AddListener(UpdateAmmoHUD);
        weapon.onMagazineEvent.AddListener(UpdateMagazineHUD);
    }

    private void SetupWeapon()
    {
        textWeaponName.text = weapon.WeaponName.ToString();
        imageWeaponIcon.sprite = spriteWeaponIcons[(int)weapon.WeaponName]; 

    }

    private void UpdateAmmoHUD(int currentAmmo, int maxAmmo)
    {
        textAmmo.text = $"<size=40>{currentAmmo}/</size>{maxAmmo}";
    }

    private void SetupMagazine()
    {
        magazineList = new List<GameObject>();
        for(int i=0; i<weapon.MaxMagazine; ++i)
         {
                GameObject clone = Instantiate(magazineUIPrehab);
                clone.transform.SetParent(magazineParent);
                clone.SetActive(false);

                magazineList.Add(clone);
        }

                    for(int i=0; i<weapon.currentMagazine; ++i)
            {
                magazineList[i].SetActive(true);
            }

    }

    private void UpdateMagazineHUD(int currentMagazine)
    {
        for(int i=0; i<magazineList.Count; i++)
        {
            magazineList[i].SetActive(false);
        }
        for(int i=0; i<currentMagazine; ++i)
        {
            magazineList[i].SetActive(true);
        }
    }
}

