using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class TractorAgent : MonoBehaviour
{
    public string customSavePath = "C:/MisProyectos/Trayectorias/";
    private NavMeshAgent navMeshAgent;
    private FieldManager[] fieldManagers;
    private FieldManager assignedField;
    private GameObject[] dischargePonits;  // Puntos de descarga
    private Vector3 initialPosition;
    // Velocidad de movimiento del tractor
    public float moveSpeed = 2.0f;
    public float unloadTime = 3f;

    // lista para rastrear la trayectoria
    private List<TrajectoryPoint> trajectory = new List<TrajectoryPoint>();

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

        // Iniciar el registro de trayectoria
        StartCoroutine(TrackTrajectory());
    }
    else
    {
        Debug.LogError($"NavMeshAgent no encontrado en {gameObject.name}");
        enabled = false;
        return;
    }
    initialPosition = transform.position;

    fieldManagers = UnityEngine.Object.FindObjectsByType<FieldManager>(FindObjectsSortMode.None);
    // Buscar puntos de descarga con el tag específico
    dischargePonits = GameObject.FindGameObjectsWithTag("Descarga");
    StartCoroutine(ManageFields());
}

    // Método para registrar la trayectoria
    IEnumerator TrackTrajectory()
    {
        while (true)
        {
            // Registrar la posición actual
            trajectory.Add(new TrajectoryPoint
            {
                timestamp = DateTime.Now.ToString("o"),
                position = transform.position,
                speed = navMeshAgent.velocity.magnitude
            });

            // Guardar trayectoria cada 10 segundos
            if (trajectory.Count % 10 == 0)
            {
                SaveTrajectoryToJson();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    // Método para guardar la trayectoria en un archivo JSON
    void SaveTrajectoryToJson()
{
    TractorTrajectory trajectoryData = new TractorTrajectory
    {
        tractorName = gameObject.name,
        points = trajectory.ToArray()
    };

    // Create the JSON string here
    string json = JsonUtility.ToJson(trajectoryData, true);

    string filename = $"Trajectory_{gameObject.name}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
    
    try
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllText(path, json);
        Debug.Log($"Trayectoria guardada en: {path}");
    }
    catch (Exception e)
    {
        Debug.LogError($"Error guardando trayectoria: {e.Message}");
    }
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
                SaveTrajectoryToJson();
                break;
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

    [Serializable]
    public class TrajectoryPoint
    {
        public string timestamp;
        public Vector3 position;
        public float speed;
    }

    [Serializable]
    public class TractorTrajectory
    {
        public string tractorName;
        public TrajectoryPoint[] points;
    }

    // Método para guardar la trayectoria final al terminar el campo
    void OnDestroy()
    {
        SaveTrajectoryToJson();
    }
}
