using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private bool jugadorCerca = false;
    private GameObject jugador;

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.X))
        {
            RecogerObjeto();
        }
    }

    private void RecogerObjeto()
    {
        WeaponController wc = jugador.GetComponent<WeaponController>();

        if (wc != null)
        {
            string[] herramientas = { "Hacha", "Sable", "Machete" };
            bool esHerramienta = false;

            foreach (string herramienta in herramientas)
            {
                if (gameObject.name.ToLower().Contains(herramienta.ToLower()))
                {
                    esHerramienta = true;
                    wc.EquipWeapon((WeaponController.WeaponType)System.Enum.Parse(typeof(WeaponController.WeaponType), herramienta, true));
                    break;
                }
            }

            if (!esHerramienta)
            {
                // armas de fuego
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
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = true;
            jugador = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = false;
            jugador = null;
        }
    }
}
