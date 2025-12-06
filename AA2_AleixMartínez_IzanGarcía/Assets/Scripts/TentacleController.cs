using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform target;
    public Transform endEffector;
    public Transform[] joints;

    [Header("El Otro Brazo (Anti-Choque)")]
    public Transform otherArmEndEffector;
    public float repulsionRange = 2.0f;
    public float repulsionForce = 10.0f; // Aumentado para que reaccionen más rápido al choque

    [Header("Parámetros del Gradiente")]
    // CAMBIO 1: He subido esto de 50 a 150 para que el movimiento sea mucho más ágil
    public float learningRate = 150.0f;

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

        // CAMBIO 2: Aumentado de 10 a 25 iteraciones por frame.
        // Esto hace que el algoritmo "piense" más rápido cada segundo.
        for (int k = 0; k < 25; k++)
        {
            float error = CalculateCostFunction();

            if (error > stopThreshold)
            {
                ApplyGradientDescent();
            }
        }
    }

    float CalculateCostFunction()
    {
        // 1. Queremos tocar a Spider-Man
        float distanceToTarget = Vector3.Distance(endEffector.position, target.position);

        // 2. No queremos tocar al otro brazo
        float repulsionCost = 0;

        if (otherArmEndEffector != null)
        {
            float distanceToOtherArm = Vector3.Distance(endEffector.position, otherArmEndEffector.position);

            if (distanceToOtherArm < repulsionRange)
            {
                // Penalización exponencial para que reaccione brusco si se acerca mucho
                repulsionCost = (repulsionRange - distanceToOtherArm) * repulsionForce;
            }
        }

        return distanceToTarget + repulsionCost;
    }

    void ApplyGradientDescent()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            // Calculamos gradientes
            float gradientX = CalculateGradient(i, 'x');
            float gradientY = CalculateGradient(i, 'y');
            float gradientZ = CalculateGradient(i, 'z');

            // Actualizamos ángulos (theta_new = theta_old - alpha * gradient)
            // Al ser learningRate más alto, este cambio es mayor -> más velocidad
            anglesX[i] -= learningRate * gradientX * Time.deltaTime;
            anglesY[i] -= learningRate * gradientY * Time.deltaTime;
            anglesZ[i] -= learningRate * gradientZ * Time.deltaTime;

            // Aplicamos rotación
            joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
        }
    }

    float CalculateGradient(int i, char axis)
    {
        float error1 = CalculateCostFunction();

        if (axis == 'x') joints[i].localRotation = Quaternion.Euler(anglesX[i] + samplingDistance, anglesY[i], anglesZ[i]);
        if (axis == 'y') joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i] + samplingDistance, anglesZ[i]);
        if (axis == 'z') joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i] + samplingDistance);

        float error2 = CalculateCostFunction();

        float gradient = (error2 - error1) / samplingDistance;

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