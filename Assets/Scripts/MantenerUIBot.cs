using UnityEngine;

public class MantenerUIBot : MonoBehaviour
{
    private Vector3 escalaOriginal;
    private Transform padre;

    void Start()
    {
        padre = transform.parent;
        escalaOriginal = transform.localScale;
    }

    void LateUpdate()
    {
        if (padre == null) return;

        // Detecta si el padre (el bot) está invertido en X
        float direccion = Mathf.Sign(padre.localScale.x);

        // Si el bot está volteado, invierte la escala local del Canvas para compensar
        transform.localScale = new Vector3(
            escalaOriginal.x * direccion,
            escalaOriginal.y,
            escalaOriginal.z
        );

        // Mantiene la rotación en cero (Canvas nunca rota con el bot)
        transform.rotation = Quaternion.identity;
    }
}
