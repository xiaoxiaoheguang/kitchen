using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteraction;
    public event EventHandler OnInteractionAlternate;
    public event EventHandler OnPauseAction;

    public enum Binding
    {
        Move_UP,
        Move_DOWN,
        Move_LEFT,
        Move_RIGHT,
        Interact,
        InteractAlt,
        Pause,
    }

    private PlayerInputActions playerInput;
    private void Awake()
    {
        Instance = this;

        playerInput = new PlayerInputActions();
        playerInput.player.Enable();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            playerInput.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }


        playerInput.player.Interact.performed += Interact_performed;
        playerInput.player.InteractAlternate.performed += InteractAlternate_performed;
        playerInput.player.Pause.performed += Pause_performed;
    }
    private void OnDestroy()
    {
        playerInput.player.Interact.performed -= Interact_performed;
        playerInput.player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInput.player.Pause.performed -= Pause_performed;

        playerInput.Dispose();
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractionAlternate?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteraction?.Invoke(this, EventArgs.Empty);
    }

    //获取位移量
    public Vector2 GetMovement_Normalized_Vector2()
    {
        Vector2 move = playerInput.player.move.ReadValue<Vector2>();

        move = move.normalized;

        return move;
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Move_UP:
                return playerInput.player.move.bindings[1].ToDisplayString();

            case Binding.Move_DOWN:
                return playerInput.player.move.bindings[2].ToDisplayString();

            case Binding.Move_LEFT:
                return playerInput.player.move.bindings[3].ToDisplayString();

            case Binding.Move_RIGHT:
                return playerInput.player.move.bindings[4].ToDisplayString();

            case Binding.Interact:
                return playerInput.player.Interact.bindings[0].ToDisplayString();

            case Binding.InteractAlt:
                return playerInput.player.InteractAlternate.bindings[0].ToDisplayString();

            case Binding.Pause:
                return playerInput.player.Pause.bindings[0].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action action)
    {
        playerInput.player.Disable();

        InputAction inputAction = null;
        int index = 0;

        switch (binding)
        {
            case Binding.Move_UP:
                inputAction = playerInput.player.move;
                index = 1;
                break;
            case Binding.Move_DOWN:
                inputAction = playerInput.player.move;
                index = 2;
                break;
            case Binding.Move_LEFT:
                inputAction = playerInput.player.move;
                index = 3;
                break;
            case Binding.Move_RIGHT:
                inputAction = playerInput.player.move;
                index = 4;
                break;
            case Binding.Interact:
                inputAction = playerInput.player.Interact;
                index = 0;
                break;
            case Binding.InteractAlt:
                inputAction = playerInput.player.InteractAlternate;
                index = 0;
                break;
            case Binding.Pause:
                inputAction = playerInput.player.Pause;
                index = 0;
                break;
            default:
                break;
        }

        inputAction.PerformInteractiveRebinding(index)
            .OnComplete(callback =>
            {
                Debug.Log(callback.action.bindings[index].path);
                Debug.Log(callback.action.bindings[index].overridePath);
                callback.Dispose();
                playerInput.player.Enable();
                action();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInput.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            }).Start();
    }
}
