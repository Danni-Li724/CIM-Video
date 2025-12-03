using UnityEngine;

public class SlowlyMove : MonoBehaviour
{
    public float moveSpeed = .2f;
    public bool isRight = false;

    private void Update()
    {
        if (isRight)
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }
    }
}
