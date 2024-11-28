using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlacementData", menuName = "ScriptableObjects/PlacementData")]
public class PlacementDataSO : ScriptableObject
{
    public GridData furnitureData;
    public List<Tuple<Vector3Int, Vector3Int>> placedGameObjectsPositions = new();
    public Vector2Int gridSize;
    public Vector3 cellSize;
    public ObjectsDatabaseSO database;
}