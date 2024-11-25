using System.Collections;
using UnityEngine;

public class TractorAgent : MonoBehaviour
{
    private FieldManager[] fieldManagers;
    private FieldManager assignedField;

    void Start()
    {
        fieldManagers = Object.FindObjectsByType<FieldManager>(FindObjectsSortMode.None);
        StartCoroutine(ManageFields());
    }

    IEnumerator ManageFields()
{
    
    while (true)
    {
        // Verificar si el campo asignado está completamente cosechado
        if (assignedField != null && assignedField.IsFieldFullyHarvested())
        {
            Debug.Log($"Tractor: {gameObject.name} terminó con {assignedField.name}");
            assignedField = null; // Liberar el campo asignado
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
}
