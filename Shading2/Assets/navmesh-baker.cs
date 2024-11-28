using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface surface;
    public bool startProcess = false;
    void Start()
    {
        // Si no se ha asignado manualmente, buscar el componente NavMeshSurface en este GameObject
        if (surface == null)
        {
            surface = GetComponent<NavMeshSurface>();
        }

        // Si aún no se encuentra, intentar agregarlo
        if (surface == null)
        {
            surface = gameObject.AddComponent<NavMeshSurface>();
        }


    }


    void Update()
    {
        if (startProcess)
        {           
            // Configurar y hacer bake del NavMesh
            if (surface != null)
            {
                // Intentar configuraciones básicas
                surface.BuildNavMesh();

                Debug.Log("NavMesh baked successfully at start.");
            }
            else
            {
                Debug.LogError("No se pudo encontrar o crear un componente NavMeshSurface.");
            }

            startProcess = false;
        }
    }
}