using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    private Vector3 lastPosition;

    [SerializeField]
    private LayerMask placementLayermask;

    public event Action OnClicked; // Evento para clic izquierdo.
    public event Action OnExit; // Evento para tecla Escape.

    // Eventos para el sistema de clic derecho.
    public event Action OnRightClickHoldStart; // Inicio del clic derecho.
    public event Action OnRightClickHold; // Mantener clic derecho.
    public event Action OnRightClickReleased; // Soltar clic derecho.

    private bool isRightClickHeld = false; // Indica si el botón derecho está siendo mantenido.
    private bool isLeftClickHeld = false; // Indica si el botón izquierdo está activo.

    [SerializeField]
    private float rightClickHoldThreshold = 0.5f; // Tiempo mínimo para considerar que el botón derecho está siendo mantenido.

    private float rightClickHoldTime = 0f; // Tiempo acumulado de mantener clic derecho.


    private void Update()
    {
        // Manejo de la tecla Escape (siempre tiene prioridad sobre clics).
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExit?.Invoke();
            return;
        }

        // Manejo del clic izquierdo: Solo si el derecho no está activo.
        if (Input.GetMouseButtonDown(0) && !isRightClickHeld)
        {
            isLeftClickHeld = true;
            OnClicked?.Invoke();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isLeftClickHeld = false;
        }

        // Manejo del clic derecho: Solo si el izquierdo no está activo.
        if (Input.GetMouseButtonDown(1) && !isLeftClickHeld)
        {
            isRightClickHeld = true;
            rightClickHoldTime = 0f;

            // Evento de inicio del clic derecho.
            OnRightClickHoldStart?.Invoke();
        }

        // Mantener clic derecho.
        if (Input.GetMouseButton(1) && isRightClickHeld)
        {
            rightClickHoldTime += Time.deltaTime;

            // Invoca el evento de mantener clic derecho.
            OnRightClickHold?.Invoke();
        }

        // Soltar clic derecho.
        if (Input.GetMouseButtonUp(1) && isRightClickHeld)
        {
            isRightClickHeld = false;
            rightClickHoldTime = 0f;

            // Evento al finalizar el clic derecho.
            OnRightClickReleased?.Invoke();
        }
    }


    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200, placementLayermask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
}