using UnityEngine;

public class SpiderManMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public Vector3 areaSize = new Vector3(5, 4, 5); 
    public float minHeight = 0.5f;
    public float minZ = 2.0f;

    private MyVector3 targetPos; 

    void Start()
    {
        GetNewPosition();
    }

    void Update()
    {
        // Conversión necesaria para mover el Transform
        Vector3 currentPosUnity = transform.position;
        Vector3 targetPosUnity = targetPos.ToUnity();

        // MoveTowards manual
        Vector3 direction = targetPosUnity - currentPosUnity;
        float distance = direction.magnitude;

        if (distance <= speed * Time.deltaTime)
        {
            transform.position = targetPosUnity;
            GetNewPosition();
        }
        else
        {
            transform.position = currentPosUnity + direction.normalized * speed * Time.deltaTime;
        }
    }

    void GetNewPosition()
    {
       
       
        float x = Random.Range(-areaSize.x, areaSize.x);
        float z = Random.Range(minZ, areaSize.z);
        float y = Random.Range(minHeight, areaSize.y);

        targetPos = new MyVector3(x, y, z);
    }
}