using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera cameraCenital;
    public Camera cameraSeguimiento;
    public GameObject[] tractors;

    private int currentTractorIndex = 0; // �ndice del tractor actual que sigue la c�mara
    private bool isCenitalActive = true; // Bandera para controlar la c�mara activa

    void Start()
    {
        // Hacer que solo la c�mara cenital est� activa al inicio
        cameraCenital.enabled = true;
        cameraSeguimiento.enabled = false;
    }

    void Update()
    {
        // Cambiar entre c�maras con la tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isCenitalActive)
            {
                // Cambiar a la c�mara de seguimiento
                SwitchToFollowCamera();
            }
            else
            {
                // Cambiar a la c�mara cenital
                SwitchToCenitalCamera();
            }
        }
    }

    void SwitchToFollowCamera()
    {
        if (tractors.Length == 0) return;

        // Activar la c�mara de seguimiento y desactivar la cenital
        cameraCenital.enabled = false;
        cameraSeguimiento.enabled = true;

        // Cambiar al siguiente tractor
        currentTractorIndex = (currentTractorIndex + 1) % tractors.Length;
        GameObject currentTractor = tractors[currentTractorIndex];

        // Hacer que la c�mara sea hija del tractor y ajustar su posici�n
        cameraSeguimiento.transform.SetParent(currentTractor.transform);
        cameraSeguimiento.transform.localPosition = new Vector3(10f, 10f, 10f); // Posici�n relativa al tractor
        cameraSeguimiento.transform.localRotation = Quaternion.Euler(0, 0, 0); // �ngulo de 90 grados en el eje Y

        isCenitalActive = false;
    }


    void SwitchToCenitalCamera()
    {
        // Activar la c�mara cenital y desactivar la de seguimiento
        cameraCenital.enabled = true;
        cameraSeguimiento.enabled = false;

        // Desanclar la c�mara de seguimiento del tractor
        cameraSeguimiento.transform.SetParent(null);

        isCenitalActive = true;
    }
}
