using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TractorAgent : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private FieldManager[] fieldManagers;
    private FieldManager assignedField;
    private GameObject[] dischargePonits;  // Puntos de descarga
    private Vector3 initialPosition;
    // Velocidad de movimiento del tractor
    public float moveSpeed = 2.0f;
    public float unloadTime = 3f;

    void Start()
{
    navMeshAgent = GetComponent<NavMeshAgent>();
    
    if (navMeshAgent != null)
    {
        // Configuraciones importantes
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = 0.5f; // Distancia para considerar "llegado"
        navMeshAgent.angularSpeed = 120f; // Velocidad de giro
        navMeshAgent.acceleration = 8f; // Aceleración
    }
    else
    {
        Debug.LogError($"NavMeshAgent no encontrado en {gameObject.name}");
        enabled = false;
        return;
    }
    initialPosition = transform.position;

    fieldManagers = Object.FindObjectsByType<FieldManager>(FindObjectsSortMode.None);
    // Buscar puntos de descarga con el tag específico
    dischargePonits = GameObject.FindGameObjectsWithTag("Descarga");
    StartCoroutine(ManageFields());
}

    // Método para moverse al destino usando NavMesh
    public void GetToTargetPosition(Vector3 targetPosition)
    {
        if (navMeshAgent != null)
        {
            // Establecer el destino
            navMeshAgent.SetDestination(targetPosition);
        }
        else
        {
            Debug.LogError("NavMeshAgent no está disponible");
        }
    }

    IEnumerator ManageFields()
    {
        while (true)
        {
            // Verificar si el campo asignado está completamente cosechado
            if (assignedField != null && assignedField.IsFieldFullyHarvested())
            {
                Debug.Log($"Tractor: {gameObject.name} terminó con {assignedField.name}");
                
                // Buscar el punto de descarga más cercano
                GameObject closestDischargePoint = FindClosestDischargePoint();
                
                if (closestDischargePoint != null)
                {
                    // Moverse al punto de descarga
                    GetToTargetPosition(closestDischargePoint.transform.position);
                    
                    // Esperar a llegar al punto de descarga
                    yield return new WaitUntil(() => 
                        Vector3.Distance(transform.position, closestDischargePoint.transform.position) <= navMeshAgent.stoppingDistance
                    );
                    
                    // Simular descarga
                    yield return new WaitForSeconds(unloadTime);
                    Debug.Log($"{gameObject.name} ha descargado su carga.");
                }

                // Liberar el campo actual
                assignedField = null;
            }

            // Intentar asignar un nuevo campo si no hay ninguno asignado
            if (assignedField == null)
            {
                FieldManager bestField = null;
                float closestDistance = Mathf.Infinity;

                foreach (var field in fieldManagers)
                {
                    if (!field.IsAssigned && !field.IsFieldFullyHarvested())
                    {
                        // Calcular la distancia entre el tractor y el campo
                        float distance = Vector3.Distance(transform.position, field.transform.position);

                        // Seleccionar el campo más cercano
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            bestField = field;
                        }
                    }
                }

                if (bestField != null)
                {
                    assignedField = bestField;
                    bestField.AssignTractor(gameObject);
                    Debug.Log($"{gameObject.name} asignado al campo más cercano: {bestField.name}");
                }
            }

            // Si no hay campos disponibles, espera antes de volver a intentar
            if (assignedField == null)
            {
                if (fieldManagers.Length > 0 && 
                System.Array.TrueForAll(fieldManagers, field => field.IsFieldFullyHarvested()))
            {
                Debug.Log($"{gameObject.name} ha terminado todos los campos. Regresando a posición inicial.");
                
                // Moverse a la posición inicial
                GetToTargetPosition(initialPosition);
                
                // Esperar hasta llegar a la posición inicial
                yield return new WaitUntil(() => 
                    Vector3.Distance(transform.position, initialPosition) <= navMeshAgent.stoppingDistance
                );
                
                // Opcional: Añadir un tiempo de espera o detener el movimiento
                yield return new WaitForSeconds(2f);
            }
                Debug.Log($"{gameObject.name} no encontró campos disponibles. Intentando nuevamente...");
                yield return new WaitForSeconds(5f); // Tiempo de espera entre intentos
            }
            else
            {
                // Esperar un momento antes de verificar el progreso del campo asignado
                yield return new WaitForSeconds(1f);
            }
        }
    }

    // Método para encontrar el punto de descarga más cercano
    private GameObject FindClosestDischargePoint()
    {
        GameObject closestPoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject dischargePoint in dischargePonits)
        {
            float distance = Vector3.Distance(transform.position, dischargePoint.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = dischargePoint;
            }
        }

        return closestPoint;
    }
}
