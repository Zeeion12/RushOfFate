using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Vector2 currentPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            currentPos = transform.position;
            Debug.Log(transform.position);
        }

        if (collision.gameObject.CompareTag("Trap"))
        {
            transform.position = currentPos;
        }
    }
}
