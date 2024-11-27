using UnityEngine;

public class Cell : MonoBehaviour
{
    public Color harvestedColor = Color.green; // Color para celdas cosechadas
    private Renderer cellRenderer; // Referencia al Renderer
    private bool isHarvested = false; // Estado de la celda
    private FieldManager fieldManager;
    private int row;
    private int col;    

    void Start()
    {
        // Add a trigger collider to the cell
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
            ((BoxCollider)collider).isTrigger = true;
        }
        else
        {
            collider.isTrigger = true;
        }

        cellRenderer = GetComponent<Renderer>();
        
        if (cellRenderer == null)
        {
            Debug.LogError($"No se encontró un Renderer en {gameObject.name}");
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Truck") && !isHarvested)
        {
            HarvestCell();
            fieldManager?.MarkCellAsHarvested(row, col);
        }
    }

    // Método para inicializar la celda
    public void Initialize(FieldManager manager, int rowIndex, int colIndex)
    {
        fieldManager = manager;
        row = rowIndex;
        col = colIndex;
    }

public void HarvestCell()
    {
        if (!isHarvested)
        {
            isHarvested = true;
            
            if (cellRenderer != null)
            {
                cellRenderer.material.color = harvestedColor;
            }
            
            Debug.Log($"Celda {gameObject.name} cosechada visualmente.");
        }
    }


}