using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    public enum WeaponType
    {
        None, Basuca, Desert, Escopeta, Hacha, Lanzallamas, M4a1, Machete, Miniusi, Sable, Snyper, Usp
    }

    [Header("Slots")]
    public WeaponType currentWeapon = WeaponType.None;
    public WeaponType currentTool = WeaponType.None;

    private Animator animator;

    [Header("Prefabs de Proyectiles (solo armas de fuego)")]
    public GameObject basucaBullet, desertBullet, escopetaBullet,
                      lanzallamasBullet, m4a1Bullet, miniusiBullet,
                      snyperBullet, uspBullet;

    [Header("Prefabs de Armas que Caen")]
    public GameObject basucaDrop, desertDrop, escopetaDrop, hachaDrop,
                      lanzallamasDrop, m4a1Drop, macheteDrop,
                      miniusiDrop, sableDrop, snyperDrop, uspDrop;

    [Header("Sprites del puñete (por pares)")]
    public Sprite[] punchSprites;
    public SpriteRenderer spriteRenderer;

    [Header("Configuración de puñete")]
    public float tiempoMaxCombo = 0.6f;
    public float duracionFrame = 0.1f;

    private int indiceGolpe = 0;
    private float tiempoUltimoGolpe = 0f;
    private bool mostrandoSegundoFrame = false;
    private float tiempoFrameActual = 0f;

    private bool golpeEnCurso = false;
    public float tiempoEntreGolpes = 0.25f;

    [Header("Punto de Disparo")]
    public Transform firePoint;

    [Header("Fuerza del disparo")]
    public float bulletForce = 10f;

    private float nextFireTime = 0f;

    [Header("Sonidos de Disparo por Arma")]
    public AudioSource audioSource; // Componente que reproducirá los sonidos
    public AudioClip basucaSound, desertSound, escopetaSound,
                     lanzallamasSound, m4a1Sound, miniusiSound,
                     snyperSound, uspSound;

    [Header("Sonidos de Ataque Cuerpo a Cuerpo")]
    public AudioClip hachaSound;
    public AudioClip macheteSound;
    public AudioClip sableSound;
    public AudioClip punheteSound; // opcional: sonido para el puñete sin arma


    // 🔸 Diccionario de munición
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

    // 🔫 Frecuencia de disparo (segundos entre balas)
    private Dictionary<WeaponType, float> fireRates = new Dictionary<WeaponType, float>()
    {
        { WeaponType.Usp, 0.35f },
        { WeaponType.Desert, 0.3f },
        { WeaponType.M4a1, 0.12f },
        { WeaponType.Miniusi, 0.08f },
        { WeaponType.Snyper, 0.6f },
        { WeaponType.Escopeta, 0.5f },
        { WeaponType.Basuca, 1.2f },
        { WeaponType.Lanzallamas, 0.05f }
    };

    // 🔸 Balas por click simple
    private Dictionary<WeaponType, int> bulletsPerClick = new Dictionary<WeaponType, int>()
    {
        { WeaponType.Usp, 1 },
        { WeaponType.Desert, 2 },
        { WeaponType.M4a1, 5 },
        { WeaponType.Miniusi, 7 },
        { WeaponType.Snyper, 1 },
        { WeaponType.Escopeta, 3 },
        { WeaponType.Basuca, 1 },
        { WeaponType.Lanzallamas, 0 }
    };

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        foreach (var kvp in maxAmmo)
            currentAmmo[kvp.Key] = kvp.Value;
    }

    void Update()
    {
        // 🔸 DISPARO ARMA DE FUEGO (S)
        if (currentWeapon != WeaponType.None && !EsHerramienta(currentWeapon))
        {
            // 🔹 MANTENER PRESIONADO = AUTO FIRE
            if (Input.GetKey(KeyCode.S))
            {
                animator.SetBool("isShooting", true);
                animator.SetFloat("weaponID", (float)currentWeapon);

                float rate = fireRates.ContainsKey(currentWeapon) ? fireRates[currentWeapon] : 0.25f;

                if (Time.time >= nextFireTime)
                {
                    if (currentWeapon == WeaponType.Lanzallamas)
                        LanzallamasRafaga();
                    else
                        Shoot(currentWeapon);

                    nextFireTime = Time.time + rate;
                }
            }

            // 🔹 UNA SOLA PRESIONADA = RAFALEO
            else if (Input.GetKeyDown(KeyCode.S))
            {
                animator.SetTrigger("ShootOnce");
                animator.SetFloat("weaponID", (float)currentWeapon);
                StartCoroutine(DisparoUnico(currentWeapon));
            }

            else if (Input.GetKeyUp(KeyCode.S))
            {
                animator.SetBool("isShooting", false);
                StopCoroutine("LanzallamasRafaga");

                // 🔇 Detener sonido del lanzallamas
                if (audioSource != null && audioSource.clip == lanzallamasSound)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                    audioSource.clip = null;
                }
            }

            // 🔹 Recargar (R)
            if (Input.GetKeyDown(KeyCode.R))
                Reload(currentWeapon);
        }

        // 🔸 ATAQUE CUERPO A CUERPO (D)
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (!golpeEnCurso)
            {
                if (currentTool == WeaponType.None)
                    EjecutarPuñete();
                else
                {
                    animator.SetTrigger("MeleeAttack");
                    animator.SetFloat("meleeID", (float)currentTool);
                    MeleeAttack(currentTool);
                }
            }
        }

        // 🔸 Mostrar segundo frame de golpe
        if (mostrandoSegundoFrame && Time.time - tiempoFrameActual >= duracionFrame)
        {
            int spriteIndex = indiceGolpe * 2 + 1;
            if (spriteIndex < punchSprites.Length)
                spriteRenderer.sprite = punchSprites[spriteIndex];

            mostrandoSegundoFrame = false;
            indiceGolpe++;
            if (indiceGolpe >= punchSprites.Length / 2)
                indiceGolpe = 0;

            Invoke(nameof(RestaurarAnimador), 0.2f);
        }
    }

    IEnumerator<WaitForSeconds> DisparoUnico(WeaponType weapon)
    {
        float rate = fireRates.ContainsKey(weapon) ? fireRates[weapon] : 0.25f;
        int cantidad = bulletsPerClick.ContainsKey(weapon) ? bulletsPerClick[weapon] : 1;

        // Espera animación antes de disparar
        yield return new WaitForSeconds(0.15f);

        if (weapon == WeaponType.Lanzallamas)
        {
            LanzallamasRafaga();
            yield break;
        }

        for (int i = 0; i < cantidad; i++)
        {
            Shoot(weapon);
            yield return new WaitForSeconds(rate);
        }
    }

    void LanzallamasRafaga()
    {
        if (lanzallamasBullet == null || firePoint == null) return;

        // 🎵 Sonido continuo del lanzallamas
        if (audioSource != null && lanzallamasSound != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.loop = true;
                audioSource.clip = lanzallamasSound;
                audioSource.Play();
            }
        }

        // Crear un pequeño grupo de partículas simulando "mini llamas"
        int cantidadLlamas = Random.Range(4, 7); // cuántas partículas por ráfaga
        for (int i = 0; i < cantidadLlamas; i++)
        {
            GameObject llama = Instantiate(lanzallamasBullet, firePoint.position, firePoint.rotation);

            // Asignar shooter e indicar que es un lanzallamas
            BulletLife bl = llama.GetComponent<BulletLife>();
            if (bl != null)
            {
                bl.shooter = gameObject;
                bl.isFlamethrower = true; // 🔥 evita colisión inmediata con el jugador
                bl.damage = 5;            // Daño configurable
            }

            // Configuración del sistema de partículas
            var ps = llama.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startLifetime = Random.Range(0.8f, 1.6f);
                main.startSize = Random.Range(0.05f, 0.12f);
                main.startSpeed = Random.Range(2f, 4f);
                main.gravityModifier = 0.3f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.startColor = new Color(1f, Random.Range(0.4f, 0.8f), 0f, 1f);
                ps.Play();
            }

            // Movimiento inicial de la llama
            Rigidbody2D rb = llama.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float direccion = transform.localScale.x > 0 ? 1f : -1f;

                Vector2 fuerza = new Vector2(
                    direccion * Random.Range(2.5f, 3.8f),
                    Random.Range(0.8f, 2f)
                );

                rb.gravityScale = 0.4f;
                rb.drag = 0.6f;
                rb.AddForce(fuerza, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-3f, 3f), ForceMode2D.Impulse);
            }

            // 🔥 Desvanecimiento progresivo si el prefab usa SpriteRenderer
            SpriteRenderer sr = llama.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                StartCoroutine(FadeOut(sr, Random.Range(0.8f, 1.5f)));
            }

            Destroy(llama, 2f); // Destruye la llama tras un breve tiempo
        }
    }

    // 🔸 Corrutina opcional para desvanecer sprites si no son partículas
    System.Collections.IEnumerator FadeOut(SpriteRenderer sr, float duration)
    {
        float startAlpha = sr.color.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0, t / duration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }
        Destroy(sr.gameObject);
    }

    void EjecutarPuñete()
    {
        golpeEnCurso = true;
        if (Time.time - tiempoUltimoGolpe > tiempoMaxCombo)
            indiceGolpe = 0;

        int spriteIndex = indiceGolpe * 2;
        if (spriteIndex < punchSprites.Length)
        {
            if (animator != null) animator.enabled = false;
            spriteRenderer.sprite = punchSprites[spriteIndex];
            mostrandoSegundoFrame = true;
            tiempoFrameActual = Time.time;
        }

        // 👊 Reproducir sonido del puñete (si existe)
        if (punheteSound != null && audioSource != null)
            audioSource.PlayOneShot(punheteSound);

        tiempoUltimoGolpe = Time.time;
        Invoke(nameof(DesbloquearGolpe), tiempoEntreGolpes);
    }

    void RestaurarAnimador()
    {
        if (animator != null)
            animator.enabled = true;
    }

    private void DesbloquearGolpe() => golpeEnCurso = false;

    public void EquipWeapon(WeaponType newWeapon)
    {
        if (EsHerramienta(newWeapon))
        {
            currentTool = newWeapon;
            Debug.Log("✅ Herramienta equipada: " + newWeapon);
        }
        else
        {
            currentWeapon = newWeapon;
            if (currentAmmo[newWeapon] <= 0)
                currentAmmo[newWeapon] = maxAmmo[newWeapon];

            Debug.Log("✅ Arma equipada: " + newWeapon + " | Munición: " + currentAmmo[newWeapon]);
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
        if (bulletPrefab == null || firePoint == null) return;

        // 🎵 Reproducir sonido del arma
        AudioClip sound = GetWeaponSound(weapon);
        if (sound != null && audioSource != null)
            audioSource.PlayOneShot(sound);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        BulletLife bl = bullet.GetComponent<BulletLife>();
        if (bl != null) bl.shooter = gameObject;

        float direccion = transform.localScale.x > 0 ? 1f : -1f;
        rb.AddForce(new Vector2(direccion, 0) * bulletForce, ForceMode2D.Impulse);

        currentAmmo[weapon]--;
    }

    void MeleeAttack(WeaponType tool)
    {
        Debug.Log("🗡️ Ataque cuerpo a cuerpo con " + tool);

        // Reproducir sonido correspondiente
        AudioClip sound = GetMeleeSound(tool);
        if (sound != null && audioSource != null)
            audioSource.PlayOneShot(sound);
    }

    void Reload(WeaponType weapon)
    {
        if (!maxAmmo.ContainsKey(weapon)) return;
        if (maxAmmo[weapon] == 0) return;

        currentAmmo[weapon] = maxAmmo[weapon];
        Debug.Log("🔄 Recargado " + weapon);
    }

    void DropWeapon(WeaponType weapon)
    {
        GameObject dropPrefab = GetDropPrefab(weapon);
        if (dropPrefab == null) return;

        GameObject dropped = Instantiate(dropPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = dropped.GetComponent<Rigidbody2D>() ?? dropped.AddComponent<Rigidbody2D>();

        float direccion = transform.localScale.x > 0 ? 1f : -1f;
        rb.AddForce(new Vector2(direccion * 2f, 3f), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);

        Destroy(dropped, 5f);
        if (EsHerramienta(weapon)) currentTool = WeaponType.None;
        else currentWeapon = WeaponType.None;

        animator.SetBool("isShooting", false);
    }

    GameObject GetBulletPrefab(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Basuca: return basucaBullet;
            case WeaponType.Desert: return desertBullet;
            case WeaponType.Escopeta: return escopetaBullet;
            case WeaponType.Lanzallamas: return lanzallamasBullet;
            case WeaponType.M4a1: return m4a1Bullet;
            case WeaponType.Miniusi: return miniusiBullet;
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

    AudioClip GetWeaponSound(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Basuca: return basucaSound;
            case WeaponType.Desert: return desertSound;
            case WeaponType.Escopeta: return escopetaSound;
            case WeaponType.Lanzallamas: return lanzallamasSound;
            case WeaponType.M4a1: return m4a1Sound;
            case WeaponType.Miniusi: return miniusiSound;
            case WeaponType.Snyper: return snyperSound;
            case WeaponType.Usp: return uspSound;
            default: return null;
        }
    }

    AudioClip GetMeleeSound(WeaponType tool)
    {
        switch (tool)
        {
            case WeaponType.Hacha: return hachaSound;
            case WeaponType.Machete: return macheteSound;
            case WeaponType.Sable: return sableSound;
            default: return null;
        }
    }

}
