using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera cameraCenital;
    public Camera cameraSeguimiento;
    public GameObject[] tractors;

    private int currentTractorIndex = 0; // Índice del tractor actual que sigue la cámara
    private bool isCenitalActive = true; // Bandera para controlar la cámara activa

    void Start()
    {
        // Hacer que solo la cámara cenital esté activa al inicio
        cameraCenital.enabled = true;
        cameraSeguimiento.enabled = false;
    }

    void Update()
    {
        // Cambiar entre cámaras con la tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isCenitalActive)
            {
                // Cambiar a la cámara de seguimiento
                SwitchToFollowCamera();
            }
            else
            {
                // Cambiar a la cámara cenital
                SwitchToCenitalCamera();
            }
        }
    }

    void SwitchToFollowCamera()
    {
        if (tractors.Length == 0) return;

        // Activar la cámara de seguimiento y desactivar la cenital
        cameraCenital.enabled = false;
        cameraSeguimiento.enabled = true;

        // Cambiar al siguiente tractor
        currentTractorIndex = (currentTractorIndex + 1) % tractors.Length;
        GameObject currentTractor = tractors[currentTractorIndex];

        // Hacer que la cámara sea hija del tractor y ajustar su posición
        cameraSeguimiento.transform.SetParent(currentTractor.transform);
        cameraSeguimiento.transform.localPosition = new Vector3(10f, 10f, 10f); // Posición relativa al tractor
        cameraSeguimiento.transform.localRotation = Quaternion.Euler(0, 0, 0); // Ángulo de 90 grados en el eje Y

        isCenitalActive = false;
    }


    void SwitchToCenitalCamera()
    {
        // Activar la cámara cenital y desactivar la de seguimiento
        cameraCenital.enabled = true;
        cameraSeguimiento.enabled = false;

        // Desanclar la cámara de seguimiento del tractor
        cameraSeguimiento.transform.SetParent(null);

        isCenitalActive = true;
    }
}
