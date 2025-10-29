using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CommandQueue queue;
    public PlayerController player;

    readonly List<Direction> scratch = new List<Direction>();

    void Update()
    {
        if (WinUI.Instance != null && WinUI.Instance.IsOpen) return;
        if (player.IsExecuting) return;

        // Planning
        if (player.CanAddCommand(queue.Commands.Count))
    {
    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    queue.Add(Direction.Up);
    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  queue.Add(Direction.Down);
    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  queue.Add(Direction.Left);
    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) queue.Add(Direction.Right);
    }
    else
    {
    if (Input.anyKeyDown)
        Debug.Log("Max moves reached!");
    }
        if (Input.GetKeyDown(KeyCode.Backspace)) queue.PopBack();
        if (Input.GetKeyDown(KeyCode.R)) player.ResetToStart();

        // Execute
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            queue.CopyTo(scratch);
            Debug.Log($"Run {scratch.Count} commands");
            for (int k = 0; k < scratch.Count; k++) Debug.Log($"[GM] cmd[{k}] = {scratch[k]}");
            player.RunCommands(scratch);
            queue.Clear();
        }

        // Quick test key: fills a tiny path and runs it
        if (Input.GetKeyDown(KeyCode.T))
        {
            queue.Clear();
            queue.Add(Direction.Right);
            queue.Add(Direction.Up);
            queue.CopyTo(scratch);
            player.RunCommands(scratch);
        }
    }
}
