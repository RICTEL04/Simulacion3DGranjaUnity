using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private PlacementDataSO placementDataSO;

    [SerializeField] 
    private InputManager inputManager;

    [SerializeField] 
    private Grid grid;

    [SerializeField]
    public Vector2Int gridSize = new Vector2Int(10, 10);

    [SerializeField] 
    private ObjectsDatabaseSO database;

    [SerializeField] private GameObject gridVisualization;

    public GridData floorData, furnitureData;

    public int cantidadTractores = 0, cantidadDischarge = 0, cantidadCultivos = 0;
    [SerializeField]
    private PreviewSystem preview;

    

    private Vector3Int lastDetectedPosition = Vector3Int.zero;

    [SerializeField] public ObjectPlacer objectPlacer;

    IBuildingState buildingState;

    [SerializeField] private GameObject terrain;

    [SerializeField] private Material material;

    private Vector3 cellSize = new Vector3(1f,1f,1f); // Tamaño de cada celda

    public Vector3 startRightPosition;
    public Vector3Int startGridPosition;

    private string click = "PlaceStructure";

    private void Start()
    {
        UpdateScales();
        StopPlacement();
        floorData = new();
        furnitureData = new();
        if (objectPlacer != null)
        {
            DontDestroyOnLoad(objectPlacer.gameObject);
        }

    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, floorData, furnitureData, objectPlacer);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnRightClickHoldStart += HandleRightClickHoldStart;
        inputManager.OnRightClickHold += HandleRightClickHold;
        inputManager.OnRightClickReleased += HandleRightClickReleased;
        inputManager.OnExit += StopPlacement;


    }

    public void StartRemoving()
    {

        StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new RemovingState(grid, preview,
            floorData, furnitureData, objectPlacer);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;

    }


    private void HandleRightClickHoldStart()
    {
        if (inputManager.IsPointerOverUI())
            return;

        // Inicia el proceso del clic derecho.
        click = "RightClickHoldStart";
        startRightPosition = inputManager.GetSelectedMapPosition();
        startGridPosition = grid.WorldToCell(startRightPosition);

        Debug.Log($"Right Click Hold Start at Grid Position: {startGridPosition}");
        buildingState.OnAction(startGridPosition, gridSize, click , startGridPosition);
    }

    private void HandleRightClickHold()
    {
        if (inputManager.IsPointerOverUI())
            return;
        click = "RightClickHold";
        // Detecta y procesa el movimiento durante el clic derecho.
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        Debug.Log($"Right Click Hold Move to Grid Position: {gridPosition}");
        buildingState.OnAction(gridPosition, gridSize, click, startGridPosition);
    }

    private void HandleRightClickReleased()
    {
        if (inputManager.IsPointerOverUI())
            return;
        click = "RightClickReleased";
        // Finaliza el proceso del clic derecho.
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        Debug.Log($"Right Click Released at Grid Position: {gridPosition}");
        buildingState.OnAction(gridPosition, gridSize, click, startGridPosition);
    }



    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }
        click = "PlaceStructure";
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        buildingState.OnAction(gridPosition, gridSize, click , startGridPosition);

    }

    private void StopPlacement()
    {
        if (buildingState == null) return;
        gridVisualization.SetActive(false);
        buildingState.EndState();
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnRightClickHoldStart -= HandleRightClickHoldStart;
        inputManager.OnRightClickHold -= HandleRightClickHold;
        inputManager.OnRightClickReleased -= HandleRightClickReleased;
        inputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        buildingState = null;
    }

    private void Update()
    {
        if (buildingState == null)
            return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        if(lastDetectedPosition != gridPosition)
        {
            
            buildingState.UpdateState(gridPosition, gridSize, click, startGridPosition);

            lastDetectedPosition = gridPosition;

        }

        /*
        if (objectPlacer.placedGameObjects != null && furnitureData.indexToPlacementData != null)
        {
            Debug.Log($"ObjectPlacer: {objectPlacer.placedGameObjects.Count}");
            Debug.Log($"Diccionario: {furnitureData.indexToPlacementData.Count}");
        }
        */


    }

    private void UpdateScales()
    {
        cellSize = grid.cellSize; // Obtiene el tamaño actual de la celda.
        Debug.Log($"Actualizando escalas: cellSize = {cellSize}");

        // Calcular el tamaño total de la cuadrícula.
        Vector3 gridDimensions = new Vector3(
            gridSize.x * cellSize.x/10,  // Ancho total
            1,                        // Altura fija (ya que no importa)
            gridSize.y * cellSize.z/10   // Profundidad total
        );



        // Verificar si alguna dimensión es impar y mover 0.5 veces el tamaño de la celda.
        Vector3 offset = new Vector3(0f,0f,0f);

        if (gridSize.x % 2 != 0) // Si el ancho (X) es impar
        {
            offset.x = (0.5f * cellSize.x);
        }

        if (gridSize.y % 2 != 0) // Si la profundidad (Y) es impar
        {
            offset.z = (0.5f * cellSize.z);
        }

        if (gridVisualization != null)
        {
            gridVisualization.transform.localScale = new Vector3(
                gridDimensions.x,
                1,
                gridDimensions.z
            );
            // Aplicar el desplazamiento a gridVisualization
            gridVisualization.transform.position = offset;
        }

        // Ajustar la escala del terreno y moverlo si es necesario.
        if (terrain != null)
        {
            terrain.transform.localScale = new Vector3(
                gridDimensions.x,
                1,
                gridDimensions.z
            );
            // Aplicar el desplazamiento a terrain
            terrain.transform.position = offset;
        }


        // Ajustar la escala del material.
        if (material != null)
        {
            Vector2 defaultSize = new Vector2(1f, 1f);
            material.SetVector("_Size", new Vector4(
                defaultSize.x / cellSize.x ,
                defaultSize.y / cellSize.z ,
                0, 0
            ));
        }

        // Actualizar la escala del CursorIndicator en el PreviewSystem.
        if (preview != null)
        {
            preview.UpdateCursorScale(cellSize);
            Debug.Log("Se actualizó la escala del CursorIndicator.");
        }
    }


    public void SavePlacementData()
    {
        if (placementDataSO == null)
        {
            Debug.LogWarning("No ScriptableObject assigned to store placement data.");
            return;
        }

        // Guardar los datos relevantes en el ScriptableObject


        for (int i = 0; i < objectPlacer.placedGameObjectsPositions.Count; i++)
        {
            if (furnitureData.indexToPlacementData.ContainsKey(i))
            {
                Vector3Int startPosition = objectPlacer.placedGameObjectsPositions[i].Item1;
                Vector3Int gridPosition = objectPlacer.placedGameObjectsPositions[i].Item2;

                int width = Mathf.Abs(gridPosition.x - startPosition.x) + 1; // +1 para incluir la celda inicial
                int length = Mathf.Abs(gridPosition.z - startPosition.z) + 1;

                int minX = Mathf.Min(startPosition.x, gridPosition.x);
                int maxX = Mathf.Max(startPosition.x, gridPosition.x);
                int minZ = Mathf.Min(startPosition.z, gridPosition.z);
                int maxZ = Mathf.Max(startPosition.z, gridPosition.z);

                int objectID = furnitureData.indexToPlacementData[i].ID;
                string objectName = database.objectsData[objectID].Name;
                Vector2Int objectSize = database.objectsData[objectID].Size;
                List<Vector3Int> positionsToObjects = new List<Vector3Int>();

                for (int x = minX; x <= maxX; x += objectSize.x)
                {
                    for (int z = minZ; z <= maxZ; z += objectSize.y)
                    {
                        if (objectName == "Parcela")
                        {
                            cantidadCultivos++;
                        }
                        else if (objectName == "Tractor")
                        {
                            cantidadTractores++;
                        }
                        else if (objectName == "Discharge")
                        {
                            cantidadDischarge++;
                        }
                    }
                }

            }
        }



        if (cantidadCultivos < 1 || cantidadDischarge < 1 || cantidadTractores < 1)
        {
            cantidadTractores = 0;
            cantidadDischarge = 0;
            cantidadCultivos = 0;
            return;
        }
        

        placementDataSO.furnitureData = furnitureData;
        placementDataSO.placedGameObjectsPositions = objectPlacer.placedGameObjectsPositions;
        placementDataSO.gridSize = gridSize;
        placementDataSO.cellSize = cellSize;
        placementDataSO.database = database;


        Debug.Log($"Cantidad de tractores: {cantidadTractores} ");
        Debug.Log($"Cantidad de Cultivos: {cantidadCultivos} ");
        Debug.Log($"Cantidad de Discharge: {cantidadDischarge} ");

        Debug.Log("Placement data saved to ScriptableObject!");
        SceneManager.LoadScene("SampleScene");
    }

}
