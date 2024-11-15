using UnityEngine;

public enum ParcelState { Empty, ReadyToHarvest, Harvested, RefuelStation, UnloadPoint }

public class Parcel : MonoBehaviour
{
    private ParcelState state;

    public void SetState(ParcelState newState)
    {
        state = newState;
        UpdateAppearance();
    }

    public ParcelState GetState()
    {
        return state;
    }

    void UpdateAppearance()
    {
        // Cambia el color o textura de acuerdo al estado de la parcela
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (state)
            {
                case ParcelState.ReadyToHarvest:
                    renderer.material.color = Color.green;
                    break;
                case ParcelState.Harvested:
                    renderer.material.color = Color.white;
                    break;
                case ParcelState.Empty:
                    renderer.material.color = Color.gray;
                    break;
                default:
                    renderer.material.color = Color.white;
                    break;
            }
        }
    }
}
