using UnityEngine;

public class SpiderManMovement : MonoBehaviour
{
    public float speed = 5.0f;

   
    public Vector3 areaSize = new Vector3(5, 4, 5);

    public float minHeight = 0.5f;

   
    public float minZ = 2.0f;

    private Vector3 targetPos;

    void Start()
    {
        GetNewPosition();
    }

    void Update()
    {
       
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Si llega al destino, busca otro nuevo
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            GetNewPosition();
        }
    }

    void GetNewPosition()
    {
       
        float x = Random.Range(-areaSize.x, areaSize.x);

       
        float z = Random.Range(minZ, areaSize.z);

       
        float y = Random.Range(minHeight, areaSize.y);

        targetPos = new Vector3(x, y, z);
    }
}