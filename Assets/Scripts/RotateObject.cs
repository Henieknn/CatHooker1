using UnityEngine;
using Mirror;

public class RotateObject : NetworkBehaviour
{
    public float speed = 50f;

    void Update()
    {
        if (isServer)
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime);
        }
    }
}