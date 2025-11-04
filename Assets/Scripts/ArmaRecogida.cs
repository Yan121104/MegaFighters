using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private bool jugadorCerca = false;
    private GameObject jugador;

    [Header("Efecto de Sonido al Recoger")]
    public AudioClip pickupSound;

    void Update()
    {
        // 🎮 Solo el jugador humano necesita presionar X
        if (jugadorCerca && jugador != null && jugador.GetComponent<MovimientoBotInteligente>() == null)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                RecogerObjeto(jugador);
            }
        }
    }

    private void RecogerObjeto(GameObject receptor)
    {
        if (receptor == null) return;

        WeaponController wc = receptor.GetComponent<WeaponController>();
        if (wc == null) return;

        string[] herramientas = { "Hacha", "Sable", "Machete" };
        bool esHerramienta = false;

        // 🪓 Detectar herramientas cuerpo a cuerpo
        foreach (string herramienta in herramientas)
        {
            if (gameObject.name.ToLower().Contains(herramienta.ToLower()))
            {
                esHerramienta = true;
                wc.EquipWeapon((WeaponController.WeaponType)System.Enum.Parse(typeof(WeaponController.WeaponType), herramienta, true));
                break;
            }
        }

        // 🔫 Detectar armas de fuego
        if (!esHerramienta)
        {
            string n = gameObject.name.ToLower();
            if (n.Contains("basuca"))
                wc.EquipWeapon(WeaponController.WeaponType.Basuca);
            else if (n.Contains("desert"))
                wc.EquipWeapon(WeaponController.WeaponType.Desert);
            else if (n.Contains("escopeta"))
                wc.EquipWeapon(WeaponController.WeaponType.Escopeta);
            else if (n.Contains("lanzallamas"))
                wc.EquipWeapon(WeaponController.WeaponType.Lanzallamas);
            else if (n.Contains("m4a1"))
                wc.EquipWeapon(WeaponController.WeaponType.M4a1);
            else if (n.Contains("miniusi"))
                wc.EquipWeapon(WeaponController.WeaponType.Miniusi);
            else if (n.Contains("usp"))
                wc.EquipWeapon(WeaponController.WeaponType.Usp);
            else if (n.Contains("snyper"))
                wc.EquipWeapon(WeaponController.WeaponType.Snyper);
        }

        // 🎵 Reproduce sonido
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);

        Debug.Log($"{receptor.name} recogió {gameObject.name}");
        Destroy(gameObject, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🔍 Detectar si es jugador o bot
        WeaponController wc = collision.GetComponent<WeaponController>();
        if (wc != null)
        {
            jugadorCerca = true;
            jugador = collision.gameObject;

            // 🤖 Si es un bot, recoge automáticamente
            if (jugador.GetComponent<MovimientoBotInteligente>() != null)
            {
                RecogerObjeto(jugador);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<WeaponController>() != null)
        {
            jugadorCerca = false;
            jugador = null;
        }
    }
}
