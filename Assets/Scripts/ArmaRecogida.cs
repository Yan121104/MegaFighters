using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private bool jugadorCerca = false;
    private GameObject jugador;

    [Header("Efecto de Sonido al Recoger")]
    public AudioClip pickupSound;

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.X))
        {
            RecogerObjeto();
        }
    }

    private void RecogerObjeto()
    {
        if (jugador == null) return;

        WeaponController wc = jugador.GetComponent<WeaponController>();

        if (wc != null)
        {
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
                if (gameObject.name.ToLower().Contains("basuca"))
                    wc.EquipWeapon(WeaponController.WeaponType.Basuca);
                else if (gameObject.name.ToLower().Contains("desert"))
                    wc.EquipWeapon(WeaponController.WeaponType.Desert);
                else if (gameObject.name.ToLower().Contains("escopeta"))
                    wc.EquipWeapon(WeaponController.WeaponType.Escopeta);
                else if (gameObject.name.ToLower().Contains("lanzallamas"))
                    wc.EquipWeapon(WeaponController.WeaponType.Lanzallamas);
                else if (gameObject.name.ToLower().Contains("m4a1"))
                    wc.EquipWeapon(WeaponController.WeaponType.M4a1);
                else if (gameObject.name.ToLower().Contains("miniusi"))
                    wc.EquipWeapon(WeaponController.WeaponType.Miniusi);
                else if (gameObject.name.ToLower().Contains("usp"))
                    wc.EquipWeapon(WeaponController.WeaponType.Usp);
                else if (gameObject.name.ToLower().Contains("snyper"))
                    wc.EquipWeapon(WeaponController.WeaponType.Snyper);
            }

            // 🎵 Reproduce el sonido aunque el objeto se destruya
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);
            }

            Debug.Log($"{jugador.name} recogió {gameObject.name}");

            Destroy(gameObject, 0.5f); // le da un pequeño margen (50ms)
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<WeaponController>() != null)
        {
            jugadorCerca = true;
            jugador = collision.gameObject;
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
