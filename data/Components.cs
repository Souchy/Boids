using Godot;

namespace BoidsProject.data;



public record struct Alive;
public record struct BoidTag;
public record struct ObstacleTag;
public record struct TargetTag;

public record struct Id(int Value);

// Movement
public record struct Speed(float Value);
public record struct Position(Vector3 Value);
public record struct Direction(Vector3 Value);
public record struct Angle(float Value);
public record struct Quat(Quaternion Value);
public record struct Position2d(Vector2 Value);

// Partitioning
public record struct Partition(int Value);