using UnityEngine;

public class Wall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
{
    var player = other.GetComponent<PlayerController>();
    if (player != null)
    {
        Debug.Log("Wall hit");
        player.ResetToStart();
    }
}

}
