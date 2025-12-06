# Implementation Plan

- [x] 1. Backup existing systems and prepare for migration





  - Create backup copies of CameraFollowSystem.cs and EnemySpawnSystem.cs
  - Document current system behavior for regression testing
  - _Requirements: 8.1, 8.2_

- [x] 2. Upgrade CameraFollowSystem to ISystem






- [x] 2.1 Implement new CameraFollowSystem using ISystem interface


  - Remove `[DisableAutoCreation]` attribute
  - Convert from SystemBase to ISystem struct
  - Implement OnCreate with RequireForUpdate<PlayerData>
  - Implement OnUpdate with SystemAPI queries
  - Add null safety checks for Camera.main and Transform
  - Maintain offset calculation on first frame
  - Use Vector3.Lerp with configured smoothing value
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 3.1, 3.3, 3.7, 4.2, 4.4_

- [ ]* 2.2 Write property test for camera offset maintenance
  - **Property 1: Camera maintains offset during player movement**
  - **Validates: Requirements 1.2, 1.3**

- [ ]* 2.3 Write unit tests for camera system edge cases
  - Test offset calculation on first frame
  - Test camera behavior when player is destroyed
  - Test camera behavior when no player exists
  - Test null Camera.main handling
  - _Requirements: 1.1, 1.4, 8.2_

- [-] 3. Update EnemySpawner MonoBehaviour for hybrid architecture


- [x] 3.1 Add entity creation to EnemySpawner.Start()



  - Create entity in Start() method
  - Add EnemySpawnerData component with SpawnTime and CurrentTime
  - Add Transform as managed component
  - Add self reference as managed component
  - Store entity reference in public Entity field
  - Add debug logging for initialization
  - _Requirements: 4.3, 4.5, 8.5_

- [ ]* 3.2 Write unit test for spawner initialization
  - Test entity creation during Start()
  - Test component registration
  - Test multiple spawners in scene
  - _Requirements: 2.1, 4.5_

- [x] 4. Create EnemySpawnerData component




- [x] 4.1 Implement EnemySpawnerData struct

  - Define as IComponentData struct
  - Add SpawnTime field (float)
  - Add CurrentTime field (float)
  - Add PrefabEntity field (Entity) for future optimization
  - _Requirements: 2.2, 2.3, 2.6_

- [x] 5. Upgrade EnemySpawnSystem to ISystem



- [x] 5.1 Implement new EnemySpawnSystem using ISystem interface



  - Convert from SystemBase to ISystem struct
  - Implement OnCreate with RequireForUpdate<EnemySpawnerData>
  - Query for player health and check if alive
  - Early exit if player health <= 0 or player doesn't exist
  - Query for all spawners with Transform, EnemySpawner, and EnemySpawnerData
  - Increment CurrentTime by deltaTime for each spawner
  - Spawn enemy when CurrentTime >= SpawnTime
  - Reset CurrentTime to 0 after spawning
  - Add null safety checks for Transform and EnemySpawner
  - Add debug logging for null components
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 3.2, 3.3, 3.7, 4.3, 4.4_

- [ ]* 5.2 Write property test for spawn location accuracy
  - **Property 2: Enemies spawn at correct locations when timer expires**
  - **Validates: Requirements 2.2**

- [ ]* 5.3 Write property test for timer reset consistency
  - **Property 3: Spawn timer resets after spawning**
  - **Validates: Requirements 2.3**

- [ ]* 5.4 Write property test for spawner independence
  - **Property 4: Spawners operate independently**
  - **Validates: Requirements 2.6**

- [ ]* 5.5 Write property test for null safety
  - **Property 5: Systems handle null managed components safely**
  - **Validates: Requirements 4.4, 8.4**

- [ ]* 5.6 Write unit tests for spawn system edge cases
  - Test spawning stops when player health is zero
  - Test spawning stops when player entity doesn't exist
  - Test null transform handling
  - Test null EnemySpawner MonoBehaviour handling
  - _Requirements: 2.4, 2.5, 4.4, 8.2_

- [ ] 6. Checkpoint - Verify core systems work in main game scene
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Create performance_lab scene



- [x] 7.1 Set up basic scene structure


  - Create new scene named "performance_lab"
  - Add Directional Light
  - Add Main Camera with standard setup
  - Copy lighting settings from main game scene
  - _Requirements: 5.1, 5.8_

- [x] 7.2 Create high-contrast floor


  - Create Plane GameObject for floor
  - Create PerformanceLabFloor material
  - Set material to bright white albedo with black grid texture
  - Configure tiling to 10x10
  - Apply material to floor plane
  - _Requirements: 5.2_

- [x] 7.3 Add game objects to scene


  - Instantiate Settings GameObject with SurvivalShooterSettings
  - Instantiate Player prefab
  - Instantiate GameUi prefab
  - Position objects appropriately
  - _Requirements: 5.3, 5.6_

- [x] 7.4 Add enemy spawners to scene


  - Create 4 EnemySpawner GameObjects at different positions
  - Configure Spawner_1 with SpawnTime=3s
  - Configure Spawner_2 with SpawnTime=5s
  - Configure Spawner_3 with SpawnTime=2s
  - Configure Spawner_4 with SpawnTime=4s
  - Assign enemy prefab references
  - _Requirements: 5.6_

- [ ] 8. Implement PlayerDebugWindow component
- [x] 8.1 Create PlayerDebugWindow MonoBehaviour

  - Add fields for cached metrics (fps, frameTime, moveInput, lookInput, health, position)
  - Add configurable offset for window position
  - Add updateInterval field (default 0.1s)
  - Add targetFrameTime threshold field (default 16.67ms)
  - Implement Start() to find player entity
  - Implement Update() to call UpdateMetrics() at intervals
  - Implement UpdateMetrics() to read player component data
  - Implement CountEntitiesWithComponent<T>() helper method
  - _Requirements: 6.1, 6.2, 7.1, 7.2_

- [x] 8.2 Implement ALINE debug rendering


  - Import ALINE package if not already imported
  - Add using Drawing directive
  - Implement RenderDebugInfo() method
  - Use Draw.WithDuration() for efficient rendering
  - Use Draw.Label3D() for text display
  - Display FPS, frame time, health, move input, position, entity counts
  - Use color coding (green for good performance, red for poor)
  - Draw WireSphere indicator when frame time exceeds threshold
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 7.6_

- [ ]* 8.3 Write property test for debug display accuracy
  - **Property 6: Debug window displays accurate real-time data**
  - **Validates: Requirements 6.2, 6.4**

- [ ]* 8.4 Write property test for entity count accuracy
  - **Property 7: Entity counts match actual world state**
  - **Validates: Requirements 7.2**

- [ ]* 8.5 Write property test for performance indicator triggering
  - **Property 8: Visual indicators appear when performance degrades**
  - **Validates: Requirements 7.6**

- [ ]* 8.6 Write unit tests for debug window
  - Test debug window attachment to player
  - Test metric calculation accuracy
  - Test visual indicator triggering at threshold
  - _Requirements: 6.1, 7.1, 7.3, 7.6_

- [ ] 9. Add PlayerDebugWindow to player prefabs
- [ ] 9.1 Attach debug window to player in performance_lab scene
  - Add PlayerDebugWindow component to Player GameObject
  - Configure offset (0, 3, 0)
  - Configure updateInterval (0.1s)
  - Configure targetFrameTime (16.67ms)
  - _Requirements: 6.1_

- [ ] 9.2 Attach debug window to player in main game scene
  - Add PlayerDebugWindow component to Player prefab
  - Use same configuration as performance_lab
  - _Requirements: 6.1_

- [ ] 10. Add conditional compilation for debug features
- [ ] 10.1 Add preprocessor directives to PlayerDebugWindow
  - Wrap ALINE drawing code in #if UNITY_EDITOR || DEVELOPMENT_BUILD
  - Add #define ENABLE_DEBUG_WINDOW guard
  - Ensure zero overhead in release builds
  - _Requirements: 6.7, 7.7_

- [ ] 11. Test in performance_lab scene
- [ ] 11.1 Validate camera system
  - Start game in performance_lab scene
  - Verify camera follows player smoothly
  - Verify offset is maintained
  - Check console for errors
  - _Requirements: 1.1, 1.2, 1.3, 8.2_

- [ ] 11.2 Validate enemy spawning
  - Verify enemies spawn at spawner locations
  - Verify spawn timers work independently
  - Verify spawning stops when player dies
  - Check console for errors
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.6, 8.2_

- [ ] 11.3 Validate debug window
  - Verify debug window appears above player
  - Verify all metrics display correctly
  - Verify performance indicator appears when frame time is high
  - Verify entity counts are accurate
  - _Requirements: 6.1, 6.2, 6.4, 7.1, 7.2, 7.6_

- [ ] 12. Performance testing and optimization
- [ ] 12.1 Run performance tests in performance_lab
  - Test with 10 enemies - measure FPS and frame time
  - Test with 50 enemies - measure FPS and frame time
  - Test with 100 enemies - measure FPS and frame time
  - Profile CameraFollowSystem execution time
  - Profile EnemySpawnSystem execution time
  - Document performance metrics
  - _Requirements: 7.4_

- [ ] 12.2 Optimize if needed
  - Identify bottlenecks using Unity Profiler
  - Apply optimizations to meet target metrics (60 FPS with 100 enemies)
  - Verify camera system < 0.1ms per frame
  - Verify spawn system < 0.2ms per frame
  - _Requirements: 3.8, 3.9, 3.10_

- [ ] 13. Final validation in main game scene
- [ ] 13.1 Test complete gameplay
  - Start game in main game scene
  - Verify camera follows player
  - Verify enemies spawn correctly
  - Verify debug window displays correctly
  - Play for 5 minutes to ensure stability
  - Check console for any errors or warnings
  - _Requirements: 5.7, 8.1, 8.2, 8.3_

- [ ] 13.2 Verify no deprecation warnings
  - Check console for ECS API deprecation warnings
  - Verify no ForEach pattern warnings
  - Verify no ToComponentArray warnings
  - _Requirements: 3.4, 3.5, 8.3_

- [ ] 14. Final Checkpoint - Complete system validation
  - Ensure all tests pass, ask the user if questions arise.
