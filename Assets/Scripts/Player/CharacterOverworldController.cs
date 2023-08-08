using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterOverworldController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float enduranceDepletionRate = 1f;
    public float tilePixelSize = 16f; // Adjust if needed

    private PlayerBaseInput playerBaseInputs;
    private Rigidbody2D rb;
    private float currentEndurance;

    private void Awake()
    {
        playerBaseInputs = new PlayerBaseInput();
        playerBaseInputs.Character.Disable();
        playerBaseInputs.Overworld.Enable();
        rb = GetComponent<Rigidbody2D>();
        //currentEndurance = 100f; // Initialize the endurance meter

        // Assign input handlers
        ///overworldInputMap["Move"].performed += Move;
        //overworldInputMap["Interact"].started += OnInteractInput;
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>().normalized;
        Debug.Log("Triggered1");
        if (movementInput != Vector2.zero)
        {
            Vector2 movement = CalculateMovement(movementInput);
            rb.MovePosition(rb.position + movement);
            Debug.Log("Triggered2");
            // Deplete endurance
            //currentEndurance -= CalculateEnduranceDepletion(movementInput);
        }
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        // Implement scene loading logic here based on player's position and input
        // Example: Use Physics.Raycast to detect nearby levels and initiate loading
    }

    private Vector2 CalculateMovement(Vector2 input)
    {
        float diagonalFactor = Mathf.Abs(input.x) == 1 && Mathf.Abs(input.y) == 1 ? 0.7071f : 1f; // For diagonal movement
        return input * movementSpeed * diagonalFactor * Time.deltaTime;
    }

    private float CalculateEnduranceDepletion(Vector2 input)
    {
        float diagonalFactor = Mathf.Abs(input.x) == 1 && Mathf.Abs(input.y) == 1 ? 1f : 2f; // Diagonal movements deplete 1, others deplete 2
        return enduranceDepletionRate * diagonalFactor;
    }
}