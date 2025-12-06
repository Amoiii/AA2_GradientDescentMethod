using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform target;
    public Transform endEffector;
    public Transform[] joints;

    [Header("El Otro Brazo (Para evitar choques)")]
    public Transform otherArmEndEffector; // Arrastra aquí la punta del OTRO brazo
    public float repulsionRange = 2.0f;   // A qué distancia empiezan a repelerse
    public float repulsionForce = 5.0f;   // Cuánto "odio" se tienen (fuerza de empuje)

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

        // Ejecutamos varias veces para suavidad
        for (int k = 0; k < 10; k++)
        {
            // Usamos nuestra nueva función de coste que incluye la repulsión
            float error = CalculateCostFunction();

            // Si el error es alto (estamos lejos O estamos chocando), corregimos
            if (error > stopThreshold)
            {
                ApplyGradientDescent();
            }
        }
    }

      float CalculateCostFunction()
    {
        // Coste principal: Querer tocar a Spider-Man
        float distanceToTarget = Vector3.Distance(endEffector.position, target.position);

        //  penalización para asi evitar que vaya 
        float repulsionCost = 0;

        if (otherArmEndEffector != null)
        {
            float distanceToOtherArm = Vector3.Distance(endEffector.position, otherArmEndEffector.position);

            // Si estamos demasiado cerca del otro brazo, aumentamos el error drásticamente
            if (distanceToOtherArm < repulsionRange)
            {
                // Cuanto más cerca, mayor es el coste
                // Usamos (Rango - Distancia) 
                repulsionCost = (repulsionRange - distanceToOtherArm) * repulsionForce;
            }
        }

        //error
        return distanceToTarget + repulsionCost;
    }

    void ApplyGradientDescent()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            float gradientX = CalculateGradient(i, 'x');
            float gradientY = CalculateGradient(i, 'y');
            float gradientZ = CalculateGradient(i, 'z');

            anglesX[i] -= learningRate * gradientX * Time.deltaTime;
            anglesY[i] -= learningRate * gradientY * Time.deltaTime;
            anglesZ[i] -= learningRate * gradientZ * Time.deltaTime;

            joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
        }
    }

    float CalculateGradient(int i, char axis)
    {
        // Paso 1: Error actual
        float error1 = CalculateCostFunction();

        // Paso 2: Mover virtualmente
        if (axis == 'x') joints[i].localRotation = Quaternion.Euler(anglesX[i] + samplingDistance, anglesY[i], anglesZ[i]);
        if (axis == 'y') joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i] + samplingDistance, anglesZ[i]);
        if (axis == 'z') joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i] + samplingDistance);

        // Paso 3: Nuevo error (incluye repulsión)
        float error2 = CalculateCostFunction();

        // Paso 4: Pendiente
        float gradient = (error2 - error1) / samplingDistance;

        // Paso 5: Restaurar
        joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);

        return gradient;
    }

   
    void OnDrawGizmos()
    {
        if (endEffector != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(endEffector.position, repulsionRange);
        }
    }
}