# Rewind System

## 🎥 Watch the Introduction Video
[![Watch on YouTube](https://img.youtube.com/vi/xQvowNhNzmI/0.jpg)](https://www.youtube.com/watch?v=xQvowNhNzmI)

---

## 📖 Documentation

### Table of Contents
1. [Introduction](#1-introduction)
2. [High-Level Architecture](#2-high-level-architecture)
   - [Core Concepts and Flow](#21-core-concepts-and-flow)
3. [Core Components](#3-core-components)
   - [IRewindable](#31-irewindable)
   - [RewindInfo](#32-rewindinfo)
   - [ICoroutineRunner & DefaultMonoBehaviourRunner](#33-icoroutinerunner--defaultmonobehaviourrunner)
   - [IRewindBuffer & StructRingBuffer](#34-irewindbuffer--structringbuffer)
   - [RewindableStructBase](#35-rewindablestructbase)
   - [Rewinder](#36-rewinder)
4. [Component Rewinders](#4-component-rewinders)
   - [TransformRewinder](#41-transformrewinder)
   - [RigidbodyRewinder](#42-rigidbodyrewinder)
   - [Rigidbody2DRewinder](#43-rigidbody2drewinder)
   - [ParticleRewinder](#44-particlerewinder)
   - [AnimatorRewinder](#45-animatorrewinder)
   - [GameObjectRewinder](#46-gameobjectrewinder)
5. [Shared Data Structures](#5-shared-data-structures)
6. [Usage](#6-usage)

---

### 1. Introduction
The **Rewind System** is a robust framework designed to record and rewind the state of Unity objects over time. Its key design principles include:
- **Modularity**: Support for components like Transforms, Rigidbodies, Animators, and Particle Systems.
- **Extensibility**: Easy creation of custom rewinders using abstract base classes.
- **Performance**: Fixed-capacity ring buffer to prevent excessive memory usage.
- **Ease of Integration**: Smooth integration into Unity projects.

---

### 2. High-Level Architecture

#### 2.1 Core Concepts and Flow
1. **Recording**: Captures object states periodically, storing snapshots in a ring buffer.
2. **Buffering**: Fixed-capacity buffers overwrite older snapshots once full.
3. **Rewinding**:
   - **Smooth Rewind**: Interpolates states for fluid backward motion.
   - **Discrete Rewind**: Applies the latest snapshot directly.
4. **Core Workflow**:
   - Start and stop recording snapshots.
   - Retrieve and apply snapshots for rewinding.

---

### 3. Core Components

#### 3.1 IRewindable
Defines the basic contract for rewindable objects, including methods like:
- `StartRecord()`
- `StopRecord()`
- `StartRewind()`
- `StopRewind()`

#### 3.2 RewindInfo
Stores configuration details:
- `RecordInterval`: Time between snapshots.
- `RecordCapacity`: Maximum snapshots in the buffer.
- `SmoothRewind`: Enables state interpolation.
- `RewindSpeed`: Adjusts rewind speed.
- `RewindCurve`: Defines the interpolation curve.

#### 3.3 ICoroutineRunner & DefaultMonoBehaviourRunner
Provides coroutine management independent of specific MonoBehaviour scripts. A hidden GameObject hosts coroutines if needed.

#### 3.4 IRewindBuffer & StructRingBuffer
- **IRewindBuffer<T>**: Abstract interface for buffer operations.
- **StructRingBuffer<T>**: Implementation using a fixed-size array for efficient memory usage.

#### 3.5 RewindableStructBase
Abstract base for all rewinding logic:
- Defines methods for recording (`SetRecordSlot`) and applying (`ApplyState`) data.
- Handles events like `OnRewindStarted` and `OnRewindEnded`.

#### 3.6 Rewinder
Centralized manager for multiple IRewindable objects. Allows batch recording and rewinding of registered entities.

---

### 4. Component Rewinders

#### 4.1 TransformRewinder
- Tracks position, rotation, and scale of objects.
- Supports smooth interpolation during rewinds.

#### 4.2 RigidbodyRewinder
- Captures Rigidbody states (e.g., velocity, mass, and drag).
- Temporarily sets `isKinematic` during rewinds to prevent physics interference.

#### 4.3 Rigidbody2DRewinder
- Handles 2D Rigidbody properties like velocity, rotation, and gravity scale.

#### 4.4 ParticleRewinder
- Captures particle data and repositions particles during rewinds.

#### 4.5 AnimatorRewinder
- Tracks Animator parameters, state info, and HumanPose data.

#### 4.6 GameObjectRewinder
- Records properties like `activeSelf`, layer, and parent transform.

---

### 5. Shared Data Structures
Defines reusable structs for captured data, such as:
- `TransformData`: Position, rotation, and scale.
- `RigidbodyData`: Velocity, angular velocity, and mass.
- `AnimatorData`: Parameters and state info.

---

### 6. Usage

#### Example Implementation
```csharp
// Create a Rewinder
var transformRewinder = new TransformRewinder(myTransform, new RewindInfo(
    recordInterval: 0.1f,
    recordCapacity: 100,
    smoothRewind: true,
    rewindSpeed: 1f
));

// Start Recording
transformRewinder.StartRecord();
// Stop Recording
transformRewinder.StopRecord();

// Start Rewind
transformRewinder.StartRewind();
// Stop Rewind
transformRewinder.StopRewind();
