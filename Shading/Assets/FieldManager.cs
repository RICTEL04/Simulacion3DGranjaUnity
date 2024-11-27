using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FieldManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject[] tractors;
    public int rows = 16;
    public int cols = 16;
    public float cellSize = 1.0f;
    public float moveSpeed = 2.0f;
    public int rowSkip = 3;

    private GameObject[,] grid;
    private Vector3 gridStartPosition;
    private bool[,] harvestedCells; // Estado de las celdas

    public bool IsAssigned { get; private set; } = false;

    void Start()
    {
        tractors = GameObject.FindGameObjectsWithTag("Truck");
        InitializeField();

        // Ensure harvestedCells is initialized even if InitializeField is not called
        if (harvestedCells == null)
        {
            harvestedCells = new bool[rows, cols];
        }
    }

    void InitializeField()
    {
        // Ensure harvestedCells is initialized
        if (harvestedCells == null)
        {
            harvestedCells = new bool[rows, cols];
        }

        gridStartPosition = this.transform.position;

        grid = new GameObject[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 position = gridStartPosition + new Vector3(col * cellSize, 0, -row * cellSize);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"Cell_{row}_{col}";
                grid[row, col] = cell;

                // Configura el Cell con su posición lógica
                Cell cellScript = cell.GetComponent<Cell>();
                if (cellScript != null)
                {
                    cellScript.Initialize(this, row, col);
                }

                // Ensure this specific cell is not marked as harvested initially
                harvestedCells[row, col] = false;
            }
        }
    }



    public void AssignTractor(GameObject tractor)
    {
        if (!IsAssigned)
        {
            IsAssigned = true;
            StartCoroutine(SimulateHarvest(tractor));
        }
    }

    public void ReleaseField()
    {
        IsAssigned = false;
        Debug.Log($"Campo {name} liberado.");
    }

    // Elimina los métodos MoveTractor y reemplázalos con la nueva lógica
    IEnumerator SimulateHarvest(GameObject tractor)
{
    TractorAgent tractorAgent = tractor.GetComponent<TractorAgent>();
    NavMeshAgent navMeshAgent = tractor.GetComponent<NavMeshAgent>();

    for (int row = 0; row < rows; row += rowSkip)
    {
        bool isEvenRow = (row % 2 == 0);
        int startCol = isEvenRow ? 0 : (cols - 1);
        int endCol = isEvenRow ? cols : -1;
        int step = isEvenRow ? 1 : -1;

        for (int col = startCol; col != endCol; col += step)
        {
            if (!harvestedCells[row, col])
            {
                Vector3 targetPosition = gridStartPosition + new Vector3(
                    col * cellSize, 
                    0.5f, 
                    -row * cellSize
                );

                tractorAgent.GetToTargetPosition(targetPosition);

                // Wait until the cell is harvested (trigger mechanism will handle marking)
                yield return new WaitUntil(() => harvestedCells[row, col]);

                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    ReleaseField();
    Debug.Log($"{gameObject.name} ha terminado de cosechar el campo {name}.");
}


    IEnumerator MoveTractor(GameObject tractor, Vector3 targetPosition)
{
    Vector3 startPosition = tractor.transform.position;
    Quaternion startRotation = tractor.transform.rotation;

    Vector3 direction = (targetPosition - startPosition).normalized;
    Quaternion targetRotation = Quaternion.LookRotation(direction);

    float journeyLength = Vector3.Distance(startPosition, targetPosition);
    float startTime = Time.time;

    // Rotation duration (adjust this value to control rotation speed)
    float rotationDuration = 0.5f;
    float rotationStartTime = Time.time;

    float distanceCovered = 0f;
    while (distanceCovered < journeyLength)
    {
        // Calculate rotation progress
        float rotationProgress = Mathf.Clamp01((Time.time - rotationStartTime) / rotationDuration);
        
        // Smoothly rotate the tractor
        tractor.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationProgress);

        // Calculate the fraction of the journey completed
        float fractionOfJourney = distanceCovered / journeyLength;

        // Move the tractor
        tractor.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

        // Calculate distance covered based on actual move speed
        distanceCovered = (Time.time - startTime) * moveSpeed;

        yield return null;
    }

    // Ensure the tractor reaches the exact target position and rotation
    tractor.transform.position = targetPosition;
    tractor.transform.rotation = targetRotation;
}

    public void MarkCellAsHarvested(int row, int col)
    {
        // Ensure the matrix is initialized before accessing
        if (harvestedCells == null)
        {
            harvestedCells = new bool[rows, cols];
        }

        if (row >= 0 && row < rows && col >= 0 && col < cols)
        {
            if (!harvestedCells[row, col])
            {
                harvestedCells[row, col] = true;
                Debug.Log($"Celda [{row}, {col}] marcada como cosechada en {name}.");
            }
        }
        else
        {
            Debug.LogWarning($"Intento de marcar celda fuera de los límites: [{row}, {col}]");
        }
    }


    public bool IsFieldFullyHarvested()
    {
        // Check if the matrix is null or not the correct size
        if (harvestedCells == null || 
            harvestedCells.GetLength(0) != rows || 
            harvestedCells.GetLength(1) != cols)
        {
            // Reinitialize the matrix if it's invalid
            harvestedCells = new bool[rows, cols];
            Debug.Log("HarvestedCells matrix reinitialized.");
            return false;
        }

        // Verify if all cells are harvested
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (!harvestedCells[row, col])
                {
                    return false; // Found an unharvested cell
                }
            }
        }
        return true; // All cells are harvested
    }
}
