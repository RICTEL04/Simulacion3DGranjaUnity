using UnityEngine;

public class CameraMover : MonoBehaviour
{


    public float moveSpeed = 10f; // Velocidad base de movimiento
    public float verticalMoveSpeed = 25f; // Velocidad de movimiento en eje Y (Q y E)

    private float minX, maxX, minZ, maxZ, minY, maxY;

    [SerializeField] private PlacementSystem placementSystem;


    void Start()
    {
        Initialize(placementSystem);
    }
    public void Initialize(PlacementSystem placementSystem)
    {
        this.placementSystem = placementSystem;

        // Calcula los límites basados en el gridSize del PlacementSystem
        Vector2Int gridSize = placementSystem.gridSize;

        minX = -gridSize.x / 2f;
        maxX = gridSize.x / 2f;
        minZ = -gridSize.y / 2f;
        maxZ = gridSize.y / 2f;

        // Configurar límites en el eje Y
        minY = 10f; // Límite mínimo en altura (Y)
        maxY = 120f; // Límite máximo en altura (Y)
        transform.position = new Vector3(transform.position.x, 70f, transform.position.z);
    }


    void Update()
    {
        // Obtener la entrada horizontal (A/D o Flechas Izquierda/Derecha)
        float horizontalInput = Input.GetAxis("Horizontal");

        // Obtener la entrada vertical (W/S o Flechas Arriba/Abajo)
        float verticalInput = Input.GetAxis("Vertical");

        float verticalYInput = 0f;

        // Comprobar las teclas para mover en el eje Y
        if (Input.GetKey(KeyCode.Q))
        {
            verticalYInput = -1f; // Mover hacia abajo
        }
        else if (Input.GetKey(KeyCode.E))
        {
            verticalYInput = 1f; // Mover hacia arriba
        }


        // Calcular el nuevo movimiento basado en las entradas
        Vector3 newPosition = transform.position + new Vector3(
            horizontalInput * moveSpeed * Time.deltaTime, 
            verticalYInput * verticalMoveSpeed * Time.deltaTime, 
            verticalInput * moveSpeed * Time.deltaTime);

        // Restringir la posición de la cámara dentro de los límites
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // Aplicar el nuevo movimiento a la cámara
        transform.position = newPosition;
    }
}