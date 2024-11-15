using UnityEngine;
using System.Collections.Generic;

public class HarvestModel : MonoBehaviour
{
    public int fieldSize = 20;
    public int numTractors = 3;
    public GameObject tractorPrefab;
    public GameObject parcelPrefab;
    public GameObject refuelStationPrefab;
    public GameObject unloadPointPrefab;

    private List<Vector2Int> parcelsReady = new List<Vector2Int>();
    private Vector2Int refuelStation;
    private Vector2Int unloadPoint;

    void Start()
    {
        InitializeField();
        PlaceRefuelStation();
        PlaceUnloadPoint();
        SpawnTractors();
    }

    void InitializeField()
    {
        for (int x = 0; x < fieldSize; x++)
        {
            for (int y = 0; y < fieldSize; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                GameObject parcel = Instantiate(parcelPrefab, new Vector3(x, 0, y), Quaternion.identity);
                if (Random.value < 0.9f)
                {
                    parcelsReady.Add(position); // Parcela lista para cosechar
                    parcel.GetComponent<Parcel>().SetState(ParcelState.ReadyToHarvest);
                }
                else
                {
                    parcel.GetComponent<Parcel>().SetState(ParcelState.Empty);
                }
            }
        }
    }

    void PlaceRefuelStation()
    {
        refuelStation = new Vector2Int(-1, -1); // Colocación fuera de la cuadrícula
        Instantiate(refuelStationPrefab, new Vector3(refuelStation.x, 0, refuelStation.y), Quaternion.identity);
    }

    void PlaceUnloadPoint()
    {
        unloadPoint = new Vector2Int(fieldSize, fieldSize); // Colocación fuera de la cuadrícula en el borde opuesto
        Instantiate(unloadPointPrefab, new Vector3(unloadPoint.x, 0, unloadPoint.y), Quaternion.identity);
    }

    public Vector2Int GetRefuelStationPosition()
    {
        return refuelStation;
    }

    public Vector2Int GetUnloadPointPosition()
    {
        return unloadPoint;
    }

    public List<Vector2Int> GetParcelsReady()
    {
        return parcelsReady;
    }
    public GameObject GetParcelAtPosition(Vector2Int position)
{
    Collider[] colliders = Physics.OverlapSphere(new Vector3(position.x, 0, position.y), 0.1f);
    foreach (var collider in colliders)
    {
        if (collider.gameObject.CompareTag("Parcel")) // Asegúrate de etiquetar las parcelas como "Parcel"
        {
            return collider.gameObject;
        }
    }
    return null;
}



    void SpawnTractors()
    {
        for (int i = 0; i < numTractors; i++)
        {
            Vector2Int position = new Vector2Int(Random.Range(0, fieldSize), Random.Range(0, fieldSize));
            GameObject tractor = Instantiate(tractorPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
            tractor.GetComponent<TractorAgent>().Initialize(this, refuelStation, unloadPoint, 5f);
        }
    }
}
