using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }

public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent        onAmmoEvent = new AmmoEvent();

    [Header("Fire Effects")]
    [SerializeField]
    private GameObject      muzzleFlashEffect;          //총구 이팩트

    [Header("Spawn Points")]
    [SerializeField]
    private Transform       casingSpawnPoint;           //탄피 생성 위치

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip       audioClipTakeOutWeapon;     //무기 장착 사운드

    [SerializeField]
    private AudioClip       audioClipFire;          

    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSetting weaponSetting;                //무기 설정

    private float lastAttackTime = 0;                   //마지막 발사시간 체크용

    private AudioSource     audioSource;                // 사운드 재생 컴포넌트
    private PlayerAnimatorController animator;          //애니메이션 재생 제어
    private CasingMemoryPool casingMemoryPool;          //탄피 생성 후 활성/비활성 관리

    public WeaponName WeaponName => weaponSetting.weaponName;

    private void Awake() 
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<PlayerAnimatorController>();
        casingMemoryPool = GetComponent<CasingMemoryPool>();

        weaponSetting.currentAmmo = weaponSetting.maxAmmo;
    }

    private void OnEnable() 
    {
        //무기 장착 사운드 재생
        PlaySound(audioClipTakeOutWeapon);
        muzzleFlashEffect.SetActive(false);

        onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);
    }

    public void StartWeaponAction(int type = 0)
    {
        //마우스 왼쪽 클릭(공격 시작)
        if(type ==0)
        {
            //연속 공격
            if(weaponSetting.isAutomaticAttack == true)
            {
                StartCoroutine("OnAttackLoop");
            }
            //단발 공격
            else{
                OnAttack();
            }
        }
    }

    public void StopWeaponAction(int type =0)
    {
        //마우스 왼쪽 클릭(공격 종료)
        if(type ==0)
        {
            StopCoroutine("OnAttackLoop");
        }
    }

    private IEnumerator OnAttackLoop()
    {
        while(true)
        {
            OnAttack();

            yield return null;
        }
    }

    public void OnAttack()
    {
        if(Time.time - lastAttackTime > weaponSetting.attackRate)
        {
            if(animator.MoveSpeed > 0.5f)
            {
                return;
            }

            //공격주기가 되어야 공격할 수 있도록 하기 위해서 현재 시간 저장
            lastAttackTime = Time.time;

            //탄 수가 없으면 공격 불가능
            if(weaponSetting.currentAmmo <=0)
            {
                return;
            }
            //공격시 currentAmmo 1감소
            weaponSetting.currentAmmo--;
            onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

            animator.Play("Fire", -1, 0);

            StartCoroutine("OnMuzzleFlashEffect");
            PlaySound(audioClipFire);
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right);
        }
    }

    private IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSetting.attackRate * 0.3f);

        muzzleFlashEffect.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();         //기존에 재생중인 사운드를 정지
        audioSource.clip = clip;    // 새로운 사운드 clip으로 교체 후
        audioSource.Play();         // 사운드 재생
    }
}
