using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class Queue : MonoBehaviour
{
    Queue<KeyCode> logger = new Queue<KeyCode>();
    //movement distance
    public float moveDistance = 10f;
    public float delayBetweenMoves = 0.5f;
    private Vector3 startPosition;
    private bool isExecuting = false;
    private bool levelComplete = false;

    void Start()
    {
        // reset pos
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isExecuting && !levelComplete)
        {
            RecordInput();
        }
    }

    //movement queue 
    void RecordInput()
    {   
        //queue keys inputs
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            logger.Enqueue(KeyCode.UpArrow);
            Debug.Log("Up " + logger.Count);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            logger.Enqueue(KeyCode.DownArrow);
            Debug.Log("Down " + logger.Count);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            logger.Enqueue(KeyCode.RightArrow);
            Debug.Log("Right " + logger.Count);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            logger.Enqueue(KeyCode.LeftArrow);
            Debug.Log("Left " + logger.Count);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartCoroutine(ExecuteMovementSequence());
        }
    }

    IEnumerator ExecuteMovementSequence()
    {
        isExecuting = true;
        // movement loop
        foreach (KeyCode input in logger)
        {
            
            if (input == KeyCode.UpArrow)
            {
                transform.position += Vector3.forward * moveDistance;
            }
            else if (input == KeyCode.DownArrow)
            {
                transform.position += Vector3.back * moveDistance;
            }
            else if (input == KeyCode.LeftArrow)
            {
                transform.position += Vector3.left * moveDistance;
            }
            else if (input == KeyCode.RightArrow)
            {
                transform.position += Vector3.right * moveDistance;
            }

            // delay movement? 
            // maybe add animations idk
            // doesnt really work wel
            yield return new WaitForSeconds(delayBetweenMoves);
        }
        logger.Clear();
        isExecuting = false;
        // if not in trigger by end
        if (levelComplete == false)
        {
            transform.position = startPosition;
        }
    }
    // end zone trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            Debug.Log("Finish, Reload");
            levelComplete = true;
            
            // hide object once finished
            other.GetComponent<MeshRenderer>().enabled = false;
            
            // copy paste reloader
            StartCoroutine(ReloadLevel());
        }
    }
    //reload scene if reach end, temp for testing
    IEnumerator ReloadLevel()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}