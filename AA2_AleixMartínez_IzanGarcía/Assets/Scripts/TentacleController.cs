using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform target;
    public Transform endEffector;
    public Transform[] joints;

    [Header("El Otro Brazo (Anti-Choque entre brazos)")]
    public Transform otherArmEndEffector;
    public float repulsionRange = 2.0f;
    public float repulsionForce = 10.0f;

    [Header("Anti-Atravesar SpiderMan (NUEVO)")]
    // Radio del cuerpo de SpiderMan (si es una esfera de escala 1, pon 0.6f o 0.7f)
    public float targetBodyRadius = 0.8f;
    // Fuerza con la que el cuerpo repele a los huesos del brazo
    public float targetRepulsionForce = 20.0f;

    [Header("Parámetros del Gradiente")]
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

        // Iteraciones por frame
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
        // 1. Coste Principal: La punta quiere llegar al centro
        float distanceToTarget = Vector3.Distance(endEffector.position, target.position);

        // 2. Repulsión entre brazos (lo que ya tenías)
        float armRepulsionCost = 0;
        if (otherArmEndEffector != null)
        {
            float distanceToOtherArm = Vector3.Distance(endEffector.position, otherArmEndEffector.position);
            if (distanceToOtherArm < repulsionRange)
            {
                armRepulsionCost = (repulsionRange - distanceToOtherArm) * repulsionForce;
            }
        }

        // 3. NUEVO: Repulsión del cuerpo de SpiderMan (Evitar atravesarlo)
        float bodyCollisionCost = 0;

        // Recorremos todas las articulaciones EXCEPTO la última (la punta sí debe tocarle)
        for (int i = 0; i < joints.Length - 1; i++)
        {
            float distToSpidey = Vector3.Distance(joints[i].position, target.position);

            // Si un hueso entra en el radio del cuerpo
            if (distToSpidey < targetBodyRadius)
            {
                // Añadimos un coste muy alto para que el algoritmo busque otra postura
                bodyCollisionCost += (targetBodyRadius - distToSpidey) * targetRepulsionForce;
            }
        }

        // La función de coste total suma todo
        return distanceToTarget + armRepulsionCost + bodyCollisionCost;
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
        // Visualizar el área de "no tocar" de SpiderMan
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, targetBodyRadius);
        }
    }
}