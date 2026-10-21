using UnityEngine;

/// <summary>
/// Cámara cinematográfica que sigue dinámicamente las bolas de la máquina de Rube Goldberg.
/// Transiciona automáticamente de una bola a la siguiente a medida que se activan.
/// </summary>
public class CameraFollowMachine : MonoBehaviour
{
    [Header("Objetivos a seguir en orden")]
    [Tooltip("Arrastra aquí Ball_1, Ball_2 y Ball_3. Si lo dejas vacío, las buscará por nombre automáticamente.")]
    public Transform[] targets;

    [Header("Ajustes de Cámara")]
    [Tooltip("Distancia relativa respecto a la bola (X, Y, Z). X negativo la coloca atrás/izquierda en diagonal.")]
    public Vector3 offset = new Vector3(-5.0f, 4.0f, -9.5f);

    [Tooltip("Anticipación: hacia dónde mira la cámara por delante de la bola para enfocar la rampa que viene.")]
    public Vector3 lookAhead = new Vector3(3.5f, -0.5f, 0f);

    [Tooltip("Suavidad de movimiento (valores más bajos son más suaves y fluidos).")]
    public float smoothTime = 0.35f;

    [Tooltip("Qué tan rápido rota la cámara para mirar a la bola.")]
    public float lookAtSmoothSpeed = 4f;

    private int currentTargetIndex = 0;
    private Vector3 currentVelocity;
    private Vector3[] initialPositions;

    private void Start()
    {
        // Si no se asignaron en el inspector, buscarlas automáticamente por su nombre
        if (targets == null || targets.Length == 0)
        {
            var b1 = GameObject.Find("Ball_1_Starter") ?? GameObject.Find("StarterBall");
            var b2 = GameObject.Find("Ball_2_Secondary");
            var b3 = GameObject.Find("Ball_3_Final");

            var found = new System.Collections.Generic.List<Transform>();
            if (b1 != null) found.Add(b1.transform);
            if (b2 != null) found.Add(b2.transform);
            if (b3 != null) found.Add(b3.transform);

            targets = found.ToArray();
        }

        // Guardar posiciones iniciales para saber cuándo empieza a rodar cada bola
        if (targets != null && targets.Length > 0)
        {
            initialPositions = new Vector3[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    initialPositions[i] = targets[i].position;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (targets == null || targets.Length == 0) return;

        // Comprobar si la siguiente bola ya comenzó a moverse para cambiar de objetivo
        CheckNextTargetTrigger();

        var currentTarget = targets[currentTargetIndex];
        if (currentTarget == null) return;

        // 1. Posición suave de la cámara (SmoothDamp)
        Vector3 targetPosition = currentTarget.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // 2. Rotación suave mirando hacia lo que sigue de la rampa (Punto de anticipación)
        Vector3 focalPoint = currentTarget.position + lookAhead;
        Vector3 lookDirection = focalPoint - transform.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSmoothSpeed * Time.deltaTime);
        }
    }

    private void CheckNextTargetTrigger()
    {
        // Si aún quedan bolas por seguir
        if (currentTargetIndex + 1 < targets.Length)
        {
            int nextIndex = currentTargetIndex + 1;
            Transform nextTarget = targets[nextIndex];

            if (nextTarget != null && initialPositions != null && nextIndex < initialPositions.Length)
            {
                // Si la siguiente bola se movió más de 30 cm de su posición inicial, cambiamos de objetivo
                float distanceFromStart = Vector3.Distance(nextTarget.position, initialPositions[nextIndex]);
                if (distanceFromStart > 0.35f)
                {
                    currentTargetIndex = nextIndex;
                }
            }
        }
    }
}
