using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public Camera fpsCamera;
    public AudioSource gunAudio;
    public GameObject impactEffect;
    public Transform firePoint; // ����� ��� ��������

    // ������ �� �������
    public Light muzzleFlashLight;
    public SpriteRenderer muzzleFireSprite; // �������� ������ ����
    public Sprite[] fireSprites; // ������ �������� ��� �����������
    private int currentSpriteIndex = 0; // ������� ������ �������

    // ��������� ������
    public Transform gunTransform; // ������, ������� ����� ��������� ��� ������
    public float recoilAmount = 0.1f; // ���� ������
    public float recoilSpeed = 6f; // �������� ��������

    // ��������� ������������
    public Transform aimPosition; // ������� ��� ������������
    public float aimSpeed = 10f; // �������� �������� � ������������
    private Vector3 originalPosition; // �������� ��������� ������
    public float normalFOV = 60f; // ������� ���� ������ ������
    public float aimFOV = 40f; // ���� ������ ��� ������������

    private float nextTimeToFire = 0f;
    private bool isAiming = false; // ���� ������������

    void Start()
    {
        // ��������� �������� ��������� ������
        if (gunTransform != null)
        {
            originalPosition = gunTransform.localPosition;
        }

        // ���������� ��������� ������� �������
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false; // ��������� ���� ��� ������
        }

        if (muzzleFireSprite != null)
        {
            muzzleFireSprite.enabled = false; // ��������� ������ ������� ��� ������
        }

        // ������������� ����������� ���� ������ ������
        fpsCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        // �������� ������� ������ �������� � ������� �� ���������� ��������
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // ���������� �������������
        if (Input.GetMouseButton(1)) // ������ ������ ���� ��� ������������
        {
            isAiming = true;
            AimDownSights();
        }
        else
        {
            isAiming = false;
            ReturnToNormal();
        }

        // ����������� ����������� ������ � �������� ���������
        if (gunTransform != null && !isAiming)
        {
            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
        }
    }

    void Shoot()
    {
        // ������ ������� �����
        if (muzzleFlashLight != null)
        {
            StartCoroutine(MuzzleFlashEffect());
        }

        // �������� �������� ���� � ���������� �������
        if (muzzleFireSprite != null && fireSprites.Length > 0)
        {
            muzzleFireSprite.sprite = fireSprites[currentSpriteIndex];
            currentSpriteIndex = (currentSpriteIndex + 1) % fireSprites.Length; // ���������� ����������� �������
            StartCoroutine(MuzzleFireEffect());
        }

        // ��������������� ����� ��������
        if (gunAudio != null)
        {
            gunAudio.Play();
        }

        // ���������� ������� ������
        if (gunTransform != null)
        {
            gunTransform.localPosition -= new Vector3(0, 0, recoilAmount); // �������� ������ �����
        }

        // �������� �� ���������
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // �������� ������� ���������
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
    }

    // ������� ��� ���������� �������������
    void AimDownSights()
    {
        // ���������� ������ � ������� ������������
        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, aimPosition.localPosition, Time.deltaTime * aimSpeed);
        // ��������� ���� ������ ������ ��� ������� �����������
        fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, aimFOV, Time.deltaTime * aimSpeed);
    }

    // ������� ��� �������� � �������� ����
    void ReturnToNormal()
    {
        // ���������� ������ � �������� ���������
        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalPosition, Time.deltaTime * aimSpeed);
        // ���������� ���� ������ ������ � ������� ���������
        fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, normalFOV, Time.deltaTime * aimSpeed);
    }

    // ��������� ��� ���������� �������� �����
    IEnumerator MuzzleFlashEffect()
    {
        muzzleFlashLight.enabled = true; // �������� ����
        yield return new WaitForSeconds(0.05f); // ��������� ���� ���������� �� �������� �����
        muzzleFlashLight.enabled = false; // ��������� ����
    }

    // ��������� ��� ���������� �������� ����
    IEnumerator MuzzleFireEffect()
    {
        muzzleFireSprite.enabled = true; // �������� ����������� ������� ����
        yield return new WaitForSeconds(0.05f); // ��������� ����� ���������� �� �������� �����
        muzzleFireSprite.enabled = false; // ��������� ����������� �������
    }
}
