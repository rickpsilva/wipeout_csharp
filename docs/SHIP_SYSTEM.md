# Ship System Documentation

## Overview

The Ship system implements the complete racing ship entity for the Wipeout game, including physics simulation, 3D rendering, and shadow projection. The implementation is based on the original C code from [`wipeout-rewrite/src/wipeout/ship.c`](../../wipeout-rewrite/src/wipeout/ship.c).

## Architecture

### Core Components

#### Ship.cs
**Location:** `src/Core/Entities/Ship.cs`

The main ship entity class containing:
- **Physics simulation** - Position, velocity, acceleration, angular motion
- **Direction vectors** - Forward, right, up vectors calculated from Euler angles
- **Rendering** - 3D model transformation and shadow projection
- **State management** - Flying, racing, visibility, destruction

#### Vec3.cs
**Location:** `src/Vec3.cs`

3D vector mathematics library providing:
- Basic operations: Add, Subtract, Multiply, Divide
- Vector operations: Length, Normalize, Dot product, Cross product
- Plane math: DistanceToPlane, ProjectOntoPlane
- Operator overloads for clean syntax

#### Mat4.cs
**Location:** `src/Mat4.cs`

4x4 transformation matrix system for 3D rendering:
- Identity and Translation matrices
- Rotation from Euler angles
- Matrix multiplication
- Combined position + rotation transforms

## Physics System

### Constants

Defined in `Ship.cs` based on original game physics:

```csharp
private const float ShipFlyingGravity = 80000.0f;      // Gravity when airborne
private const float ShipOnTrackGravity = 30000.0f;     // Gravity when on track
private const float ShipMinResistance = 20.0f;          // Minimum air resistance
private const float ShipMaxResistance = 74.0f;          // Maximum air resistance
private const float ShipTrackMagnet = 64.0f;            // Magnetic pull to track
private const float ShipTrackFloat = 256.0f;            // Hover height above track
```

### Update Loop

The `Ship.Update(float deltaTime)` method implements the physics simulation:

1. **Direction Vectors Calculation**
   ```csharp
   UpdateDirectionVectors()
   ```
   - Calculates forward/right/up vectors from pitch/yaw/roll angles
   - Uses rotation matrix math (sin/cos of angles)
   - Ensures vectors are orthogonal and unit length

2. **Position Integration**
   ```csharp
   Position = Position + Velocity * deltaTime
   ```
   - Euler integration of velocity to position
   - Handles 3D movement in XYZ space

3. **Angle Integration**
   ```csharp
   Angle = Angle + AngularVelocity * deltaTime
   ```
   - Rotational motion from angular velocity
   - Updates pitch, yaw, roll angles

4. **Speed Calculation**
   ```csharp
   Speed = Velocity.Length()
   ```
   - Magnitude of velocity vector
   - Used for HUD display and gameplay logic

### Direction Vector Calculation

Based on `ship_update()` from original `ship.c`:

```csharp
private void UpdateDirectionVectors()
{
    float sx = MathF.Sin(Angle.X);  // pitch
    float cx = MathF.Cos(Angle.X);
    float sy = MathF.Sin(Angle.Y);  // yaw
    float cy = MathF.Cos(Angle.Y);
    float sz = MathF.Sin(Angle.Z);  // roll
    float cz = MathF.Cos(Angle.Z);
    
    // Forward vector
    DirForward = new Vec3(
        -(sy * cx),
        -sx,
        cy * cx
    );
    
    // Right vector
    DirRight = new Vec3(
        cy * sz + sy * sx * cz,
        cx * cz,
        sy * sz - cy * sx * cz
    );
    
    // Up vector (cross product)
    DirUp = DirForward.Cross(DirRight);
}
```

## Rendering System

### 3D Model Rendering

#### Transformation Matrix
The `CalculateTransformMatrix()` method creates a combined transformation:

```csharp
public Mat4 CalculateTransformMatrix()
{
    return Mat4.FromPositionAndAngles(Position, Angle);
}
```

This matrix:
- Translates model to ship position
- Rotates model by ship angles (pitch/yaw/roll)
- Applied to all vertices during rendering

#### Render Method
```csharp
public void Render(IRenderer renderer)
```

Based on `ship_draw()` from `ship.c`:
- Checks visibility flag
- Calculates transformation matrix
- Renders 3D model (TODO: PRM file loading)
- Handles exhaust plume effects (TODO)

#### Helper Position Methods

Essential for camera, collision, and shadow rendering:

**Cockpit Position** - Camera mounting point
```csharp
public Vec3 GetCockpitPosition()
{
    return Position + DirForward * 150 - DirUp * 60;
}
```

**Nose Position** - Front collision point
```csharp
public Vec3 GetNosePosition()
{
    return Position + DirForward * 384;
}
```

**Wing Positions** - Side collision and shadow points
```csharp
public Vec3 GetWingLeftPosition()
{
    return Position - DirRight * 256 - DirForward * 384;
}

public Vec3 GetWingRightPosition()
{
    return Position + DirRight * 256 - DirForward * 384;
}
```

### Shadow Rendering

Based on `ship_draw_shadow()` from `ship.c`:

```csharp
public void RenderShadow(IRenderer renderer)
```

#### Algorithm

1. **Calculate 3D Positions**
   ```csharp
   Vec3 nose = GetNosePosition();
   Vec3 wingLeft = GetWingLeftPosition();
   Vec3 wingRight = GetWingRightPosition();
   ```

2. **Project onto Track Surface**
   ```csharp
   Vec3 noseProjected = nose.ProjectOntoPlane(trackFacePoint, trackNormal);
   Vec3 wingLeftProjected = wingLeft.ProjectOntoPlane(trackFacePoint, trackNormal);
   Vec3 wingRightProjected = wingRight.ProjectOntoPlane(trackFacePoint, trackNormal);
   ```
   
   Uses plane projection math:
   - Calculate distance to plane: `d = (point - planePoint) · normal`
   - Project: `projected = point - normal * d`

3. **Render Shadow Triangle**
   ```csharp
   renderer.PushTri(
       wingLeftProjected, uv(0,256), color(0,0,0,0.5),
       wingRightProjected, uv(128,256), color(0,0,0,0.5),
       noseProjected, uv(64,0), color(0,0,0,0.5)
   );
   ```
   
   - Semi-transparent black (50% alpha)
   - UV coordinates match original shadow texture
   - Only rendered when ship is on ground (not flying)

#### Shadow Texture

Original game uses 4 shadow textures (`shad1.tim` - `shad4.tim`):
- Loaded as semi-transparent textures
- Assigned per ship: `shadow_texture = start + (shipId >> 1)`
- 2 ships share same shadow texture (memory optimization)

## Testing Approach

### Test Structure

The ship system has comprehensive unit tests achieving **>90% coverage**:

#### ShipPhysicsTests.cs (23 tests)
- Constructor initialization
- Direction vector calculation
- Orthogonality of direction vectors
- Unit length verification
- Velocity integration
- Angular velocity updates
- Speed calculation
- Multiple frame updates
- Damage system

#### ShipRenderingTests.cs (17 tests)
- Transformation matrix calculation
- Helper position methods
- Wing symmetry
- Rotation following
- Visibility guards
- Render and RenderShadow behavior

#### Mat4Tests.cs (15 tests)
- Identity matrix
- Translation matrix
- Rotation from Euler angles
- Matrix multiplication
- Combined transforms

#### Vec3Tests.cs (23 tests)
- Basic vector operations
- Dot and cross products
- Distance calculations
- Plane distance calculation
- Plane projection
- Operator overloads

### Test Results

**Total:** 153/153 tests passing ✅

**Coverage:**
- `Ship.cs`: 97.24% (141/145 statements)
- `Vec3.cs`: 91.67% (33/36 statements)
- `Mat4.cs`: 100% (all statements)

### Testing Strategy

1. **Unit Tests** - Test individual methods in isolation
2. **Mock Dependencies** - Use `Mock<IRenderer>` to avoid graphics dependencies
3. **Property-Based** - Test mathematical properties (orthogonality, unit length)
4. **Integration Tests** - Test complete update loop over multiple frames
5. **Boundary Conditions** - Test edge cases (zero values, large values)

## Integration with Other Systems

### Renderer Integration
```csharp
public interface IRenderer
{
    void PushTri(Vector3 a, Vector2 uvA, Vector4 colorA,
                 Vector3 b, Vector2 uvB, Vector4 colorB,
                 Vector3 c, Vector2 uvC, Vector4 colorC);
}
```

Ships call `PushTri` to render:
- Shadow triangles
- 3D model faces (TODO)
- Exhaust plume particles (TODO)

### Track Integration (TODO)
Ships need track system for:
- `Section` - Current track section
- `track_face_t` - Track surface for collision and shadow projection
- Track normal vectors for proper shadow projection

### Camera Integration
Camera follows ship using:
```csharp
Vec3 cameraPos = ship.GetCockpitPosition();
Vec3 lookAt = ship.Position + ship.DirForward * distance;
```

## Future Work

### PRM Model Loading
Implement 3D model loading from Playstation PRM format:
- Parse binary PRM file structure
- Load vertices, faces, normals
- Load texture coordinates
- Create OpenGL buffers

### Track Integration
Complete shadow rendering:
- Get `Section` reference from track system
- Get track face below ship: `track_section_get_base_face(section)`
- Use actual track normal for projection
- Handle junctions and special sections

### Exhaust Plume
Particle effects from engine:
- 3 plume vertices per ship
- Updated based on thrust
- Rendered with additive blending

### Collision Detection
Implement collision with:
- Track walls (nose/wing positions)
- Other ships
- Weapons and pickups

## References

- Original C implementation: [`wipeout-rewrite/src/wipeout/ship.c`](../../wipeout-rewrite/src/wipeout/ship.c)
- Ship header: [`wipeout-rewrite/src/wipeout/ship.h`](../../wipeout-rewrite/src/wipeout/ship.h)
- Vector math: [`wipeout-rewrite/src/types.c`](../../wipeout-rewrite/src/types.c)
- Object rendering: [`wipeout-rewrite/src/wipeout/object.c`](../../wipeout-rewrite/src/wipeout/object.c)

## Commit History

- **db2933c** - Initial ship physics implementation
- **66345b3** - Mat4 transforms and rendering structure
- **d279574** - Shadow rendering with plane projection
