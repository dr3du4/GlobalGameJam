using UnityEngine;
using UnityEngine.InputSystem;

public class GameManger : MonoBehaviour
{
    public static GameManger instance;

    public bool useFirstMap = true;
    private bool wasSpacePressed = false;

    void Awake()
    {
        instance = this;
        useFirstMap = true;
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            bool isSpacePressed = Keyboard.current.spaceKey.isPressed;
           
            if (isSpacePressed && !wasSpacePressed)
            {
                Debug.Log("Use first map: " + useFirstMap);
                useFirstMap = !useFirstMap;
            }
            
            wasSpacePressed = isSpacePressed;
        }
    }
}
