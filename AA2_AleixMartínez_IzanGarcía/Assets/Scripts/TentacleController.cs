using UnityEngine;

public class TentacleController : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform target;
    public Transform endEffector;
    public Transform[] joints;

    [Header("Zonas")]
    public Transform bodyReference;
    public bool isRightArm;
    public float returnSpeed = 2.0f;

    [Header("Anti-Choque")]
    public Transform otherArmEndEffector;
    public float repulsionRange = 2.0f;
    public float repulsionForce = 10.0f;

    [Header("Anti-Atravesar")]
    public float targetBodyRadius = 0.8f;
    public float targetRepulsionForce = 20.0f;

    [Header("Parámetros")]
    public float learningRate = 150.0f;
    public float samplingDistance = 0.01f;
    public float stopThreshold = 0.1f;

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
           
            Vector3 rot = joints[i].localEulerAngles;

            anglesX[i] = rot.x;
            anglesY[i] = rot.y;
            anglesZ[i] = rot.z;

            initialAnglesX[i] = rot.x;
            initialAnglesY[i] = rot.y;
            initialAnglesZ[i] = rot.z;
        }
    }

    void Update()
    {
        if (target == null || endEffector == null || bodyReference == null) return;

        // Convertimos a  vectores 
        MyVector3 myTargetPos = MyVector3.FromUnity(target.position);
        MyVector3 myBodyPos = MyVector3.FromUnity(bodyReference.position);

      

        MyVector3 toTarget = myTargetPos - myBodyPos;
        MyVector3 bodyRight = MyVector3.FromUnity(bodyReference.right);

        // Producto Punto : (x*x + y*y + z*z)
        float dotProduct = (toTarget.x * bodyRight.x) + (toTarget.y * bodyRight.y) + (toTarget.z * bodyRight.z);

        bool targetInMyZone = false;

        if (isRightArm)
        {
            if (dotProduct > 0) targetInMyZone = true;
        }
        else
        {
            if (dotProduct < 0) targetInMyZone = true;
        }

        if (targetInMyZone)
        {
            for (int k = 0; k < 25; k++)
            {
                // Usamos nuestra función de distancia 
                MyVector3 efPos = MyVector3.FromUnity(endEffector.position);
                MyVector3 tPos = MyVector3.FromUnity(target.position);

                float error = MyVector3.Distance(efPos, tPos);

                // Aquí calculamos el coste completo
                if (CalculateCostFunction() > stopThreshold)
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
            // Usamos LerpAngle
            anglesX[i] = MyMath.LerpAngle(anglesX[i], initialAnglesX[i], returnSpeed * Time.deltaTime);
            anglesY[i] = MyMath.LerpAngle(anglesY[i], initialAnglesY[i], returnSpeed * Time.deltaTime);
            anglesZ[i] = MyMath.LerpAngle(anglesZ[i], initialAnglesZ[i], returnSpeed * Time.deltaTime);

            // Convertimos a Cuaternión y luego lo pasamos a Unity
            MyQuaternion q = MyQuaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
            joints[i].localRotation = q.ToUnity();
        }
    }

    float CalculateCostFunction()
    {
        // Convertimos todo a vectores
        MyVector3 endPos = MyVector3.FromUnity(endEffector.position);
        MyVector3 targetPos = MyVector3.FromUnity(target.position);

        float distanceToTarget = MyVector3.Distance(endPos, targetPos);

        float armRepulsionCost = 0;
        if (otherArmEndEffector != null)
        {
            MyVector3 otherPos = MyVector3.FromUnity(otherArmEndEffector.position);
            float distanceToOtherArm = MyVector3.Distance(endPos, otherPos);

            if (distanceToOtherArm < repulsionRange)
            {
                armRepulsionCost = (repulsionRange - distanceToOtherArm) * repulsionForce;
            }
        }

        float bodyCollisionCost = 0;
        for (int i = 0; i < joints.Length - 1; i++)
        {
            MyVector3 jointPos = MyVector3.FromUnity(joints[i].position);
            float distToSpidey = MyVector3.Distance(jointPos, targetPos);

            if (distToSpidey < targetBodyRadius)
            {
                // Usamos NUESTRO Pow
                float penetration = targetBodyRadius - distToSpidey;
                bodyCollisionCost += MyMath.Pow(penetration, 2) * targetRepulsionForce;
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

            
            MyQuaternion q = MyQuaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
            joints[i].localRotation = q.ToUnity();
        }
    }

    float CalculateGradient(int i, char axis)
    {
        float error1 = CalculateCostFunction();

        // Guardamos ángulos originales
        float tempAngle = 0;
        if (axis == 'x') { tempAngle = anglesX[i]; anglesX[i] += samplingDistance; }
        else if (axis == 'y') { tempAngle = anglesY[i]; anglesY[i] += samplingDistance; }
        else if (axis == 'z') { tempAngle = anglesZ[i]; anglesZ[i] += samplingDistance; }

        // Aplicamos rotación de prueba con Cuaternión
        MyQuaternion q = MyQuaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
        joints[i].localRotation = q.ToUnity();

        float error2 = CalculateCostFunction();
        float gradient = (error2 - error1) / samplingDistance;

        // Restauramos
        if (axis == 'x') anglesX[i] = tempAngle;
        else if (axis == 'y') anglesY[i] = tempAngle;
        else if (axis == 'z') anglesZ[i] = tempAngle;

        MyQuaternion qReset = MyQuaternion.Euler(anglesX[i], anglesY[i], anglesZ[i]);
        joints[i].localRotation = qReset.ToUnity();

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