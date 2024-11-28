using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovingState : IBuildingState
{
    private int gameObjectIndex = -1;
    Grid grid;
    PreviewSystem previewSystem;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;



    public RemovingState(Grid grid,
        PreviewSystem previewSystem,
        GridData floorData,
        GridData furnitureData,
        ObjectPlacer objectPlacer)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;


        previewSystem.StartShowingRemovePreview();

    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
        
    }

    public void OnAction(Vector3Int gridPosition, Vector2Int gridSize, string actionSource, Vector3Int startPosition)
    {

        if (actionSource == "PlaceStructure")
        {
            GridData selectedData = null;
            if (furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one, gridSize) == false)
            {
                selectedData = furnitureData;
            }
            else if (floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one, gridSize) == false)
            {
                selectedData = floorData;
            }

            if (selectedData == null)
            {
                //sound
            }
            else
            {
                gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
                Debug.Log(gameObjectIndex);
                if (gameObjectIndex == -1) return;
                selectedData.RemoveObjectAt(gridPosition);
                objectPlacer.RemoveObjectAt(gameObjectIndex);
            }

            Vector3 cellPosition = grid.CellToWorld(gridPosition);
            previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition, gridSize));
        }

        else if (actionSource == "HandleRightClickHold")
        {

        }



    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition, Vector2Int gridSize)
    {
        return !(furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one, gridSize) &&
                 floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one, gridSize));
    }

    public void UpdateState(Vector3Int gridPosition, Vector2Int gridSize, string actionSource, Vector3Int startPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition, gridSize);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }
}
