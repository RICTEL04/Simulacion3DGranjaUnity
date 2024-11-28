using UnityEngine;

public interface IBuildingState
{
    void EndState();
    void OnAction(Vector3Int gridPosition, Vector2Int gridSize, string actionSource, Vector3Int startPosition);
    void UpdateState(Vector3Int gridPosition, Vector2Int gridSize, string actionSource, Vector3Int startPosition);
}
