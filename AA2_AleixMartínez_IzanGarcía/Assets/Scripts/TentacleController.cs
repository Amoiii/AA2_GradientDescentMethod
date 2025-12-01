using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Configuración")]
    public Transform target;          
    public Transform endEffector;      
    public Transform[] joints;         

    [Header("Parámetros del Gradiente")]
    public float learningRate = 50.0f;
    public float samplingDistance = 0.01f; 
    public float stopThreshold = 0.1f; 

   
    private float[] anglesX, anglesY, anglesZ;

    void Start()
    {
       
        int count = joints.Length;
        anglesX = new float[count];
        anglesY = new float[count];
        anglesZ = new float[count];

        // rotación inicial de cada articulación
        for (int i = 0; i < count; i++)
        {
            Vector3 currentRot = joints[i].localEulerAngles;
            anglesX[i] = currentRot.x;
            anglesY[i] = currentRot.y;
            anglesZ[i] = currentRot.z;
        }
    }

    void Update()
    {
        if (target == null || endEffector == null) return;

       
        for (int k = 0; k < 10; k++)
        {
            float distance = Vector3.Distance(endEffector.position, target.position);

            // Si no hemos llegado, aplicamos Gradient Descent
            if (distance > stopThreshold)
            {
                ApplyGradientDescent();
            }
        }
    }

    void ApplyGradientDescent()
    {
       
        for (int i = 0; i < joints.Length; i++)
        {
            // Calculamos el gradiente para cada eje (X, Y, Z) y actualizamos el ángulo
            // Formula: angulo_nuevo = angulo_viejo - alpha * gradiente

            float gradientX = CalculateGradient(i, 'x');
            anglesX[i] -= learningRate * gradientX * Time.deltaTime;

            float gradientY = CalculateGradient(i, 'y');
            anglesY[i] -= learningRate * gradientY * Time.deltaTime;

            float gradientZ = CalculateGradient(i, 'z');
            anglesZ[i] -= learningRate * gradientZ * Time.deltaTime;

           
            joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
        }
    }

    // Esta función calcula la pendiente numéricamente
    float CalculateGradient(int i, char axis)
    {
        // 1. Calcular distancia actual
        float distance1 = Vector3.Distance(endEffector.position, target.position);

        // 2. Mover un poquito el ángulo (Sampling)
        if (axis == 'x') joints[i].localRotation = Quaternion.Euler(anglesX[i] + samplingDistance, anglesY[i], anglesZ[i]);
        if (axis == 'y') joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i] + samplingDistance, anglesZ[i]);
        if (axis == 'z') joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i] + samplingDistance);

        // 3. Calcular la nueva distancia (Error nuevo)
        float distance2 = Vector3.Distance(endEffector.position, target.position);

        // 4. Calcular el gradiente (diferencia de error / cambio en ángulo)
        float gradient = (distance2 - distance1) / samplingDistance;

        // 5. Devolver la articulación a su sitio original (importante)
        joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);

        return gradient;
    }
}