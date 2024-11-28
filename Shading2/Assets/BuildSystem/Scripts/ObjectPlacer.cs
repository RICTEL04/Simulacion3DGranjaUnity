using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> placedGameObjects = new();

    public List<Tuple<Vector3Int, Vector3Int>> placedGameObjectsPositions = new();

    public int PlaceObject(GameObject prefab, Vector3Int position, Vector3Int lastPosition, Grid grid)
    {
        //Debug.Log("PlaceObject called with prefab: " + prefab.name);

        if (prefab == null)
        {
            Debug.LogError("Prefab is null! Ensure it is set correctly in the database.");
            return -1; // Indicar un error si el prefab es nulo.
        }
        
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = grid.CellToWorld(position);
        placedGameObjects.Add(newObject);
        placedGameObjectsPositions.Add(new Tuple<Vector3Int, Vector3Int>(position, lastPosition));
        Debug.Log($"Index: {placedGameObjects.Count - 1}, posicion: ({position}, {lastPosition}");

        return placedGameObjects.Count - 1;

    }


    public List<int> placeMultipleObjects(GameObject prefab, Vector3Int gridPosition, Vector3Int startPosition, Vector2Int objectSize, bool isConjunto, Vector2Int cellSize)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null! Ensure it is set correctly in the database.");
            return null; // Devolver null para indicar un error si el prefab es nulo.
        }

        // Determinar las dimensiones de la cuadrícula para colocar objetos
        int width = Mathf.Abs(gridPosition.x - startPosition.x) + 1; // +1 para incluir la celda inicial
        int length = Mathf.Abs(gridPosition.z - startPosition.z) + 1;

        // Calcular las posiciones de los objetos
        int minX = Mathf.Min(startPosition.x, gridPosition.x);
        int maxX = Mathf.Max(startPosition.x, gridPosition.x);
        int minZ = Mathf.Min(startPosition.z, gridPosition.z);
        int maxZ = Mathf.Max(startPosition.z, gridPosition.z);
        List<Vector3Int> positionsToObjects = new List<Vector3Int>();
        List<int> objectIndices = new List<int>();

        for (int x = minX; x <= maxX; x += objectSize.x)
        {
            for (int z = minZ; z <= maxZ; z += objectSize.y)
            {
                positionsToObjects.Add(new Vector3Int(x, 0, z));
            }
        }

        if (isConjunto)
        {
            // Crear un contenedor vacío para agrupar los objetos
            GameObject conjuntoParent = new GameObject($"Conjunto{placedGameObjects.Count}");
            conjuntoParent.transform.position = Vector3.zero;

            // Instanciar objetos en las posiciones calculadas y agruparlos como hijos
            foreach (var position in positionsToObjects)
            {
                Vector3 fixedPosition = new Vector3
                (
                    position.x * cellSize.x,
                    0,
                    position.z * cellSize.y
                );
                GameObject newObject = Instantiate(prefab);
                newObject.transform.position = fixedPosition;
                newObject.transform.parent = conjuntoParent.transform; // Hacerlo hijo del contenedor
            }

            //GameObject newObject2 = Instantiate(prefab);
            // Guardar el contenedor como un único índice
            placedGameObjects.Add(conjuntoParent);
            placedGameObjectsPositions.Add(new Tuple<Vector3Int, Vector3Int>(startPosition, gridPosition));
            Debug.Log($"Index: {placedGameObjects.Count - 1}, posicion: ({startPosition}, {gridPosition}");
            //newObject2.SetActive(false);
            objectIndices.Add(placedGameObjects.Count - 1); // El índice del "super objeto"
        }

        else
        {
            // Instanciar objetos en las posiciones calculadas
            foreach (var position in positionsToObjects)
            {
                Vector3 fixedPosition = new Vector3
                (
                    position.x * cellSize.x,
                    0,
                    position.z * cellSize.y
                );
                GameObject newObject = Instantiate(prefab);
                newObject.transform.position = fixedPosition;
                placedGameObjects.Add(newObject);
                placedGameObjectsPositions.Add(new Tuple<Vector3Int, Vector3Int>(position, position + new Vector3Int(objectSize.x - 1, 0, objectSize.y - 1)));
                Debug.Log($"Index: {placedGameObjects.Count - 1}, posicion: ({position}, {position + new Vector3Int(objectSize.x - 1, 0, objectSize.y - 1)}");

                // Guardar el índice del objeto colocado
                objectIndices.Add(placedGameObjects.Count - 1);
            }
        }





        return objectIndices; // Devolver los índices de los objetos colocados
    }


    internal void RemoveObjectAt(int gameObjectIndex)
    {
        if (placedGameObjects.Count <= gameObjectIndex ||
            placedGameObjects[gameObjectIndex] == null) return;
        Destroy(placedGameObjects[gameObjectIndex]);
        placedGameObjects[gameObjectIndex] = null;
        placedGameObjectsPositions[gameObjectIndex] = null;
    }
}
