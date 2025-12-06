using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

// Register UnityEngine components with the ECS TypeManager
// Required in Unity 6 + Entities 1.4 for hybrid GameObject/ECS scenarios

// Register Animator component (used in PlayerAnimationSystem, EnemyDeathSystem, PlayerHitFxSystem)
[assembly: RegisterUnityEngineComponentType(typeof(Animator))]

// Register NavMeshAgent component (used in EnemyMovementSystem)
[assembly: RegisterUnityEngineComponentType(typeof(NavMeshAgent))]

// Register AudioSource component (used in multiple systems)
[assembly: RegisterUnityEngineComponentType(typeof(AudioSource))]

// Register CapsuleCollider component (used in EnemyDeathSystem)
[assembly: RegisterUnityEngineComponentType(typeof(CapsuleCollider))]

// Register Rigidbody component (used in PlayerMovementSystem, PlayerTurningSystem)
[assembly: RegisterUnityEngineComponentType(typeof(Rigidbody))]

// Register Transform component (used in CameraFollowSystem and others)
[assembly: RegisterUnityEngineComponentType(typeof(Transform))]
