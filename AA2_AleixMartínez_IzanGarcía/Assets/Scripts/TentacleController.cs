using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Objetivos Principales")]
    public Transform target;
    public Transform endEffector;
    public Transform[] joints;

    [Header("Configuración de Zonas")]
    public Transform bodyReference;
    public bool isRightArm;

    public float returnSpeed = 5.0f; 

    [Header("Anti-Choque (Solo cuando ataca)")]
    public Transform otherArmEndEffector;
    public float repulsionRange = 2.0f;
    public float repulsionForce = 50.0f; 

    [Header("Anti-Atravesar SpiderMan (CRÍTICO)")]
    public float targetBodyRadius = 1.0f; 
    public float targetRepulsionForce = 500.0f; 

    [Header("Parámetros del Gradiente")]
    public float learningRate = 300.0f; 
    public float samplingDistance = 0.01f;
    public float stopThreshold = 0.1f;

    // Variables de estado
    private float[] anglesX, anglesY, anglesZ;

   
    private float[] initialAnglesX, initialAnglesY, initialAnglesZ;

    void Start()
    {
        int count = joints.Length;

        anglesX = new float[count];
        anglesY = new float[count];
        anglesZ = new float[count];

        initialAnglesX = new float[count];
        initialAnglesY = new float[count];
        initialAnglesZ = new float[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 currentRot = joints[i].localEulerAngles;

            anglesX[i] = currentRot.x;
            anglesY[i] = currentRot.y;
            anglesZ[i] = currentRot.z;

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
            if (localTargetPos.x > 0) targetInMyZone = true;
        }
        else
        {
            if (localTargetPos.x < 0) targetInMyZone = true;
        }

        // ATACAR O DESCANSAR
        if (targetInMyZone)
        {
            // MODO ATAQUE: Aumentamos iteraciones a 40 para reacción instantánea
            // Esto hace que el brazo sea "más listo" en cada frame
            for (int k = 0; k < 50; k++)
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
            ReturnToStartPose();
        }
    }

    void ReturnToStartPose()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            anglesX[i] = Mathf.LerpAngle(anglesX[i], initialAnglesX[i], returnSpeed * Time.deltaTime);
            anglesY[i] = Mathf.LerpAngle(anglesY[i], initialAnglesY[i], returnSpeed * Time.deltaTime);
            anglesZ[i] = Mathf.LerpAngle(anglesZ[i], initialAnglesZ[i], returnSpeed * Time.deltaTime);

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

        // 3. Repulsión de SpiderMan (MODIFICADO PARA NO ATRAVESAR)
        float bodyCollisionCost = 0;

        // Revisamos todos los huesos MENOS la punta
        for (int i = 0; i < joints.Length - 1; i++)
        {
            float distToSpidey = Vector3.Distance(joints[i].position, target.position);

            if (distToSpidey < targetBodyRadius)
            {
              
                // Elevamos la penetración al cuadrado (Mathf.Pow).
                
                // si entra se dispara exponencialmente.
             

                float penetrationDepth = targetBodyRadius - distToSpidey;
                bodyCollisionCost += Mathf.Pow(penetrationDepth, 2) * targetRepulsionForce;
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
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(target.position, targetBodyRadius);
        }
    }
}