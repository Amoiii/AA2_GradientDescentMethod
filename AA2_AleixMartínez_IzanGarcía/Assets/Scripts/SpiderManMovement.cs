using UnityEngine;

public class SpiderManMovement : MonoBehaviour
{
    public float speed = 5.0f;

    // Define los límites máximos (X y Z) y la altura máxima (Y)
    public Vector3 areaSize = new Vector3(5, 4, 5);

    public float minHeight = 0.5f;

    // NUEVO: Límite mínimo para Z (para que no baje de 2)
    public float minZ = 2.0f;

    private Vector3 targetPos;

    void Start()
    {
        GetNewPosition();
    }

    void Update()
    {
        // Moverse hacia el punto destino
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Si llega al destino, busca otro nuevo
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            GetNewPosition();
        }
    }

    void GetNewPosition()
    {
        // X sigue siendo aleatorio total (-5 a 5)
        float x = Random.Range(-areaSize.x, areaSize.x);

        // Z ahora está restringido: De 2.0 a areaSize.z (ej. de 2 a 5)
        // Esto cumple tu requisito de "no llegar a menos de Z 2"
        float z = Random.Range(minZ, areaSize.z);

        // Y sigue siendo aleatorio entre la altura mínima y la máxima
        float y = Random.Range(minHeight, areaSize.y);

        targetPos = new Vector3(x, y, z);
    }
}