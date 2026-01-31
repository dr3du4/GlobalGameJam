using UnityEngine;

/// <summary>
/// Helper do uzyskania Input Actions niezależnie od trybu gry (single/multiplayer).
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Pobierz Input Actions dla podanego gracza.
    /// Działa zarówno w single-player jak i multiplayer.
    /// </summary>
    public static InputSystem_Actions GetInputActionsForPlayer(GameObject player)
    {
        // Tryb multiplayer - sprawdź NetworkPlayerController
        var networkController = player.GetComponent<NetworkPlayerController>();
        if (networkController != null)
        {
            var actions = networkController.GetInputActions();
            if (actions != null)
            {
                return actions;
            }
        }

        // Tryb single-player - użyj GameManager
        if (GameManager.instance != null && GameManager.instance.inputActions != null)
        {
            return GameManager.instance.inputActions;
        }

        return null;
    }

    /// <summary>
    /// Sprawdź czy gracz może się poruszać (czy to lokalny gracz w multiplayer).
    /// </summary>
    public static bool CanPlayerMove(GameObject player)
    {
        // Tryb multiplayer - sprawdź czy to właściciel
        var networkObject = player.GetComponent<Unity.Netcode.NetworkObject>();
        if (networkObject != null)
        {
            return networkObject.IsOwner;
        }

        // Tryb single-player - zawsze może się poruszać
        return true;
    }
}
