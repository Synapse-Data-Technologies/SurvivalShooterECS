# Implementation Plan

- [x] 1. Enhance PlayerInputData component with gamepad support





  - Add `IsUsingGamepad` flag to track active input method
  - Update component to support both screen-space (mouse) and direction-based (stick) look inputs
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [x] 2. Implement input utility functions





  - Create deadzone filtering function that zeros inputs below threshold
  - Create normalization function to map [deadzone, max] range to [0, 1]
  - Create angle-to-direction conversion function for stick inputs
  - _Requirements: 1.2, 2.2, 4.1, 4.2_

- [ ]* 2.1 Write property test for deadzone filtering
  - **Property 2: Deadzone filtering for all analog inputs**
  - **Validates: Requirements 1.2, 2.2, 4.1**

- [ ]* 2.2 Write property test for deadzone normalization
  - **Property 9: Deadzone normalization**
  - **Validates: Requirements 4.2**

- [x] 3. Update PlayerInputSystem with gamepad input actions





  - Add right stick input action for gamepad aiming
  - Add right trigger input action for gamepad shooting
  - Update existing move action to properly support left stick
  - Configure deadzone values from Unity Input System settings
  - _Requirements: 2.1, 3.1, 4.3, 6.4_

- [x] 4. Implement input mode detection




  - Detect when player uses mouse vs gamepad for aiming
  - Set `IsUsingGamepad` flag based on most recent input device
  - Handle seamless switching between input methods
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 4.4_

- [ ]* 4.1 Write example test for input mode switching
  - Test switching from mouse to gamepad and back
  - **Validates: Requirements 5.3, 5.4**

- [x] 5. Implement gamepad movement processing





  - Apply deadzone filtering to left stick input
  - Normalize stick input after deadzone
  - Update Move field in PlayerInputData with processed values
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [ ]* 5.1 Write property test for movement direction and magnitude
  - **Property 1: Movement direction and magnitude**
  - **Validates: Requirements 1.1**

- [ ]* 5.2 Write property test for input release stops movement
  - **Property 3: Input release stops action**
  - **Validates: Requirements 1.4, 3.3**

- [x] 6. Implement gamepad aiming processing





  - Apply deadzone filtering to right stick input
  - Convert stick input to direction angle when above deadzone
  - Update Look field with direction vector for gamepad mode
  - Maintain screen-space position for mouse mode
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [ ]* 6.1 Write property test for aiming direction
  - **Property 4: Aiming direction matches stick angle**
  - **Validates: Requirements 2.1**

- [ ]* 6.2 Write property test for continuous aiming updates
  - **Property 5: Continuous aiming updates**
  - **Validates: Requirements 2.3**

- [ ]* 6.3 Write property test for independent input processing
  - **Property 6: Independent input processing**
  - **Validates: Requirements 2.4**

- [x] 7. Update PlayerTurningSystem for gamepad aiming





  - Detect if using gamepad mode from PlayerInputData
  - When using gamepad, interpret Look as direction vector instead of screen position
  - Calculate rotation from direction vector
  - Maintain existing mouse-based rotation for mouse mode
  - _Requirements: 2.1, 5.1, 5.2_

- [ ]* 7.1 Write property test for mouse aiming calculation
  - **Property 10: Mouse aiming calculation**
  - **Validates: Requirements 5.1**

- [x] 8. Implement gamepad shooting with trigger




  - Apply threshold to right trigger input (default 0.5)
  - Set Shoot field to 1.0 when trigger exceeds threshold, 0.0 otherwise
  - Ensure continuous shooting works with held trigger
  - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [ ]* 8.1 Write property test for trigger threshold activation
  - **Property 7: Trigger threshold activation**
  - **Validates: Requirements 3.1, 3.4**

- [ ]* 8.2 Write property test for continuous shooting timing
  - **Property 8: Continuous shooting timing**
  - **Validates: Requirements 3.2**

- [x] 9. Add error handling and edge cases




  - Handle NaN and infinite values from input devices
  - Gracefully handle controller disconnection
  - Add logging for missing or misconfigured input actions
  - _Requirements: 4.3, 4.4_

- [ ]* 9.1 Write unit tests for error handling
  - Test controller disconnection scenarios
  - Test invalid input value handling
  - Test missing input action configuration

- [ ] 10. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 11. Test integration with existing systems
  - Verify PlayerMovementSystem works with gamepad input
  - Verify PlayerShootingSystem works with trigger input
  - Verify PlayerAnimationSystem responds to gamepad movement
  - Test simultaneous keyboard/mouse and gamepad input
  - _Requirements: 4.4, 5.3, 5.4_

- [ ]* 11.1 Write integration tests
  - Test end-to-end gameplay with gamepad
  - Test switching between input methods during gameplay
  - Test multiple input devices connected simultaneously
