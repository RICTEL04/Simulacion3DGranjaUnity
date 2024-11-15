using UnityEngine;
using System.Collections.Generic;

public class TractorAgent : MonoBehaviour
{
    public int capacity = 40;
    public int maxFuel = 470;
    public int fuelConsumptionRate = 1;
    public float maxSpeed = 5f;
    public float acceleration = 0.1f;
    public float rotationSpeed = 2f;

    private int load = 0;
    private int fuelLevel;
    private Vector2Int target;
    private HarvestModel model;
    private Vector2Int unloadPoint;
    private bool isRefueling = false;
    private bool isMoving = false;
    private float currentSpeed = 0f;
    private Quaternion targetRotation;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex = 0;

    public TextMesh loadDisplay;

    public void Initialize(HarvestModel model, Vector2Int refuelStation, Vector2Int unloadPoint, float tractorSpeed)
    {
        maxSpeed = tractorSpeed;
        this.model = model;
        this.unloadPoint = unloadPoint;
        fuelLevel = maxFuel;
        MoveToNextTarget();
        UpdateLoadDisplay();
    }

    void Update()
    {
        if (model == null) return;

        if (fuelLevel > 0)
        {
            if (currentPath.Count == 0)
            {
                MoveToNextTarget(); // Recalcular la ruta cuando no hay camino.
            }

            if (isMoving)
            {
                MoveAlongPath();
            }

            HarvestNearbyParcels(); // Continuar cosechando
            fuelLevel -= fuelConsumptionRate;
        }
        else if (!isRefueling)
        {
            target = model.GetRefuelStationPosition();
            isRefueling = true;
        }
    }

    void MoveToNextTarget()
{
    if (fuelLevel < maxFuel / 4 && !isRefueling)
    {
        target = model.GetRefuelStationPosition();
        isRefueling = true;
    }
    else if (load >= capacity)
    {
        target = model.GetUnloadPointPosition();
    }
    else
    {
        List<Vector2Int> parcels = model.GetParcelsReady();
        if (parcels.Count > 0)
        {
            target = parcels[Random.Range(0, parcels.Count)];
            isRefueling = false;
        }
    }

    // Obtener la ruta hacia el siguiente objetivo utilizando el Pathfinding.
    currentPath = Pathfinding.FindPath(new Vector2Int((int)transform.position.x, (int)transform.position.z), target, model);
    pathIndex = 0;

    if (currentPath.Count > 0)
    {
        isMoving = true;
        SetTargetRotation(currentPath[pathIndex]);
    }
}


    void MoveAlongPath()
    {
        if (pathIndex >= currentPath.Count)
        {
            isMoving = false; // Si hemos llegado al final de la ruta, dejar de movernos.
            return;
        }

        Vector2Int nextPosition = currentPath[pathIndex];
        Vector3 targetPosition = new Vector3(nextPosition.x, 0, nextPosition.y);

        // Ajustar la rotación hacia el siguiente punto de la ruta.
        SetTargetRotation(nextPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            pathIndex++; // Avanzar al siguiente punto en la ruta.
        }

        // Moverse hacia el siguiente punto de la ruta.
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    void SetTargetRotation(Vector2Int targetPosition)
    {
        Vector3 targetVec = new Vector3(targetPosition.x, 0, targetPosition.y);
        Vector3 direction = (targetVec - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void HarvestNearbyParcels()
{
    // Solo cosechar si no está lleno
    if (load >= capacity)
    {
        target = model.GetUnloadPointPosition();
        return;
    }

    // Detectar parcelas cercanas
    Collider[] parcelsInRange = Physics.OverlapSphere(transform.position + transform.forward, 1.0f);
    foreach (var collider in parcelsInRange)
    {
        if (collider.CompareTag("Parcel"))
        {
            Parcel parcel = collider.GetComponent<Parcel>();
            if (parcel != null && parcel.GetState() == ParcelState.ReadyToHarvest)
            {
                // Cosechar la parcela
                parcel.SetState(ParcelState.Harvested);
                load += 10; // Asumimos que cada cosecha añade 10 unidades al tractor
                UpdateLoadDisplay();

                // Eliminar la parcela de la lista de parcelas listas para cosechar
                model.GetParcelsReady().Remove(new Vector2Int((int)collider.transform.position.x, (int)collider.transform.position.z));

                // Si el tractor está lleno, dirigirlo al punto de descarga
                if (load >= capacity)
                {
                    target = model.GetUnloadPointPosition();
                    return;
                }
            }
        }
    }
}


    void UpdateLoadDisplay()
    {
        if (loadDisplay != null)
        {
            loadDisplay.text = "Load: " + load;
        }
    }
}
