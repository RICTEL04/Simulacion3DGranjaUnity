using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField]
    private float previewYOffset = 0.06f;

    [SerializeField]
    private GameObject cellIndicator;
    private GameObject previewObject;


    [SerializeField]
    private Material previewMaterialPrefab;
    private Material previewMaterialInstance;

    private Renderer cellIndicatorRenderer;

    private Vector3 cellSize = Vector3.one; // Tamaño por defecto (1, 1, 1)


    private void Start()
    {
        previewMaterialInstance = new Material(previewMaterialPrefab);
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }

    internal void StartShowingRemovePreview()
    {
        
        cellIndicator.SetActive(true);
        PrepareCursor(Vector2Int.one);
        ApplyFeedbackToCursor(false);
    }

    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        previewObject = Instantiate(prefab);
        PreparePreavie(previewObject);
        PrepareCursor(size);
        cellIndicator.SetActive(true);
    }

    public void PrepareCursor(Vector2Int size)
    {
        // Usamos la variable global 'cellSize' para asegurarnos de que el tamaño esté sincronizado.
        if (size.x > 0 || size.y > 0)
        {
            // Usamos 'cellSize' para ajustar la escala, no 'size' directamente.
            cellIndicator.transform.localScale = new Vector3(cellSize.x * size.x, 1, cellSize.z * size.y);
            cellIndicatorRenderer.material.mainTextureScale = new Vector2(size.x, size.y);
        }
    }


    private void PreparePreavie(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for(int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterialInstance;
            }

            renderer.materials = materials;

        }
    }

    public void StopShowingPreview()
    {
        cellIndicator.SetActive(false);
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        

    }

    public void UpdatePosition(Vector3 position, bool validity)
    {
        if (previewObject != null)
        {
            MovePreview(position);
            ApplyFeedbackToPreview(validity);
        }

        MoveCursor(position);
        ApplyFeedbackToCursor(validity);
    }

    public void UpdateMultiplePosition(Vector3Int gridPosition, Vector2Int gridSize, Vector3Int startPosition, bool validity)
    {


        // Mueve y ajusta la vista previa
        if (previewObject != null)
        {
            MoveMultiplePreview(gridPosition, startPosition);
            ApplyFeedbackToPreview(validity);
        }

        // Mueve y ajusta el indicador de celda
        MoveMultiplePreview(gridPosition, startPosition);
        ApplyFeedbackToCursor(validity);

    }

    public void MoveMultiplePreview(Vector3Int gridPosition, Vector3Int startPosition)
    {
        // Convertir las posiciones del grid al espacio del mundo
        Vector3 worldStart = new Vector3(
            startPosition.x * cellSize.x,
            startPosition.y * cellSize.y,
            startPosition.z * cellSize.z
        );

        Vector3 worldGrid = new Vector3(
            gridPosition.x * cellSize.x,
            gridPosition.y * cellSize.y,
            gridPosition.z * cellSize.z
        );

        // Calcular las coordenadas mínimas (origen del cuadro)
        float minX = Mathf.Min(worldStart.x, worldGrid.x);
        float minZ = Mathf.Min(worldStart.z, worldGrid.z);

        // Determinar las dimensiones del área seleccionada
        int width = Mathf.Abs(gridPosition.x - startPosition.x) + 1; // +1 para incluir la celda inicial
        int length = Mathf.Abs(gridPosition.z - startPosition.z) + 1;

        // Posicionar el indicador en el origen del área seleccionada
        Vector3 originPosition = new Vector3(
            minX, // Ajuste para centrar
            worldStart.y, // Mantener la misma altura en el mundo
            minZ   // Ajuste para centrar
        );


        

        cellIndicator.transform.position = originPosition;

        // Ajustar la escala del indicador
        cellIndicator.transform.localScale = new Vector3(cellSize.x * width, 1, cellSize.z * length);
        cellIndicatorRenderer.material.mainTextureScale = new Vector2(width, length);
    }




    private void ApplyFeedbackToPreview(bool validity)
    {
        Color c = validity ? Color.white : Color.red;
        c.a = 0.5f;
        previewMaterialInstance.color = c;
    }


    private void ApplyFeedbackToCursor(bool validity)
    {
        Color c = validity ? Color.white : Color.red;
        c.a = 0.5f;
        cellIndicatorRenderer.material.color = c;

    }

    private void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        previewObject.transform.position = new Vector3(
            position.x, 
            position.y + previewYOffset, 
            position.z);
    }

    public void UpdateCursorScale(Vector3 newCellSize)
    {
        if (cellIndicator != null)
        {
            cellSize = newCellSize; // Actualiza el tamaño global.
            Debug.Log("Se actualizó la escala del CursorIndicator a: " + cellSize);
            cellIndicator.transform.localScale = new Vector3(cellSize.x, 1, cellSize.z);
        }
    }



}
