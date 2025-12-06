# Implementation Plan

- [x] 1. Backup and prepare for migration





  - Create backup of EnemyMovementSystem.cs
  - Document current behavior for regression testing
  - _Requirements: 7.1, 7.2_

- [x] 2. Upgrade EnemyMovementSystem to ISystem





- [x] 2.1 Implement new EnemyMovementSystem using ISystem interface


  - Convert from SystemBase to ISystem struct
  - Implement OnCreate with RequireForUpdate<EnemyData>
  - Query for player entity with PlayerData, HealthData, Transform
  - Early exit if no player exists
  - Get player position and health status
  - Query for all living enemies (with EnemyData, HealthData, without DeadData)
  - For each enemy, get NavMeshAgent managed component
  - Set NavMeshAgent destination to player position if both alive
  - Disable NavMeshAgent if player or enemy is dead
  - Add null safety checks for Transform and NavMeshAgent
  - Add debug logging for null components
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 3.1, 3.2, 3.3, 4.1, 4.2, 4.3_

- [ ]* 2.2 Write property test for enemy movement toward player
  - **Property 1: Enemies move toward player when alive**
  - **Validates: Requirements 1.1, 1.2**

- [ ]* 2.3 Write property test for movement stops on player death
  - **Property 2: Enemies stop when player dies**
  - **Validates: Requirements 1.3**

- [ ]* 2.4 Write property test for dead enemy stops
  - **Property 3: Dead enemies stop moving**
  - **Validates: Requirements 1.4**

- [ ]* 2.5 Write property test for null safety
  - **Property 4: Systems handle null managed components safely**
  - **Validates: Requirements 4.3, 7.4**

- [ ]* 2.6 Write unit tests for edge cases
  - Test no player in scene
  - Test null NavMeshAgent handling
  - Test null Transform handling
  - _Requirements: 4.3, 7.4_

- [ ] 3. Test in Game scene
  - Start game in Game scene
  - Verify enemies spawn and move toward player
  - Verify enemies stop when player dies
  - Verify enemies stop when they die
  - Check console for errors
  - _Requirements: 5.1, 5.2, 5.3, 7.1, 7.2_

- [ ] 4. Test in performance_lab scene
  - Start game in performance_lab scene
  - Verify enemies spawn from spawn points
  - Verify enemies move toward player
  - Verify enemies attack when in range
  - Test with 10 enemies - measure FPS
  - Test with 50 enemies - measure FPS
  - Test with 100 enemies - measure FPS
  - Verify debug window shows enemy count
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [ ] 5. Performance validation
  - Profile EnemyMovementSystem execution time
  - Verify < 0.5ms per frame with 100 enemies
  - Verify no GC allocations in hot path
  - Document performance metrics
  - _Requirements: 5.4_

- [x] 6. Evaluate EnemyDeathSystem for upgrade





  - Review EnemyDeathSystem implementation
  - Determine if ISystem upgrade is needed
  - Document findings and recommendations
  - _Requirements: 3.6_

- [x] 7. Document modular AI extension points





  - Document how to add AI behavior components
  - Document how to create AI decision systems
  - Provide example plugin architecture
  - Update design document with extension guide
  - _Requirements: 6.1, 6.2, 6.3, 6.4_

- [ ] 8. Final validation
  - Ensure all tests pass
  - Verify no deprecation warnings
  - Verify no runtime errors
  - Play both scenes for 5 minutes each
  - _Requirements: 7.1, 7.2, 7.3, 7.4_
