using UnityEngine;

public class InstantiatePrefabOnStart : MonoBehaviour
{
    // Asigna el prefab desde el Inspector
    [SerializeField] private GameObject prefabToInstantiate;

    void Start()
    {
        // Verifica si el prefab está asignado
        if (prefabToInstantiate != null)
        {
            // Instancia el prefab en la posición y rotación del objeto actual
            Instantiate(prefabToInstantiate, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogError("No se ha asignado ningún prefab en el Inspector.");
        }
    }
}
