using System.Collections.Generic;
using UnityEngine;

public class CreateBuild : MonoBehaviour
{
    [SerializeField] private PlacementDataSO placementDataSO;
    [SerializeField] private GameObject terrain;
    [SerializeField] public ObjectsDatabaseSO database;

    [SerializeField]
    public NavMeshBaker navMeshBaker;

    public Vector3 cellSize = new Vector3(1f, 1f, 1f); // Tamaño de cada celda

    private List<Vector3> tractorPositions = new List<Vector3>();
    private List<Vector3> cropPositions = new List<Vector3>();
    private List<Vector3> dischargePositions = new List<Vector3>();
    public GameObject[] tractors;

    [SerializeField] private CameraSwitcher cambiocamara;

    void Start()
    {
        UpdateScales();
        AddElements();
        InstantiateObjects();

        navMeshBaker.startProcess = true;
    }

    void Update()
    {
    }

    void AddElements()
    {
        Debug.Log($"Tamaño de la lista: {placementDataSO.placedGameObjectsPositions.Count} ");
        for (int i = 0; i < placementDataSO.placedGameObjectsPositions.Count; i++)
        {
            if (!placementDataSO.furnitureData.indexToPlacementData.ContainsKey(i)) continue ;

            if (placementDataSO.furnitureData.indexToPlacementData[i] != null)
            {
                Vector3Int startPosition = placementDataSO.placedGameObjectsPositions[i].Item1;
                Vector3Int gridPosition = placementDataSO.placedGameObjectsPositions[i].Item2;

                float xDiferencia = 0;
                float zDiferencia = 0;

                int width = Mathf.Abs(gridPosition.x - startPosition.x) + 1; // +1 para incluir la celda inicial
                int length = Mathf.Abs(gridPosition.z - startPosition.z) + 1;

                int minX = Mathf.Min(startPosition.x, gridPosition.x);
                int maxX = Mathf.Max(startPosition.x, gridPosition.x);
                int minZ = Mathf.Min(startPosition.z, gridPosition.z);
                int maxZ = Mathf.Max(startPosition.z, gridPosition.z);


                
                int objectID = placementDataSO.furnitureData.indexToPlacementData[i].ID;
                if (objectID < 0) continue;

                GameObject objectPrefab = database.objectsData[objectID].Prefab;
                Vector2Int objectSize = database.objectsData[objectID].Size;
                string objectName = database.objectsData[objectID].Name;

                // Ajustar las diferencias según el tipo de objeto
                if (objectName == "Parcela")
                {
                    xDiferencia = (-11.5f - 6.5f - 3 - 1 - 4);
                    zDiferencia = (-2.5f - 4 - 1 - 4);
                }
                else if (objectName == "Tractor")
                {
                    xDiferencia = 0;
                    zDiferencia = -8;
                }
                else if (objectName == "Discharge")
                {
                    xDiferencia = (-4);
                    zDiferencia = (-7);
                }

                List<Vector3Int> positionsToObjects = new List<Vector3Int>();

                for (int x = minX; x <= maxX; x += objectSize.x)
                {
                    for (int z = minZ; z <= maxZ; z += objectSize.y)
                    {
                        positionsToObjects.Add(new Vector3Int(x, 0, z));
                        if (objectName == "Parcela")
                        {
                            Debug.Log("Aqui va una parcela");
                        }
                        else if (objectName == "Tractor")
                        {
                            Debug.Log("Aqui va un tractor");
                        }
                        else if (objectName == "Discharge")
                        {
                            Debug.Log("Aqui va una discharge");
                        }
                    }
                }

                foreach (var position in positionsToObjects)
                {
                    Vector3 fixedPosition = new Vector3
                    (
                        -1 * (position.x * cellSize.x) + xDiferencia,
                        0,
                        -1 * (position.z * cellSize.y) + zDiferencia
                    );

                    // Guardar la posición en la lista correspondiente según el tipo de objeto
                    if (objectName == "Parcela")
                    {
                        cropPositions.Add(fixedPosition);
                    }
                    else if (objectName == "Tractor")
                    {
                        tractorPositions.Add(fixedPosition);
                    }
                    else if (objectName == "Discharge")
                    {
                        dischargePositions.Add(fixedPosition);
                    }
                }
            }
        }
    }

    void InstantiateObjects()
    {
        // Instanciar cultivos
        foreach (var position in cropPositions)
        {
            InstantiateObjectAtPosition(position, "Parcela");
        }

        // Instanciar discharge
        foreach (var position in dischargePositions)
        {
            InstantiateObjectAtPosition(position, "Discharge");
        }

        // Instanciar tractores
        foreach (var position in tractorPositions)
        {
            InstantiateObjectAtPosition(position, "Tractor");
        }

        tractors = GameObject.FindGameObjectsWithTag("Truck");

        if (cambiocamara != null)
        {
            cambiocamara.tractors = tractors;
        }

       

    }

    private void InstantiateObjectAtPosition(Vector3 position, string objectName)
    {
        int objectID = database.objectsData.FindIndex(data => data.Name == objectName);
        if (objectID != -1)
        {
            GameObject objectPrefab = database.objectsData[objectID].Prefab;
            GameObject newObject = Instantiate(objectPrefab);
            newObject.transform.position = position;
        }
    }

    private void UpdateScales()
    {
        cellSize = placementDataSO.cellSize; // Obtiene el tamaño actual de la celda.
        Debug.Log($"Actualizando escalas: cellSize = {cellSize}");

        Vector3 gridDimensions = new Vector3(
            placementDataSO.gridSize.x * cellSize.x / 10,
            1,
            placementDataSO.gridSize.y * cellSize.z / 10
        );

        Vector3 offset = new Vector3(0f, 0f, 0f);

        if (placementDataSO.gridSize.x % 2 != 0)
        {
            offset.x = (0.5f * cellSize.x);
        }

        if (placementDataSO.gridSize.y % 2 != 0)
        {
            offset.z = (0.5f * cellSize.z);
        }

        if (terrain != null)
        {
            terrain.transform.localScale = new Vector3(
                gridDimensions.x + 5,
                1,
                gridDimensions.z + 5
            );
            terrain.transform.position = offset;
        }
    }
}
