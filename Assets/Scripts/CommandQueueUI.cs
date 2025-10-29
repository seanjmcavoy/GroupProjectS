using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandQueueUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public CommandQueue queue;
    public PlayerController player;
    public Transform container;
    public GameObject arrowPrefab; 

    readonly List<GameObject> arrows = new List<GameObject>();

    void OnEnable()
    {
        if (queue != null)
            queue.OnChanged += Refresh;
    }

    void OnDisable()
    {
        if (queue != null)
            queue.OnChanged -= Refresh;
    }

    void Refresh()
    {
        foreach (var go in arrows) Destroy(go);
        arrows.Clear();

        var cmds = queue.Commands;
        for (int i = 0; i < cmds.Count; i++)
        {
            var arrow = Instantiate(arrowPrefab, container);
            arrows.Add(arrow);

            var img = arrow.GetComponent<Image>();
            switch (cmds[i])
            {
                case Direction.Up:    img.transform.rotation = Quaternion.Euler(0, 0, 0); break;
                case Direction.Down:  img.transform.rotation = Quaternion.Euler(0, 0, 180); break;
                case Direction.Left:  img.transform.rotation = Quaternion.Euler(0, 0, 90); break;
                case Direction.Right: img.transform.rotation = Quaternion.Euler(0, 0, -90); break;
            }
        }
    }
}
