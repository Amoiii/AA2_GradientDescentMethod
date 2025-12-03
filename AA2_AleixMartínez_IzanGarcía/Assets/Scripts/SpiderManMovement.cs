using UnityEngine;

public class SpiderManMovement : MonoBehaviour
{
    public float speed = 3.0f; 
    public Vector3 areaSize = new Vector3(5, 0, 5); 
    private Vector3 targetPos;

    void Start()
    {
        GetNewPosition();
    }

    void Update()
    {
        // Moverse hacia el punto destino
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Si llega al destino, busca otro nuevo (movimiento impredecible)
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            GetNewPosition();
        }
    }

    void GetNewPosition()
    {
        //punto aleatorio
        float x = Random.Range(-areaSize.x, areaSize.x);
        float z = Random.Range(-areaSize.z, areaSize.z);
        targetPos = new Vector3(x, transform.position.y, z);
    }
}