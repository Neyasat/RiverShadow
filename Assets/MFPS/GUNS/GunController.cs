using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GunController : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public Camera fpsCamera; // �������� ������
    public AudioSource gunAudio;
    public GameObject impactEffect;
    public GameObject sparksPrefab; // ������ ����
    public GameObject smokePrefab; // ������ ����
    public Transform firePoint; // ����� ��� ��������
    public Image crosshair; // ������ �� ����������� �������

    // ������ �� �������
    public Light muzzleFlashLight;
    public SpriteRenderer muzzleFireSprite; // �������� ������ ����
    public Sprite[] fireSprites; // ������ �������� ��� �����������
    private int currentSpriteIndex = 0; // ������� ������ �������

    // ��������� ������
    public Transform gunTransform; // ������, ������� ����� ��������� ��� ������
    public float recoilAmount = 0.1f; // ���� ������ �����
    public float recoilSpeed = 6f; // �������� �������� �������
    public float recoilRotationAmountX = 2f; // ���� �������� �����
    public float recoilRotationAmountY = 1f; // ���� �������� ������
    public float recoilRotationSpeed = 4f; // �������� �������� ��������

    // ��������� �������� ������
    public float cameraRecoilAmount = 0.05f; // ���� �������� ������
    public float cameraRecoilSpeed = 8f; // �������� �������� ������

    // ��������� ������������
    public Transform aimPosition; // ������� ��� ������������
    public float aimSpeed = 10f; // �������� �������� � ������������
    private Vector3 originalPosition; // �������� ��������� ������
    private Quaternion originalRotation; // �������� ������� ������
    private Vector3 originalCameraPosition; // �������� ��������� ������
    public float normalFOV = 60f; // ������� ���� ������ ������
    public float aimFOV = 40f; // ���� ������ ��� ������������

    private float nextTimeToFire = 0f;
    private bool isAiming = false; // ���� ������������

    void Start()
    {
        // ��������� �������� ��������� � ������� ������ � ������
        if (gunTransform != null)
        {
            originalPosition = gunTransform.localPosition;
            originalRotation = gunTransform.localRotation;
        }

        if (fpsCamera != null)
        {
            originalCameraPosition = fpsCamera.transform.localPosition;
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

        // ���������, ��� ����������� �������� ��� ������
        if (crosshair != null)
        {
            crosshair.enabled = true;
        }
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

        // ����������� ����������� ������ � ������ � �������� ��������� � �������
        if (gunTransform != null && !isAiming)
        {
            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
            gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, originalRotation, Time.deltaTime * recoilRotationSpeed);
        }

        if (fpsCamera != null)
        {
            fpsCamera.transform.localPosition = Vector3.Lerp(fpsCamera.transform.localPosition, originalCameraPosition, Time.deltaTime * cameraRecoilSpeed);
        }

        // ���������� ������������ �����������
        if (crosshair != null)
        {
            crosshair.enabled = !isAiming; // �������� ����������� ��� ������������
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
            // �������� ������ �����
            gunTransform.localPosition -= new Vector3(0, 0, recoilAmount);

            // ������� ������, ������ ���� �� �������������
            if (!isAiming)
            {
                gunTransform.localRotation *= Quaternion.Euler(-recoilRotationAmountX, recoilRotationAmountY, 0); // ������� ����� � ������
            }
        }

        // �������� ������ ��� ������
        if (fpsCamera != null)
        {
            fpsCamera.transform.localPosition += new Vector3(cameraRecoilAmount, cameraRecoilAmount / 2, 0); // �������� ������ ����� � ������
        }

        // �������� �� ���������
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            // ��������� �� ������� � Target.cs
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                // �������� ����, ����� ���������, ����������� ��������� � �������
                target.TakeDamage(damage, hit.point, fpsCamera.transform.forward, hit.normal);
            }

            // ��������� �� ����� � Zombie.cs
            Zombie zombie = hit.transform.GetComponentInParent<Zombie>();
            if (zombie != null)
            {
                // �������� ���� � ��������� ���������
                zombie.TakeDamage(damage, hit.collider);
            }

            // �������� ������� ���������
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

            // �������� ������� ���� � ����� ���������
            if (sparksPrefab != null)
            {
                GameObject sparks = Instantiate(sparksPrefab, hit.point, Quaternion.identity); // ������������� ��� ��������
                sparks.transform.forward = fpsCamera.transform.forward; // ���������� �� ����������� � ������
                StartCoroutine(DisableSparks(sparks, 0.1f)); // �������� �� �������� �����
            }

            // �������� ������� ���� � ����� ���������
            if (smokePrefab != null)
            {
                GameObject smoke = Instantiate(smokePrefab, hit.point, Quaternion.identity); // ������������� ��� ��������
                smoke.transform.forward = fpsCamera.transform.forward; // ���������� �� ����������� � ������
                StartCoroutine(FadeOutSmoke(smoke, 1.5f)); // �������� � ������ �������� ���
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

    // ��������� ��� ���������� ����
    IEnumerator DisableSparks(GameObject sparks, float delay)
    {
        yield return new WaitForSeconds(delay);
        sparks.SetActive(false);
        Destroy(sparks); // ���������� ������
    }

    // ��������� ��� �������� ������������ ���� � ����������� ��������
    IEnumerator FadeOutSmoke(GameObject smoke, float duration)
    {
        SpriteRenderer smokeRenderer = smoke.GetComponent<SpriteRenderer>();
        Color originalColor = smokeRenderer.color;
        Vector3 originalScale = smoke.transform.localScale; // �������� �������
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration); // ���������� �����������
            smokeRenderer.color = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0), t);
            smoke.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t); // ��������� �������
            yield return null;
        }

        Destroy(smoke); // ���������� ������ ����� ������������
    }
}
