using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    public enum WeaponType
    {
        None, Basuca, Desert, Escopeta, Hacha, Lanzallamas, M4a1, Machete, Miniusi, Sable, Snyper, Usp
    }

    [Header("Slots")]
    public WeaponType currentWeapon = WeaponType.None; // Slot arma
    public WeaponType currentTool = WeaponType.None;   // Slot herramienta

    private Animator animator;

    [Header("Prefabs de Proyectiles")]
    public GameObject basucaBullet, desertBullet, escopetaBullet, hachaBullet,
                      lanzallamasBullet, m4a1Bullet, macheteBullet,
                      miniusiBullet, sableBullet, snyperBullet, uspBullet;

    [Header("Prefabs de Armas que Caen")]
    public GameObject basucaDrop, desertDrop, escopetaDrop, hachaDrop,
                      lanzallamasDrop, m4a1Drop, macheteDrop,
                      miniusiDrop, sableDrop, snyperDrop, uspDrop;

    [Header("Punto de Disparo")]
    public Transform firePoint;

    [Header("Fuerza del disparo")]
    public float bulletForce = 10f;

    [Header("Cadencia de disparo (segundos entre balas)")]
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    private Dictionary<WeaponType, int> maxAmmo = new Dictionary<WeaponType, int>()
    {
        { WeaponType.Basuca, 5 },
        { WeaponType.Desert, 30 },
        { WeaponType.Escopeta, 12 },
        { WeaponType.Hacha, 0 },
        { WeaponType.Lanzallamas, 100 },
        { WeaponType.M4a1, 60 },
        { WeaponType.Machete, 0 },
        { WeaponType.Miniusi, 50 },
        { WeaponType.Sable, 0 },
        { WeaponType.Snyper, 10 },
        { WeaponType.Usp, 25 }
    };

    private Dictionary<WeaponType, int> currentAmmo = new Dictionary<WeaponType, int>();

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        foreach (var kvp in maxAmmo)
            currentAmmo[kvp.Key] = kvp.Value;
    }

    void Update()
    {
        // 🔸 Disparo arma de fuego (S)
        if (currentWeapon != WeaponType.None && !EsHerramienta(currentWeapon))
        {
            if (Input.GetKey(KeyCode.S))
            {
                animator.SetBool("isShooting", true);
                animator.SetFloat("weaponID", (float)currentWeapon);

                if (Time.time >= nextFireTime)
                {
                    Shoot(currentWeapon);
                    nextFireTime = Time.time + fireRate;
                }
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                animator.SetTrigger("ShootOnce");
                animator.SetFloat("weaponID", (float)currentWeapon);
                Shoot(currentWeapon);
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                animator.SetBool("isShooting", false);
            }

            if (Input.GetKeyDown(KeyCode.R))
                Reload(currentWeapon);
        }

        // 🔸 Ataque cuerpo a cuerpo (D)
        if (currentTool != WeaponType.None)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                animator.SetTrigger("MeleeAttack");
                animator.SetFloat("meleeID", (float)currentTool);
                MeleeAttack(currentTool);
            }
        }
    }

    public void EquipWeapon(WeaponType newWeapon)
    {
        if (EsHerramienta(newWeapon))
        {
            // Slot herramienta
            currentTool = newWeapon;
            Debug.Log("✅ Herramienta equipada: " + newWeapon);
        }
        else
        {
            // Slot arma
            currentWeapon = newWeapon;

            if (currentAmmo[newWeapon] <= 0)
                currentAmmo[newWeapon] = maxAmmo[newWeapon];

            Debug.Log("✅ Arma equipada: " + newWeapon + " | Munición: " + currentAmmo[newWeapon] + "/" + maxAmmo[newWeapon]);
        }
    }

    bool EsHerramienta(WeaponType w)
    {
        return w == WeaponType.Hacha || w == WeaponType.Machete || w == WeaponType.Sable;
    }

    void Shoot(WeaponType weapon)
    {
        if (currentAmmo[weapon] <= 0)
        {
            Debug.Log("❌ Sin munición en " + weapon);
            DropWeapon(weapon);
            return;
        }

        GameObject bulletPrefab = GetBulletPrefab(weapon);
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            float direccion = transform.localScale.x > 0 ? 1f : -1f;
            rb.AddForce(new Vector2(direccion, 0) * bulletForce, ForceMode2D.Impulse);

            currentAmmo[weapon]--;
            Debug.Log("🔫 Disparo con " + weapon + " | Munición restante: " + currentAmmo[weapon]);
        }
    }

    void MeleeAttack(WeaponType tool)
    {
        Debug.Log("🗡️ Ataque cuerpo a cuerpo con " + tool);
        // Aquí podrías detectar colisiones con enemigos, aplicar daño, etc.
    }

    void Reload(WeaponType weapon)
    {
        if (!maxAmmo.ContainsKey(weapon)) return;
        if (maxAmmo[weapon] == 0) return;

        currentAmmo[weapon] = maxAmmo[weapon];
        Debug.Log("🔄 Recargado " + weapon + " | Munición: " + currentAmmo[weapon] + "/" + maxAmmo[weapon]);
    }

    void DropWeapon(WeaponType weapon)
    {
        GameObject dropPrefab = GetDropPrefab(weapon);
        if (dropPrefab == null) return;

        GameObject dropped = Instantiate(dropPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = dropped.GetComponent<Rigidbody2D>();
        if (rb == null) rb = dropped.AddComponent<Rigidbody2D>();

        float direccion = transform.localScale.x > 0 ? 1f : -1f;
        rb.AddForce(new Vector2(direccion * 2f, 3f), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);

        Destroy(dropped, 5f);
        Debug.Log("💥 El objeto " + weapon + " cayó al suelo.");

        if (EsHerramienta(weapon))
            currentTool = WeaponType.None;
        else
            currentWeapon = WeaponType.None;

        animator.SetBool("isShooting", false);
        animator.SetFloat("weaponID", 0);
        animator.ResetTrigger("MeleeAttack");
        animator.SetFloat("meleeID", 0);
    }

    GameObject GetBulletPrefab(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Basuca: return basucaBullet;
            case WeaponType.Desert: return desertBullet;
            case WeaponType.Escopeta: return escopetaBullet;
            case WeaponType.Hacha: return hachaBullet;
            case WeaponType.Lanzallamas: return lanzallamasBullet;
            case WeaponType.M4a1: return m4a1Bullet;
            case WeaponType.Machete: return macheteBullet;
            case WeaponType.Miniusi: return miniusiBullet;
            case WeaponType.Sable: return sableBullet;
            case WeaponType.Snyper: return snyperBullet;
            case WeaponType.Usp: return uspBullet;
        }
        return null;
    }

    GameObject GetDropPrefab(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Basuca: return basucaDrop;
            case WeaponType.Desert: return desertDrop;
            case WeaponType.Escopeta: return escopetaDrop;
            case WeaponType.Hacha: return hachaDrop;
            case WeaponType.Lanzallamas: return lanzallamasDrop;
            case WeaponType.M4a1: return m4a1Drop;
            case WeaponType.Machete: return macheteDrop;
            case WeaponType.Miniusi: return miniusiDrop;
            case WeaponType.Sable: return sableDrop;
            case WeaponType.Snyper: return snyperDrop;
            case WeaponType.Usp: return uspDrop;
        }
        return null;
    }
}
