using Godot;

namespace BoidsProject.data;



public record struct Alive;
public record struct BoidTag;
public record struct ObstacleTag;
public record struct TargetTag;

public record struct Id(int Value);

// Movement
public record struct Speed(float Value);
public record struct Position(Vector2 Value);
public record struct Direction(Vector2 Value);
