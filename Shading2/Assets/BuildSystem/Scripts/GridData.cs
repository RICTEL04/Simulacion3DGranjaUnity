using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class GridData
{
    public Dictionary<Vector3Int, PlacementData> placedObjects = new();
    public Dictionary<int, PlacementData> indexToPlacementData = new();
    
    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex, string objectName)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex, objectName);

        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                throw new Exception($"Dictionary already contains cell position {pos}");
            placedObjects[pos] = data;

        }

        if (!indexToPlacementData.ContainsKey(placedObjectIndex))
        {
            indexToPlacementData[placedObjectIndex] = data;
        }

    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();

        

        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }

        return returnVal;

    }

    private bool ArePositionsInRangeFree(Vector3Int gridPosition, Vector3Int startPosition)
    {
        // Determinar los límites de las iteraciones
        int minX = Mathf.Min(startPosition.x, gridPosition.x);
        int maxX = Mathf.Max(startPosition.x, gridPosition.x);
        int minZ = Mathf.Min(startPosition.z, gridPosition.z);
        int maxZ = Mathf.Max(startPosition.z, gridPosition.z);

        // Iterar sobre todas las posiciones dentro del rango
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector3Int currentPosition = new Vector3Int(x, 0, z);

                // Verificar si la posición está ocupada
                if (placedObjects.ContainsKey(currentPosition))
                {
                    Debug.Log($"La posición {currentPosition} ya está ocupada.");
                    return false; // Retornar false si alguna posición está ocupada
                }
            }
        }

        // Todas las posiciones están libres
        return true;
    }


    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize, Vector2Int gridSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);

        int minLimitX = -gridSize.x / 2;

        Vector2Int limitX = new Vector2Int(-gridSize.x / 2, (int)((gridSize.x*1f / 2) - 0.5f));
        Vector2Int limitY = new Vector2Int(-gridSize.y / 2, (int)((gridSize.y * 1f / 2) - 0.5f));

        foreach (var pos in positionToOccupy)
        {
            //Debug.Log(pos);

            if (pos.x < limitX.x || pos.z < limitY.x || pos.x > limitX.y || pos.z > limitY.y)
            {
                return false; // Fuera de los límites.
            }

            if (placedObjects.ContainsKey(pos)) return false;
        }

        return true;

    }


    public bool CanPlaceMultipleObjects(Vector3Int gridPosition, Vector2Int objectSize, Vector2Int gridSize, Vector3Int startPosition)
    {
        // Limites del grid
        Vector2Int limitX = new Vector2Int(-gridSize.x / 2, (int)((gridSize.x * 1f / 2) - 0.5f));
        Vector2Int limitY = new Vector2Int(-gridSize.y / 2, (int)((gridSize.y * 1f / 2) - 0.5f));

        bool arePositionsFree = false;

        // Asegurar que las posiciones iniciales y finales están dentro del grid
        if (gridPosition.x < limitX.x || gridPosition.z < limitY.x ||
            gridPosition.x > limitX.y || gridPosition.z > limitY.y ||
            startPosition.x < limitX.x || startPosition.z < limitY.x ||
            startPosition.x > limitX.y || startPosition.z > limitY.y)
        {
            Debug.Log("La posición está fuera de los límites del grid.");
            return false;
        }



        // Determinar las dimensiones de la cuadrícula para colocar objetos
        int width = Mathf.Abs(gridPosition.x - startPosition.x) + 1; // +1 para incluir la celda inicial
        int length = Mathf.Abs(gridPosition.z - startPosition.z) + 1;


        // Si no cabe al menos un objeto en alguna dimensión, retornar false
        if (width % objectSize.x > 0 || length % objectSize.y > 0)
        {
            Debug.Log("No cabe ningún objeto de forma exacta en la dimension.");
            return false;
        }

        // Calcular cuántos objetos caben en cada dimensión
        int objectsInX = width / objectSize.x;
        int objectsInZ = length / objectSize.y;

        arePositionsFree = ArePositionsInRangeFree(gridPosition, startPosition);
        // Si todas las posiciones están libres, se pueden colocar los objetos
        if (arePositionsFree)
        {
            Debug.Log($"Desde {startPosition} hasta {gridPosition} caben {objectsInX * objectsInZ} objetos");
        }

        //Debug.Log($" infIzq ({minX}, {minZ})-> maxDer ({maxX}, {maxZ})" );
        //Debug.Log($"Lista de objetos {positionsToObjects}");
        return  arePositionsFree;
    }


    internal void RemoveObjectAt(Vector3Int gridPosition)
    {

        if (indexToPlacementData.ContainsKey(placedObjects[gridPosition].PlacedObjectsIndex))
            indexToPlacementData.Remove(placedObjects[gridPosition].PlacedObjectsIndex);

        foreach (var pos in placedObjects[gridPosition].occupiedPositions )
        {
            placedObjects.Remove(pos);
        }
    }

    internal int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
            return -1;
        return placedObjects[gridPosition].PlacedObjectsIndex;
    }


}

public class PlacementData
{

    public List<Vector3Int> occupiedPositions;
    public int ID { get; private set; }

    public int PlacedObjectsIndex { get; private set; }

    public string objectName { get; private set; }

    public PlacementData(List<Vector3Int> occuppiedPositions, int iD, int placedObjectsIndex, string objectName)
    {
        this.occupiedPositions = occuppiedPositions;
        ID = iD;
        PlacedObjectsIndex = placedObjectsIndex;
        this.objectName = objectName;
    }

}