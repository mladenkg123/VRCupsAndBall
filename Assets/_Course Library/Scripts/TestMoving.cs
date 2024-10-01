using UnityEngine;

public class TestMoving : MonoBehaviour
{
    public float speed = 5f; 
    public float range = 3f; 

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float movement = Mathf.Sin(Time.time * speed) * range;
        transform.position = new Vector3(startPosition.x + movement, startPosition.y, startPosition.z);
    }
}
