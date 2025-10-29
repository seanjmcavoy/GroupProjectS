using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandQueue : MonoBehaviour
{
    readonly List<Direction> _commands = new List<Direction>();
    public IReadOnlyList<Direction> Commands => _commands;
    public event Action OnChanged;
    public void Add(Direction d) { _commands.Add(d); OnChanged?.Invoke(); Debug.Log($"[Queue({_commands.Count})] +{d}"); }
    public void PopBack() { if (_commands.Count > 0) { _commands.RemoveAt(_commands.Count - 1); OnChanged?.Invoke(); } }
    public void Clear() { _commands.Clear(); OnChanged?.Invoke(); }

    public void CopyTo(List<Direction> dst) { dst.Clear(); dst.AddRange(_commands); }
}
