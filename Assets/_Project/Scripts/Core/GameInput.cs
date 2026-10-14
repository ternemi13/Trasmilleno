using UnityEngine;
using UnityEngine.InputSystem;
namespace Transmilleno
{
    public sealed class GameInput : System.IDisposable
    {
        public readonly InputActionMap Map = new InputActionMap("Juego");
        public InputAction Move, Look, Zoom, Jump, Sprint, Punch, Knife, Interact, Pause, Crouch;
        public GameInput()
        {
            Move = Map.AddAction("Mover", InputActionType.Value);
            Move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddBinding("<Gamepad>/leftStick");
            Look = Map.AddAction("Camara", InputActionType.Value);
            Look.AddBinding("<Mouse>/delta");
            Look.AddBinding("<Gamepad>/rightStick");
            Zoom = Map.AddAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");
            Jump = Add("Saltar", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            Sprint = Add("Correr", "<Keyboard>/leftShift", "<Gamepad>/leftStickPress");
            Punch = Add("Golpear", "<Mouse>/leftButton", "<Gamepad>/buttonWest");
            Knife = Add("Navaja", "<Mouse>/rightButton", "<Gamepad>/rightTrigger");
            Interact = Add("Interactuar", "<Keyboard>/e", "<Gamepad>/buttonNorth");
            Pause = Add("Pausa", "<Keyboard>/escape", "<Gamepad>/start");
            Crouch = Add("Agacharse", "<Keyboard>/c", "<Gamepad>/buttonEast");
            if (!string.IsNullOrEmpty(Session.Save?.bindings))
            {
                try
                {
                    Map.LoadBindingOverridesFromJson(Session.Save.bindings);
                }
                catch { }
            }
            Map.Enable();
        }
        InputAction Add(string name, string keyboard, string gamepad)
        {
            var a = Map.AddAction(name, InputActionType.Button, keyboard);
            a.AddBinding(gamepad);
            return a;
        }
        public void Dispose()
        {
            Map.Disable();
            foreach (var a in Map)
                a.Dispose();
        }
    }
}
