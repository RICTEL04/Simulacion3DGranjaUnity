using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlacementState : IBuildingState
{
    

    private int selectedObjectIndex = -1; 
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    ObjectsDatabaseSO database;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;

    private List<Vector3Int> temporaryPositions; // Posiciones calculadas temporalmente
    private bool isPlacementValid; // Bandera de validación de colocación

    public PlacementState(int iD,
        Grid grid,
        PreviewSystem previewSystem,
        ObjectsDatabaseSO database,
        GridData floorData,
        GridData furnitureData,
        ObjectPlacer objectPlacer)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;


        temporaryPositions = new List<Vector3Int>();
        isPlacementValid = false;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
        }
        else
        {
            throw new System.Exception($"No object with ID {iD}");
        }

    }


    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    private List<Vector3Int> CalculateGridPositions(Vector3Int gridPosition, Vector3Int startPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        int minX = Mathf.Min(startPosition.x, gridPosition.x);
        int maxX = Mathf.Max(startPosition.x, gridPosition.x);
        int minZ = Mathf.Min(startPosition.z, gridPosition.z);
        int maxZ = Mathf.Max(startPosition.z, gridPosition.z);

        for (int x = minX; x <= maxX; x += objectSize.x)
        {
            for (int z = minZ; z <= maxZ; z += objectSize.y)
            {
                positions.Add(new Vector3Int(x, 0, z));
            }
        }

        return positions;
    }

    public void OnAction(Vector3Int gridPosition, Vector2Int gridSize, string actionSource, Vector3Int startPosition)
    {
        Vector2Int cellSize = new Vector2Int((int)grid.cellSize.x, (int)grid.cellSize.z);
        if (actionSource == "PlaceStructure")
        {

            if (!database.objectsData[selectedObjectIndex].isConjunto)
            {
                bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex, gridSize);

                if (!placementValidity) return;

                Vector3Int obSize = new Vector3Int(database.objectsData[selectedObjectIndex].Size.x, 0,
                    database.objectsData[selectedObjectIndex].Size.y);
                Vector3Int lastPosition = gridPosition + obSize - new Vector3Int(1,0,1);


                int index = objectPlacer.PlaceObject(database.objectsData[selectedObjectIndex].Prefab, gridPosition, lastPosition, grid);

                GridData selectedData = furnitureData;
                selectedData.AddObjectAt(gridPosition,
                    database.objectsData[selectedObjectIndex].Size,
                    database.objectsData[selectedObjectIndex].ID,
                    index, database.objectsData[selectedObjectIndex].Name
                );

            }
            previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);
        }
        else
        {
            if (actionSource == "RightClickHold")
            {
                //HandleRightClickHold(gridPosition, gridSize, startPosition);
                isPlacementValid = CheckMultiplePlacementValidity(gridPosition, selectedObjectIndex, gridSize, startPosition);
            }
            else if (actionSource == "RightClickReleased")
            {
                //HandleRightClickReleased(gridPosition, startPosition);
                Debug.Log($"PlacementValidity = {isPlacementValid}");
                if (!isPlacementValid) return;
                
                Vector2Int objectSize = database.objectsData[selectedObjectIndex].Size;
                // Coloca múltiples objetos y obtiene los índices generados
                List<int> indexes = objectPlacer.placeMultipleObjects(
                    database.objectsData[selectedObjectIndex].Prefab, 
                    gridPosition, 
                    startPosition, 
                    objectSize,
                    database.objectsData[selectedObjectIndex].isConjunto,
                    cellSize);


                if (database.objectsData[selectedObjectIndex].isConjunto)
                    objectSize = new Vector2Int(
                        Mathf.Abs(gridPosition.x - startPosition.x) + 1,
                        Mathf.Abs(gridPosition.z - startPosition.z) + 1);

                // Determinar las posiciones donde se colocaron los objetos
                temporaryPositions = CalculateGridPositions(gridPosition, startPosition, objectSize);



                if (temporaryPositions.Count != indexes.Count)
                {
                    Debug.LogError("Las posiciones y los índices no coinciden. Revisa la lógica de placeMultipleObjects.");
                    return;
                }

                // Actualizar el GridData y realizar acciones por cada objeto
                GridData selectedData = furnitureData;
                for (int i = 0; i < indexes.Count; i++)
                {
                    Vector3Int currentPosition = temporaryPositions[i];
                    int currentIndex = indexes[i];

                    selectedData.AddObjectAt(currentPosition,
                        objectSize,
                        database.objectsData[selectedObjectIndex].ID,
                        currentIndex, database.objectsData[selectedObjectIndex].Name
                    );

                    // Opcional: actualizar vista previa por cada objeto
                    previewSystem.UpdatePosition(grid.CellToWorld(currentPosition), false);
                }

                Debug.Log($"Se colocaron {indexes.Count} objetos correctamente.");

                isPlacementValid = false;
                temporaryPositions = new List<Vector3Int>();

            }
        }

    }


    private bool CheckMultiplePlacementValidity(Vector3Int gridPosition, int selectedObjectIndex, Vector2Int gridSize, Vector3Int startPosition)
    {

        //GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
        GridData selectedData = furnitureData;
        bool validity = selectedData.CanPlaceMultipleObjects(gridPosition, database.objectsData[selectedObjectIndex].Size, gridSize, startPosition);
        //Debug.Log(validity);
        return validity;
        throw new NotImplementedException();
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex, Vector2Int gridSize)
    {

        //GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
        GridData selectedData = furnitureData;
        return selectedData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, gridSize);
        throw new NotImplementedException();
    }

    public void UpdateState(Vector3Int gridPosition, Vector2Int gridSize, string actionSource, Vector3Int startPosition)
    {


        if (actionSource == "PlaceStructure")
        {
            bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex, gridSize);
            previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
        }
        else
        {
            if (actionSource == "RightClickHold")
            {
                bool placementValidity = CheckMultiplePlacementValidity(gridPosition, selectedObjectIndex, gridSize, startPosition);
                previewSystem.UpdateMultiplePosition(gridPosition, gridSize, startPosition, placementValidity);
            }
            else if (actionSource == "RightClickReleased")
            {
                previewSystem.PrepareCursor(new Vector2Int(database.objectsData[selectedObjectIndex].Size.x, database.objectsData[selectedObjectIndex].Size.y));
                bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex, gridSize);
                previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
                
            }
        }


    }

}
