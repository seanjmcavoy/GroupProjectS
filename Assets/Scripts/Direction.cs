using UnityEngine;

public enum Direction { Up, Down, Left, Right }

public static class DirectionExtensions
{
    public static Vector2Int ToOffset(this Direction d)
    {
        switch (d)
        {
            case Direction.Up:    return Vector2Int.up;    // (0, +1)  -> +Z
            case Direction.Down:  return Vector2Int.down;  // (0, -1)  -> -Z
            case Direction.Left:  return Vector2Int.left;  // (-1, 0)  -> -X
            case Direction.Right: return Vector2Int.right; // (+1, 0)  -> +X
            default:              return Vector2Int.zero;
        }
    }
}
