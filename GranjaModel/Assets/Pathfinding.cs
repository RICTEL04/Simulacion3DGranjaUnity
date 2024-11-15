using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, HarvestModel model)
    {
        HashSet<Vector2Int> openSet = new HashSet<Vector2Int> { start };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, target) };

        while (openSet.Count > 0)
        {
            Vector2Int current = GetLowestFScoreNode(openSet, fScore);

            if (current == target)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (Vector2Int neighbor in GetNeighbors(current, model.fieldSize))
            {
                if (!IsWalkable(neighbor, model)) continue;

                float tentativeGScore = gScore[current] + Vector2Int.Distance(current, neighbor);

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        
        // Retorna una lista vacía si no hay un camino disponible
        return new List<Vector2Int>();
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }

    private static Vector2Int GetLowestFScoreNode(HashSet<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int lowestNode = default;
        float lowestScore = float.MaxValue;

        foreach (var node in openSet)
        {
            if (fScore.ContainsKey(node) && fScore[node] < lowestScore)
            {
                lowestScore = fScore[node];
                lowestNode = node;
            }
        }
        return lowestNode;
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int node, int fieldSize)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (var direction in directions)
        {
            Vector2Int neighbor = node + direction;
            if (neighbor.x >= 0 && neighbor.x < fieldSize && neighbor.y >= 0 && neighbor.y < fieldSize)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    private static bool IsWalkable(Vector2Int position, HarvestModel model)
    {
        // Aquí se puede incluir lógica para verificar que no haya tractores u otros obstáculos.
        GameObject parcel = model.GetParcelAtPosition(position);
        return parcel != null && parcel.GetComponent<Parcel>().GetState() != ParcelState.Empty;
    }

    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}
