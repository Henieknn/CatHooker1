using UnityEngine;
using Mirror;

public class TrapProjectile : NetworkBehaviour
{
    public GameObject realTrapPrefab; // Префаб ловушки, которая останется на земле
    [HideInInspector] public Vector3 targetPos;
    
    private Vector3 startPos;
    private float progress = 0;
    public float flightSpeed = 1.5f;

    void Start() {
        startPos = transform.position;
    }

    void Update() {
        if (!isServer) return;

        progress += Time.deltaTime * flightSpeed;
        
        // Движение по дуге
        Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
        currentPos.y += Mathf.Sin(progress * Mathf.PI) * 5f;
        transform.position = currentPos;

        if (progress >= 1.0f) {
            SpawnRealTrap();
        }
    }

    [Server]
    void SpawnRealTrap() {
        GameObject trap = Instantiate(realTrapPrefab, targetPos, Quaternion.identity);
        NetworkServer.Spawn(trap);
        NetworkServer.Destroy(gameObject);
    }
}