using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Objetivos Principales")]
    public Transform target;
    public Transform endEffector;
    public Transform[] joints;

    [Header("Configuración de Zonas")]
    public Transform bodyReference;    
    public bool isRightArm;            // ¿Es el brazo derecho?

   
    public float returnSpeed = 2.0f;

    [Header("Anti-Choque (Solo cuando ataca)")]
    public Transform otherArmEndEffector;
    public float repulsionRange = 2.0f;
    public float repulsionForce = 10.0f;

    [Header("Anti-Atravesar SpiderMan")]
    public float targetBodyRadius = 0.8f;
    public float targetRepulsionForce = 20.0f;

    [Header("Parámetros del Gradiente")]
    public float learningRate = 150.0f;
    public float samplingDistance = 0.01f;
    public float stopThreshold = 0.1f;

    // Variables de estado
    private float[] anglesX, anglesY, anglesZ;

    // MEMORIA: Aquí guardamos cómo estaba el brazo al principio
    private float[] initialAnglesX, initialAnglesY, initialAnglesZ;

    void Start()
    {
        int count = joints.Length;

        // Inicializar arrays de trabajo
        anglesX = new float[count];
        anglesY = new float[count];
        anglesZ = new float[count];

        // Inicializar arrays de memoria (Postura Inicial)
        initialAnglesX = new float[count];
        initialAnglesY = new float[count];
        initialAnglesZ = new float[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 currentRot = joints[i].localEulerAngles;

            // Guardamos la rotación actual para trabajar
            anglesX[i] = currentRot.x;
            anglesY[i] = currentRot.y;
            anglesZ[i] = currentRot.z;

            // Guardamos la rotación inicial como "Copia de Seguridad"
            initialAnglesX[i] = currentRot.x;
            initialAnglesY[i] = currentRot.y;
            initialAnglesZ[i] = currentRot.z;
        }
    }

    void Update()
    {
        if (target == null || endEffector == null || bodyReference == null) return;

        // ZONAS
        Vector3 localTargetPos = bodyReference.InverseTransformPoint(target.position);
        bool targetInMyZone = false;

        if (isRightArm)
        {
            if (localTargetPos.x > 0) targetInMyZone = true; // Derecha
        }
        else
        {
            if (localTargetPos.x < 0) targetInMyZone = true; // Izquierda
        }

        // ATACAR O DESCANSAR
        if (targetInMyZone)
        {
            // MODO ATAQUE: Usamos Gradient Descent (IK)
            for (int k = 0; k < 25; k++)
            {
                float error = CalculateCostFunction();
                if (error > stopThreshold)
                {
                    ApplyGradientDescent();
                }
            }
        }
        else
        {
            //Volver suavemente a la posición inicial
            ReturnToStartPose();
        }
    }

    // postura original
    void ReturnToStartPose()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            // Usamos LerpAngle para interpolar suavemente desde el ángulo actual al inicial
            // LerpAngle maneja automáticamente el salto de 360 a 0 grados.
            anglesX[i] = Mathf.LerpAngle(anglesX[i], initialAnglesX[i], returnSpeed * Time.deltaTime);
            anglesY[i] = Mathf.LerpAngle(anglesY[i], initialAnglesY[i], returnSpeed * Time.deltaTime);
            anglesZ[i] = Mathf.LerpAngle(anglesZ[i], initialAnglesZ[i], returnSpeed * Time.deltaTime);

            // Aplicamos la rotación
            joints[i].localRotation = Quaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
        }
    }

    float CalculateCostFunction()
    {
        // 1. Distancia al objetivo
        float distanceToTarget = Vector3.Distance(endEffector.position, target.position);

        // 2. Repulsión entre brazos
        float armRepulsionCost = 0;
        if (otherArmEndEffector != null)
        {
            float distanceToOtherArm = Vector3.Distance(endEffector.position, otherArmEndEffector.position);
            if (distanceToOtherArm < repulsionRange)
            {
                armRepulsionCost = (repulsionRange - distanceToOtherArm) * repulsionForce;
            }
        }

        // 3. Repulsión de SpiderMan (No atravesar)
        float bodyCollisionCost = 0;
        for (int i = 0; i < joints.Length - 1; i++)
        {
            float distToSpidey = Vector3.Distance(joints[i].position, target.position);
            if (distToSpidey < targetBodyRadius)
            {
                bodyCollisionCost += (targetBodyRadius - distToSpidey) * targetRepulsionForce;
            }
        }

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
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, targetBodyRadius);
        }
    }
}