using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public GameObject targetObject; // El objeto que seleccionas para que la luz lo rodee
    public Light pointLight; // La Point Light que usará el ciclo
    public float dayDuration = 60f; // Duración de un ciclo completo (día+noche) en segundos
    public Color dayColor = Color.white; // Color de luz de día
    public Color nightColor = Color.blue; // Color de luz de noche
    public float minIntensity = 0f; // Intensidad mínima para la noche
    public float maxIntensity = 2f; // Intensidad máxima para el día
    public float lightRadius = 10f; // Radio de alcance de la luz

    private float timeElapsed;

    void Start()
    {
        if (pointLight == null)
        {
            Debug.LogError("No se ha asignado la Point Light.");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError("No se ha asignado el objeto objetivo.");
            return;
        }

        // Configurar inicialmente la posición de la luz
        MoveLight();
    }

    void Update()
    {
        if (pointLight == null || targetObject == null) return;

        // Actualizar el tiempo transcurrido
        timeElapsed += Time.deltaTime;

        // Calcular la proporción del ciclo (0 a 1)
        float cycleRatio = (timeElapsed % dayDuration) / dayDuration;

        // Ajustar intensidad y color basados en el ciclo
        if (cycleRatio <= 0.5f) // Primera mitad: amanecer a día
        {
            float t = cycleRatio * 2f; // Escalar para que vaya de 0 a 1
            pointLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            pointLight.color = Color.Lerp(nightColor, dayColor, t);
        }
        else // Segunda mitad: atardecer a noche
        {
            float t = (cycleRatio - 0.5f) * 2f; // Escalar para que vaya de 0 a 1
            pointLight.intensity = Mathf.Lerp(maxIntensity, minIntensity, t);
            pointLight.color = Color.Lerp(dayColor, nightColor, t);
        }

        // Asegurar que la luz esté en la posición correcta
        MoveLight();
    }

    void MoveLight()
    {
        // Colocar la luz en el centro del objeto objetivo
        Vector3 targetPosition = targetObject.transform.position;
        pointLight.transform.position = targetPosition;

        // Configurar el alcance de la luz para iluminar el área alrededor
        pointLight.range = lightRadius;
    }
}
