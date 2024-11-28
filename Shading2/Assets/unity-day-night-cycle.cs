using UnityEngine;
using System.Collections;

public class RealisticSolarCycle : MonoBehaviour
{
    [Header("Solar Movement")]
    public float cycleDuration = 120f; // Duración del ciclo completo en segundos
    public float sunPitch = 50f; // Ángulo de inclinación del sol

    [Header("Lighting")]
    public Light directionalLight;
    public Light pointLight; // Luz puntual para efecto adicional
    public GameObject targetObject; // Objeto que la luz puntual rodeará

    [Header("Light Settings")]
    public float lightRadius = 10f; // Radio de alcance de la luz puntual
    public Color dayColor = Color.white; // Color de luz de día
    public Color nightColor = Color.blue; // Color de luz de noche
    public float minIntensity = 0f; // Intensidad mínima para la noche
    public float maxIntensity = 2f; // Intensidad máxima para el día

    [Header("Color Gradients")]
    public Gradient sunLightColor;
    public Gradient skyColor;

    [Header("Night Objects")]
    public GameObject[] nightObjects;

    [Range(0f, 24f)]
    private float timeOfDay = 12f;

    void Start()
    {
        // Inicializar objetos nocturnos
        nightObjects = GameObject.FindGameObjectsWithTag("Night");
        StartCoroutine(DelayedStart());

        // Validar Point Light
        if (pointLight == null || targetObject == null)
        {
            Debug.LogError("Asegúrate de asignar la Point Light y el objeto objetivo.");
            return;
        }

        MovePointLight(); // Configurar posición inicial de la luz puntual
    }

    // Corutina para añadir un delay de 5 segundos
    IEnumerator DelayedStart()
    {
        // Esperar 5 segundos
        yield return new WaitForSeconds(1.5f);

        // Buscar y añadir automáticamente todos los objetos con tag "night"
        nightObjects = GameObject.FindGameObjectsWithTag("Night");
        UpdateNightObjects();
    }

    void Update()
    {
        // Actualizar tiempo
        timeOfDay += Time.deltaTime * (24f / cycleDuration);
        if (timeOfDay >= 24f) timeOfDay = 0f;

        UpdateSolarMovement();
        UpdatePointLight();
        UpdateNightObjects();
    }

    void UpdateSolarMovement()
    {
        // Normalizar tiempo entre 0 y 1
        float normalizedTime = timeOfDay / 24f;

        // Calcular rotación del sol
        float sunAngle = normalizedTime * 360f;

        // Rotación más realista considerando inclinación
        Quaternion sunRotation = Quaternion.Euler(
            sunPitch * Mathf.Sin(normalizedTime * Mathf.PI * 2), // Movimiento vertical
            sunAngle, // Rotación horizontal
            0
        );

        // Aplicar rotación a la luz direccional
        directionalLight.transform.rotation = sunRotation;

        // Ajustar color de la luz direccional
        directionalLight.color = sunLightColor.Evaluate(normalizedTime);

        // Ajustar intensidad de la luz direccional
        float sunHeight = Mathf.Sin(normalizedTime * Mathf.PI * 2);
        directionalLight.intensity = Mathf.Clamp01(sunHeight + 0.2f);

        // Ajustar color del cielo
        RenderSettings.ambientSkyColor = skyColor.Evaluate(normalizedTime);
    }

    void UpdatePointLight()
    {
        // Calcular proporción del ciclo (0 a 1)
        float cycleRatio = timeOfDay / 24f;

        // Ajustar intensidad y color de la luz puntual
        if (cycleRatio <= 0.5f) // Amanecer a día
        {
            float t = cycleRatio * 2f; // Escalar para 0 a 1
            pointLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            pointLight.color = Color.Lerp(nightColor, dayColor, t);
        }
        else // Atardecer a noche
        {
            float t = (cycleRatio - 0.5f) * 2f; // Escalar para 0 a 1
            pointLight.intensity = Mathf.Lerp(maxIntensity, minIntensity, t);
            pointLight.color = Color.Lerp(dayColor, nightColor, t);
        }

        // Actualizar posición de la luz puntual
        MovePointLight();
    }

    void MovePointLight()
    {
        // Posicionar la luz puntual en el objeto objetivo
        pointLight.transform.position = targetObject.transform.position;

        // Configurar el alcance de la luz puntual
        pointLight.range = lightRadius;
    }

    void UpdateNightObjects()
    {
        // Detectar si es de noche (sol debajo del horizonte)
        bool isNight = directionalLight.transform.forward.y < 0;

        // Activar/desactivar objetos nocturnos
        foreach (GameObject obj in nightObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!isNight);
            }
        }
    }
}
