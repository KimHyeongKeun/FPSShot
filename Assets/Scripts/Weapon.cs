using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int>{ }
public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent        onAmmoEvent = new AmmoEvent();
    [HideInInspector]
    public MagazineEvent    onMagazineEvent = new MagazineEvent();

    [Header("Fire Effects")]
    [SerializeField]
    private GameObject      muzzleFlashEffect;          //총구 이팩트

    [Header("Spawn Points")]
    [SerializeField]
    private Transform       casingSpawnPoint;           //탄피 생성 위치
    [SerializeField]
    private Transform       bulletSpawnPoint;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip       audioClipTakeOutWeapon;     //무기 장착 사운드
    [SerializeField]
    private AudioClip       audioClipFire;      
    [SerializeField]    
    private AudioClip       audioClipReload;

    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSetting weaponSetting;                //무기 설정

    private float lastAttackTime = 0;                   //마지막 발사시간 체크용
    private bool   isReload = false;                    //재장전 중인지 체크

    private AudioSource     audioSource;                // 사운드 재생 컴포넌트
    private PlayerAnimatorController animator;          //애니메이션 재생 제어
    private CasingMemoryPool casingMemoryPool;          //탄피 생성 후 활성/비활성 관리
    private ImpactMemoryPool ImpactMemoryPool;
    private Camera           mainCamera;


    public WeaponName WeaponName => weaponSetting.weaponName;
    public int currentMagazine => weaponSetting.currentMagazine;
    public int MaxMagazine => weaponSetting.maxMagazine;

    private void Awake() 
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<PlayerAnimatorController>();
        casingMemoryPool = GetComponent<CasingMemoryPool>();
        ImpactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;


        weaponSetting.currentMagazine = weaponSetting.maxMagazine;
        weaponSetting.currentAmmo = weaponSetting.maxAmmo;
    }

    private void OnEnable() 
    {
        //무기 장착 사운드 재생
        PlaySound(audioClipTakeOutWeapon);
        muzzleFlashEffect.SetActive(false);

        onMagazineEvent.Invoke(weaponSetting.currentMagazine);

        onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);
    }

    public void StartWeaponAction(int type = 0)
    {

        //재장전 중일 때는 무기 액션을 할 수 없다.
        if(isReload == true) return;


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

    public void StartReload()
    {
        //현재 재장전 중이면 재장전 불가능
        if(isReload == true || weaponSetting.currentMagazine <=0) return;

        //무기 액션 도중에 "R"키를 눌러 재장전을 시도하면 무기 액션 종료 후 재장전
        StopWeaponAction();

        StartCoroutine("OnReload");
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

            TwoStepRaycast();
        }
    }

    private IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSetting.attackRate * 0.3f);

        muzzleFlashEffect.SetActive(false);
    }

    private IEnumerator OnReload()
    {
        isReload = true;

        //재장전 애니메이션, 사운드 재생
        animator.OnReload();
        PlaySound(audioClipReload);

        while(true)
        {
            if(audioSource.isPlaying == false && animator.CurrentAnimationIs("Movement"))
            {
                isReload = false;

                weaponSetting.currentMagazine --;
                onMagazineEvent.Invoke(weaponSetting.currentMagazine);

                //현재 탄 수를 최대로 설정하고, 바뀐 . 탄 수  Text UI에 업데이트
                weaponSetting.currentAmmo = weaponSetting.maxAmmo;
                onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

                yield break;
            }

            yield return null;
        }
    }

    private void TwoStepRaycast()
    {
        Ray ray;
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        //화면의 중앙 좌표 (Aim 기준으로 Raycast 연산)
        ray = mainCamera.ViewportPointToRay(Vector2.one * 0.5f);
        //공격 사거리(attactDistance)안에 부딪히는 오브젝트가 있으면 targetPoint는 광선에 부딪힌 위치
        if(Physics.Raycast(ray, out hit, weaponSetting.attackDistance))
        {
            targetPoint = hit.point;
        }
        //공격 사거리 안에 부딪히는 오브젝트가 없으면 targetPoint는 최대 사거리 위치
        else
        {
            targetPoint = ray.origin + ray.direction*weaponSetting.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction*weaponSetting.attackDistance, Color.red);

        //첫번째 Raycast연ㅅ나으로 얻어진 targetPoint를 목표지점으로 설정하고,
        //총구를 시작지점으로 하여 Raycast연산
        Vector3 attackDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if(Physics.Raycast(bulletSpawnPoint.position, attackDirection, out hit, weaponSetting.attackDistance))
        {
            ImpactMemoryPool.SpawnImpact(hit);
        }
        Debug.DrawRay(bulletSpawnPoint.position, attackDirection*weaponSetting.attackDistance, Color.blue);
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();         //기존에 재생중인 사운드를 정지
        audioSource.clip = clip;    // 새로운 사운드 clip으로 교체 후
        audioSource.Play();         // 사운드 재생
    }
}
