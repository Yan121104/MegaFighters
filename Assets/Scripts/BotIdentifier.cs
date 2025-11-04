using TMPro;
using UnityEngine;

public class BotIdentifier : MonoBehaviour
{
    public TextMeshProUGUI etiquetaTexto; // Referencia al texto del Canvas

    public void SetEtiqueta(string nombre)
    {
        if (etiquetaTexto != null)
            etiquetaTexto.text = nombre;
    }
}
