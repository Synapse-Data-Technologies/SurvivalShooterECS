using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[DisableAutoCreation]
public partial class PlayerInputSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;
    private InputAction rightStickAction;
    private InputAction rightTriggerAction;

    private float2 moveInput;
    private float2 lookInput;
    private float shootInput;
    private float2 rightStickInput;
    private float rightTriggerInput;
    
    // Deadzone configuration from Unity Input System settings
    private const float InnerDeadzone = 0.125f;
    private const float OuterDeadzone = 0.925f;
    
    // Trigger threshold configuration
    private const float TriggerThreshold = 0.5f; // Minimum trigger value to register shooting
    
    // Input mode detection
    private bool isUsingGamepad = false;
    private float2 lastMousePosition;
    private const float MouseMovementThreshold = 1.0f; // Minimum mouse movement to detect mouse usage
    
    // Controller disconnection detection
    private bool wasGamepadConnected = false;
    private const float GamepadDisconnectCheckInterval = 1.0f; // Check every second
    private float timeSinceLastGamepadCheck = 0f;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        lastMousePosition = float2.zero;
    }

    protected override void OnStartRunning()
    {
        try
        {
            // Move action: WASD keys + left stick (fixed from right stick)
            moveAction = new InputAction("move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            moveAction.performed += context =>
            {
                moveInput = context.ReadValue<Vector2>();
            };
            moveAction.canceled += context =>
            {
                moveInput = context.ReadValue<Vector2>();
            };
            moveAction.Enable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Failed to configure move action: {e.Message}. Using default bindings.");
            // Fallback: create a minimal action with just keyboard support
            moveAction = new InputAction("move");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.performed += context => { moveInput = context.ReadValue<Vector2>(); };
            moveAction.canceled += context => { moveInput = context.ReadValue<Vector2>(); };
            moveAction.Enable();
        }

        try
        {
            // Look action: Mouse position (for mouse aiming)
            lookAction = new InputAction("look", binding: "<Mouse>/position");
            lookAction.performed += context =>
            {
                lookInput = context.ReadValue<Vector2>();
            };
            lookAction.canceled += context =>
            {
                lookInput = context.ReadValue<Vector2>();
            };
            lookAction.Enable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Failed to configure look action: {e.Message}. Mouse aiming may not work.");
            lookAction = new InputAction("look", binding: "<Mouse>/position");
            lookAction.performed += context => { lookInput = context.ReadValue<Vector2>(); };
            lookAction.canceled += context => { lookInput = context.ReadValue<Vector2>(); };
            lookAction.Enable();
        }

        try
        {
            // Right stick action: For gamepad aiming
            rightStickAction = new InputAction("rightStick", binding: "<Gamepad>/rightStick");
            rightStickAction.performed += context =>
            {
                rightStickInput = context.ReadValue<Vector2>();
            };
            rightStickAction.canceled += context =>
            {
                rightStickInput = context.ReadValue<Vector2>();
            };
            rightStickAction.Enable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Failed to configure right stick action: {e.Message}. Gamepad aiming may not work.");
            // Create a minimal fallback action
            rightStickAction = new InputAction("rightStick");
            rightStickAction.performed += context => { rightStickInput = float2.zero; };
            rightStickAction.canceled += context => { rightStickInput = float2.zero; };
            rightStickAction.Enable();
        }

        try
        {
            // Shoot action: Left mouse button (for mouse shooting)
            shootAction = new InputAction("shoot", binding: "<Mouse>/leftButton");
            shootAction.performed += context =>
            {
                shootInput = context.ReadValue<float>();
            };
            shootAction.canceled += context =>
            {
                shootInput = context.ReadValue<float>();
            };
            shootAction.Enable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Failed to configure shoot action: {e.Message}. Mouse shooting may not work.");
            shootAction = new InputAction("shoot", binding: "<Mouse>/leftButton");
            shootAction.performed += context => { shootInput = context.ReadValue<float>(); };
            shootAction.canceled += context => { shootInput = context.ReadValue<float>(); };
            shootAction.Enable();
        }

        try
        {
            // Right trigger action: For gamepad shooting
            rightTriggerAction = new InputAction("rightTrigger", binding: "<Gamepad>/rightTrigger");
            rightTriggerAction.performed += context =>
            {
                rightTriggerInput = context.ReadValue<float>();
            };
            rightTriggerAction.canceled += context =>
            {
                rightTriggerInput = context.ReadValue<float>();
            };
            rightTriggerAction.Enable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Failed to configure right trigger action: {e.Message}. Gamepad shooting may not work.");
            // Create a minimal fallback action
            rightTriggerAction = new InputAction("rightTrigger");
            rightTriggerAction.performed += context => { rightTriggerInput = 0f; };
            rightTriggerAction.canceled += context => { rightTriggerInput = 0f; };
            rightTriggerAction.Enable();
        }
    }

    protected override void OnStopRunning()
    {
        // Safely disable all input actions, handling null references
        try
        {
            if (rightTriggerAction != null)
                rightTriggerAction.Disable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Error disabling right trigger action: {e.Message}");
        }
        
        try
        {
            if (rightStickAction != null)
                rightStickAction.Disable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Error disabling right stick action: {e.Message}");
        }
        
        try
        {
            if (shootAction != null)
                shootAction.Disable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Error disabling shoot action: {e.Message}");
        }
        
        try
        {
            if (lookAction != null)
                lookAction.Disable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Error disabling look action: {e.Message}");
        }
        
        try
        {
            if (moveAction != null)
                moveAction.Disable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerInputSystem] Error disabling move action: {e.Message}");
        }
    }

    protected override void OnUpdate()
    {
        // Check for controller disconnection periodically
        timeSinceLastGamepadCheck += UnityEngine.Time.deltaTime;
        if (timeSinceLastGamepadCheck >= GamepadDisconnectCheckInterval)
        {
            CheckGamepadConnection();
            timeSinceLastGamepadCheck = 0f;
        }
        
        // Sanitize all inputs to handle NaN and infinite values from input devices
        var moveInputCopy = InputUtilities.SanitizeInput(moveInput);
        var lookInputCopy = InputUtilities.SanitizeInput(lookInput);
        var shootInputCopy = InputUtilities.SanitizeInput(shootInput);
        var rightStickInputCopy = InputUtilities.SanitizeInput(rightStickInput);
        var rightTriggerInputCopy = InputUtilities.SanitizeInput(rightTriggerInput);

        // Detect input mode based on most recent input device
        DetectInputMode();
        
        var isUsingGamepadCopy = isUsingGamepad;

        // Process gamepad movement input with deadzone filtering and normalization
        float2 processedMove = ProcessMovementInput(moveInputCopy);
        
        // Process gamepad aiming input with deadzone filtering and direction conversion
        float2 processedLook = ProcessAimingInput(lookInputCopy, rightStickInputCopy, isUsingGamepadCopy);
        
        // Process shooting input from both mouse and trigger
        float processedShoot = ProcessShootingInput(shootInputCopy, rightTriggerInputCopy);

        Entities
            .ForEach((Entity entity, ref PlayerInputData inputData) =>
        {
            // Update Move field with processed gamepad input
            inputData.Move = processedMove;
            
            // Update Look field with processed aiming input
            // For gamepad: direction vector
            // For mouse: screen-space position
            inputData.Look = processedLook;
            
            // Update Shoot field with processed shooting input
            // Combines mouse button and trigger input
            inputData.Shoot = processedShoot;
            
            // Set the input mode flag
            inputData.IsUsingGamepad = isUsingGamepadCopy;
        }).ScheduleParallel();

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
    
    /// <summary>
    /// Processes movement input by applying deadzone filtering and normalization.
    /// This ensures smooth analog control while preventing stick drift.
    /// </summary>
    /// <param name="rawInput">The raw movement input from keyboard or gamepad</param>
    /// <returns>Processed movement input ready for use by movement systems</returns>
    private float2 ProcessMovementInput(float2 rawInput)
    {
        // Apply deadzone filtering to left stick input
        float2 filteredInput = InputUtilities.ApplyDeadzone(rawInput, InnerDeadzone);
        
        // Normalize stick input after deadzone
        float2 normalizedInput = InputUtilities.NormalizeInput(filteredInput, InnerDeadzone, OuterDeadzone);
        
        return normalizedInput;
    }
    
    /// <summary>
    /// Processes aiming input based on the active input method.
    /// For gamepad: applies deadzone filtering and converts stick input to direction vector.
    /// For mouse: maintains screen-space position for world-space conversion by other systems.
    /// </summary>
    /// <param name="mouseInput">The raw mouse position input</param>
    /// <param name="rightStickInput">The raw right stick input from gamepad</param>
    /// <param name="usingGamepad">Flag indicating whether gamepad is the active input method</param>
    /// <returns>Processed aiming input (direction vector for gamepad, screen position for mouse)</returns>
    private float2 ProcessAimingInput(float2 mouseInput, float2 rightStickInput, bool usingGamepad)
    {
        if (usingGamepad)
        {
            // Apply deadzone filtering to right stick input
            float2 filteredStick = InputUtilities.ApplyDeadzone(rightStickInput, InnerDeadzone);
            
            // Convert stick input to direction vector when above deadzone
            float2 directionVector = InputUtilities.StickToDirection(filteredStick);
            
            return directionVector;
        }
        else
        {
            // Maintain screen-space position for mouse mode
            return mouseInput;
        }
    }
    
    /// <summary>
    /// Detects whether the player is using mouse or gamepad for aiming based on the most recent input.
    /// This enables seamless switching between input methods without requiring game restart.
    /// </summary>
    private void DetectInputMode()
    {
        // Check for gamepad aiming input (right stick)
        // Apply deadzone to avoid false positives from stick drift
        float rightStickMagnitude = math.length(rightStickInput);
        if (rightStickMagnitude > InnerDeadzone)
        {
            // Player is using gamepad for aiming
            isUsingGamepad = true;
            return;
        }
        
        // Check for mouse movement
        // Calculate mouse movement since last frame
        float2 mouseDelta = lookInput - lastMousePosition;
        float mouseMovement = math.length(mouseDelta);
        
        if (mouseMovement > MouseMovementThreshold)
        {
            // Player is using mouse for aiming
            isUsingGamepad = false;
        }
        
        // Update last mouse position for next frame
        lastMousePosition = lookInput;
    }
    
    /// <summary>
    /// Processes shooting input from both mouse button and gamepad trigger.
    /// Applies threshold to trigger input to ensure reliable activation.
    /// Returns 1.0 if either input method is active, 0.0 otherwise.
    /// </summary>
    /// <param name="mouseButtonInput">The raw mouse button input (0 or 1)</param>
    /// <param name="triggerInput">The raw trigger input (0 to 1)</param>
    /// <returns>Processed shoot value: 1.0 if shooting, 0.0 otherwise</returns>
    private float ProcessShootingInput(float mouseButtonInput, float triggerInput)
    {
        // Check if mouse button is pressed
        bool mouseButtonPressed = mouseButtonInput > 0.5f;
        
        // Apply threshold to right trigger input
        // Set Shoot to 1.0 when trigger exceeds threshold, 0.0 otherwise
        bool triggerPressed = triggerInput > TriggerThreshold;
        
        // Return 1.0 if either input method is active
        // This ensures continuous shooting works with held trigger or held mouse button
        return (mouseButtonPressed || triggerPressed) ? 1.0f : 0.0f;
    }
    
    /// <summary>
    /// Checks for gamepad connection/disconnection and handles the transition gracefully.
    /// Falls back to keyboard/mouse input when gamepad is disconnected.
    /// </summary>
    private void CheckGamepadConnection()
    {
        bool isGamepadConnected = Gamepad.current != null;
        
        // Detect disconnection
        if (wasGamepadConnected && !isGamepadConnected)
        {
            Debug.Log("[PlayerInputSystem] Gamepad disconnected. Falling back to keyboard/mouse input.");
            
            // Switch to keyboard/mouse mode
            isUsingGamepad = false;
            
            // Reset gamepad inputs to prevent stale values
            rightStickInput = float2.zero;
            rightTriggerInput = 0f;
        }
        // Detect connection
        else if (!wasGamepadConnected && isGamepadConnected)
        {
            Debug.Log("[PlayerInputSystem] Gamepad connected and ready for use.");
        }
        
        wasGamepadConnected = isGamepadConnected;
    }
}
