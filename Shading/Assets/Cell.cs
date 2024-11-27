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
            
            // En lugar de cambiar el color, destruimos el objeto
            Destroy(gameObject);
            
            Debug.Log($"Celda {gameObject.name} eliminada.");
        }
    }


}