using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotWeaponController : MonoBehaviour
{
    public enum WeaponType
    {
        None, Basuca, Desert, Escopeta, Hacha, Lanzallamas, M4a1, Machete, Miniusi, Sable, Snyper, Usp
    }

    [Header("Slots")]
    public WeaponType currentWeapon = WeaponType.None;
    public WeaponType currentTool = WeaponType.None;

    private Animator animador; // Para los bots: AnimadorPlayer1

    [Header("Prefabs de Proyectiles")]
    public GameObject basucaBullet, desertBullet, escopetaBullet,
                      lanzallamasBullet, m4a1Bullet, miniusiBullet,
                      snyperBullet, uspBullet;

    [Header("Prefabs de Armas que Caen")]
    public GameObject basucaDrop, desertDrop, escopetaDrop, hachaDrop,
                      lanzallamasDrop, m4a1Drop, macheteDrop,
                      miniusiDrop, sableDrop, snyperDrop, uspDrop;

    [Header("Sprites de Puñete")]
    public Sprite[] punchSprites;
    public SpriteRenderer spriteRenderer;

    [Header("Configuración de Puñete")]
    public float tiempoMaxCombo = 0.6f;
    public float duracionFrame = 0.1f;
    private int indiceGolpe = 0;
    private bool mostrandoSegundoFrame = false;
    private float tiempoFrameActual = 0f;
    private bool golpeEnCurso = false;
    public float tiempoEntreGolpes = 0.25f;

    [Header("Punto de Disparo")]
    public Transform firePoint;
    public float bulletForce = 10f;

    private float nextFireTime = 0f;

    [Header("Sonidos de Arma")]
    public AudioSource audioSource;
    public AudioClip basucaSound, desertSound, escopetaSound,
                     lanzallamasSound, m4a1Sound, miniusiSound,
                     snyperSound, uspSound;

    [Header("Sonidos Cuerpo a Cuerpo")]
    public AudioClip hachaSound, macheteSound, sableSound, punheteSound;

    [Header("Ataque Cuerpo a Cuerpo")]
    public Transform meleePoint;
    public float meleeRange = 0.7f;
    public int damagePuñete = 10;
    public int damageMachete = 25;
    public int damageHacha = 35;
    public int damageSable = 20;
    public LayerMask damageableLayers;

    [Header("Detección del Jugador")]
    public Transform objetivo;           // El jugador principal (asígnalo en el Inspector)
    public float rangoDeteccion = 8f;    // Distancia para atacar con armas
    public float rangoMelee = 1.5f;      // Distancia para ataque cuerpo a cuerpo

    private bool jugadorCerca = false;   // Estado: ¿jugador dentro del rango?

    private Dictionary<WeaponType, int> maxAmmo = new Dictionary<WeaponType, int>()
    {
        { WeaponType.Basuca, 5 }, { WeaponType.Desert, 30 }, { WeaponType.Escopeta, 12 },
        { WeaponType.Hacha, 0 }, { WeaponType.Lanzallamas, 100 }, { WeaponType.M4a1, 60 },
        { WeaponType.Machete, 0 }, { WeaponType.Miniusi, 50 }, { WeaponType.Sable, 0 },
        { WeaponType.Snyper, 10 }, { WeaponType.Usp, 25 }
    };

    private Dictionary<WeaponType, int> currentAmmo = new Dictionary<WeaponType, int>();

    private Dictionary<WeaponType, float> fireRates = new Dictionary<WeaponType, float>()
    {
        { WeaponType.Usp, 0.35f }, { WeaponType.Desert, 0.3f }, { WeaponType.M4a1, 0.12f },
        { WeaponType.Miniusi, 0.08f }, { WeaponType.Snyper, 0.6f }, { WeaponType.Escopeta, 0.5f },
        { WeaponType.Basuca, 1.2f }, { WeaponType.Lanzallamas, 0.05f }
    };

    private Dictionary<WeaponType, int> bulletsPerClick = new Dictionary<WeaponType, int>()
    {
        { WeaponType.Usp, 1 }, { WeaponType.Desert, 2 }, { WeaponType.M4a1, 5 },
        { WeaponType.Miniusi, 7 }, { WeaponType.Snyper, 1 }, { WeaponType.Escopeta, 3 },
        { WeaponType.Basuca, 1 }, { WeaponType.Lanzallamas, 0 }
    };

    private bool weaponDroppedOnce = false;

    void Start()
    {
        animador = GetComponentInChildren<Animator>();

        // Inicializa las municiones
        foreach (var kvp in maxAmmo)
            currentAmmo[kvp.Key] = kvp.Value;

        // 🔹 Si el objetivo no está asignado (porque el prefab no lo permite),
        // lo busca automáticamente en la escena
        if (objetivo == null)
        {
            GameObject jugador = GameObject.Find("Jugador");
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró el objeto 'Jugador' en la escena.");
            }
        }

        // Revisa periódicamente si el jugador está cerca
        InvokeRepeating(nameof(VerificarJugador), 0f, 0.2f);
    }


    void VerificarJugador()
    {
        if (objetivo == null) return;

        float distancia = Vector2.Distance(transform.position, objetivo.position);
        bool estabaCerca = jugadorCerca;

        jugadorCerca = distancia <= rangoDeteccion;

        // Solo atacar si el jugador está dentro del rango
        if (jugadorCerca)
        {
            AutoAttack();
        }
        else
        {
            // Detiene animaciones si estaba atacando
            if (animador != null && estabaCerca)
            {
                animador.SetBool("isShooting", false);
            }
        }
    }

    // 🔸 DISPARO / ATAQUE AUTOMÁTICO (solo si el jugador está cerca)
    void AutoAttack()
    {
        if (objetivo == null) return; // Si no hay jugador asignado, no hace nada

        // Calcula la distancia entre el bot y el jugador
        float distancia = Vector2.Distance(transform.position, objetivo.position);

        // 🔫 Arma de fuego: solo dispara si el jugador está dentro del rango de detección
        if (currentWeapon != WeaponType.None && !EsHerramienta(currentWeapon) && distancia <= rangoDeteccion)
        {
            float rate = fireRates.ContainsKey(currentWeapon) ? fireRates[currentWeapon] : 0.25f;

            if (Time.time >= nextFireTime)
            {
                // 🎯 Animación de disparo
                if (animador != null)
                {
                    animador.SetBool("isShooting", true);
                    animador.SetFloat("weaponID", (float)currentWeapon);
                }

                // 🔹 Acción de disparo
                if (currentWeapon == WeaponType.Lanzallamas)
                    LanzallamasRafaga();
                else
                    StartCoroutine(DisparoUnico(currentWeapon));

                // ⏱️ Control de cadencia
                nextFireTime = Time.time + rate;

                // ⏹️ Detiene la animación un poco después del disparo
                Invoke(nameof(StopShootingAnimation), rate / 1.2f);
            }
        }

        // 🪓 Herramienta cuerpo a cuerpo: solo ataca si el jugador está muy cerca
        else if (currentTool != WeaponType.None && distancia <= rangoMelee)
        {
            if (!golpeEnCurso)
            {
                if (animador != null)
                {
                    animador.SetTrigger("MeleeAttack");
                    animador.SetFloat("meleeID", (float)currentTool);
                }
                MeleeAttack(currentTool);
            }
        }
        else
        {
            // Si el jugador está fuera del rango, detiene animación de disparo
            if (animador != null)
                animador.SetBool("isShooting", false);
        }

        // 👊 Mostrar segundo frame del golpe (efecto visual del puñete)
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

    void StopShootingAnimation()
    {
        if (animador != null)
            animador.SetBool("isShooting", false);
    }

    IEnumerator DisparoUnico(WeaponType weapon)
    {
        float rate = fireRates.ContainsKey(weapon) ? fireRates[weapon] : 0.25f;
        int cantidad = bulletsPerClick.ContainsKey(weapon) ? bulletsPerClick[weapon] : 1;

        yield return new WaitForSeconds(0.05f);

        for (int i = 0; i < cantidad; i++)
        {
            Shoot(weapon);
            yield return new WaitForSeconds(rate);
        }
    }

    void Shoot(WeaponType weapon)
    {
        if (currentAmmo[weapon] <= 0)
        {
            currentAmmo[weapon] = 0;
            if (!weaponDroppedOnce)
            {
                weaponDroppedOnce = true;
                DropWeapon(weapon);
            }
            return;
        }

        GameObject bulletPrefab = GetBulletPrefab(weapon);
        if (bulletPrefab == null || firePoint == null) return;

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
        int damage = GetMeleeDamage(tool);
        Collider2D[] hits = Physics2D.OverlapCircleAll(meleePoint.position, meleeRange, damageableLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            HealthSystem target = hit.GetComponent<HealthSystem>();
            if (target != null)
                target.TakeDamage(damage, gameObject);
        }

        AudioClip sound = GetMeleeSound(tool);
        if (sound != null && audioSource != null)
            audioSource.PlayOneShot(sound);
    }

    int GetMeleeDamage(WeaponType tool)
    {
        switch (tool)
        {
            case WeaponType.Hacha: return damageHacha;
            case WeaponType.Machete: return damageMachete;
            case WeaponType.Sable: return damageSable;
            default: return damagePuñete;
        }
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
            default: return punheteSound;
        }
    }

    bool EsHerramienta(WeaponType w) => w == WeaponType.Hacha || w == WeaponType.Machete || w == WeaponType.Sable;

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
            default: return null;
        }
    }

    void DropWeapon(WeaponType weapon)
    {
        GameObject dropPrefab = GetDropPrefab(weapon);
        if (dropPrefab != null)
        {
            GameObject dropped = Instantiate(dropPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = dropped.GetComponent<Rigidbody2D>() ?? dropped.AddComponent<Rigidbody2D>();
            float direccion = transform.localScale.x > 0 ? 1f : -1f;
            rb.AddForce(new Vector2(direccion * 2f, 3f), ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);
            Destroy(dropped, 5f);
        }

        if (EsHerramienta(weapon)) currentTool = WeaponType.None;
        else currentWeapon = WeaponType.None;

        if (animador != null)
            animador.SetBool("isShooting", false);
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
            default: return null;
        }
    }

    void RestaurarAnimador()
    {
        if (animador != null) animador.enabled = true;
    }

    void LanzallamasRafaga()
    {
        if (lanzallamasBullet == null || firePoint == null) return;

        int cantidadLlamas = Random.Range(4, 7);
        for (int i = 0; i < cantidadLlamas; i++)
        {
            GameObject llama = Instantiate(lanzallamasBullet, firePoint.position, firePoint.rotation);

            BulletLife bl = llama.GetComponent<BulletLife>();
            if (bl != null) { bl.shooter = gameObject; bl.isFlamethrower = true; bl.damage = 5; }

            Rigidbody2D rb = llama.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float dir = transform.localScale.x > 0 ? 1f : -1f;
                rb.AddForce(new Vector2(dir * Random.Range(2.5f, 3.8f), Random.Range(0.8f, 2f)), ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-3f, 3f), ForceMode2D.Impulse);
                rb.gravityScale = 0.4f; rb.drag = 0.6f;
            }

            SpriteRenderer sr = llama.GetComponent<SpriteRenderer>();
            if (sr != null) StartCoroutine(FadeOut(sr, Random.Range(0.8f, 1.5f)));

            Destroy(llama, 2f);
        }

        if (audioSource != null && lanzallamasSound != null)
        {
            if (!audioSource.isPlaying) { audioSource.loop = true; audioSource.clip = lanzallamasSound; audioSource.Play(); }
        }
    }

    public void EquipWeapon(WeaponType newWeapon)
    {
        if (EsHerramienta(newWeapon))
        {
            currentTool = newWeapon;
            currentWeapon = WeaponType.None;
        }
        else
        {
            currentWeapon = newWeapon;
            currentTool = WeaponType.None;
        }

        if (currentAmmo.ContainsKey(newWeapon))
            currentAmmo[newWeapon] = maxAmmo[newWeapon];

        weaponDroppedOnce = false;

        Debug.Log($"🤖 {gameObject.name} ahora equipa {newWeapon}");
    }

    IEnumerator FadeOut(SpriteRenderer sr, float duration)
    {
        float t = 0f;
        Color start = sr.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            sr.color = new Color(start.r, start.g, start.b, Mathf.Lerp(start.a, 0, t / duration));
            yield return null;
        }
        Destroy(sr.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (meleePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
        }
    }
}
