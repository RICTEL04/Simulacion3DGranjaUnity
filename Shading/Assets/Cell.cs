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
        // Obtén el componente Renderer al inicio
        cellRenderer = GetComponent<Renderer>();

        // Asegúrate de que el Renderer exista
        if (cellRenderer == null)
        {
            Debug.LogError($"No se encontró un Renderer en {gameObject.name}");
        }
    }

    void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Tractor") && !isHarvested)
    {
        HarvestCell();
        fieldManager?.MarkCellAsHarvested(row, col); // Notifica al FieldManager
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
        cellRenderer.material.color = harvestedColor;
        Debug.Log($"Celda {gameObject.name} cosechada visualmente.");
    }
}


}
