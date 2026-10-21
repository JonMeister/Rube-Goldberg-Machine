using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class PhysicsBootstrap
{
    private const string RootName = "--- RUBE GOLDBERG MACHINE ---";

    // Desactivado: no ejecutar automáticamente al iniciar
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnRuntimeLoad()
    {
    }

#if UNITY_EDITOR
    [MenuItem("Rube Goldberg/Eliminar Warnings de Scripts Faltantes")]
    public static void CleanMissingScripts()
    {
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        int total = 0;
        foreach (var go in allObjects)
        {
            total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"✅ Se limpiaron {total} componentes faltantes de la escena.");
    }
#endif

    public static GameObject BuildFullMachine()
    {
        var root = new GameObject(RootName);

        // Materiales y shaders
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        
        var matFloor = CreateMaterial(shader, new Color(0.18f, 0.20f, 0.24f));
        var matPlatform = CreateMaterial(shader, new Color(0.40f, 0.26f, 0.16f)); // Madera
        var matRamp = CreateMaterial(shader, new Color(0.65f, 0.42f, 0.22f));
        var matBall1 = CreateMaterial(shader, new Color(0.85f, 0.15f, 0.15f)); // Rojo
        var matBall2 = CreateMaterial(shader, new Color(0.15f, 0.55f, 0.95f)); // Azul
        var matBall3 = CreateMaterial(shader, new Color(0.95f, 0.80f, 0.10f)); // Dorado
        var matDomino1 = CreateMaterial(shader, new Color(0.10f, 0.75f, 0.65f)); // Verde azulado
        var matDomino2 = CreateMaterial(shader, new Color(0.95f, 0.45f, 0.15f)); // Naranja
        var matSeesaw = CreateMaterial(shader, new Color(0.55f, 0.35f, 0.20f));
        var matHammer = CreateMaterial(shader, new Color(0.35f, 0.35f, 0.38f)); // Metal
        var matTarget = CreateMaterial(shader, new Color(0.85f, 0.20f, 0.75f)); // Magenta

        // Material de física (baja fricción para esferas y rampas)
        var smoothPhysMat = new PhysicsMaterial("SmoothPhysics")
        {
            dynamicFriction = 0.05f,
            staticFriction = 0.05f,
            bounciness = 0.25f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Average
        };

        var dominoPhysMat = new PhysicsMaterial("DominoPhysics")
        {
            dynamicFriction = 0.45f,
            staticFriction = 0.50f,
            bounciness = 0.10f
        };

        // Contador de colisiones HUD y consola
        var tracker = root.AddComponent<GoldbergCollisionTracker>();

        // -------------------------------------------------------------
        // 0. PISO GENERAL
        // -------------------------------------------------------------
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(3f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(45f, 1f, 16f);
        SetMaterial(floor, matFloor);

        // -------------------------------------------------------------
        // 1. PLATAFORMA 1 (Superior) + RAMPA 1 + BOLA 1
        // -------------------------------------------------------------
        var plat1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plat1.name = "Platform_1_High";
        plat1.transform.SetParent(root.transform);
        plat1.transform.position = new Vector3(-8.5f, 9.5f, 0f);
        plat1.transform.localScale = new Vector3(11f, 0.6f, 3f);
        SetMaterial(plat1, matPlatform);

        // Rampa 1
        var ramp1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp1.name = "Ramp_1";
        ramp1.transform.SetParent(root.transform);
        ramp1.transform.position = new Vector3(-16f, 12f, 0f);
        ramp1.transform.localScale = new Vector3(7f, 0.4f, 2.4f);
        ramp1.transform.rotation = Quaternion.Euler(0f, 0f, -24f);
        SetMaterial(ramp1, matRamp);
        ramp1.GetComponent<Collider>().material = smoothPhysMat;
        tracker.Register(ramp1, "Rampa Inicial");

        // Rieles laterales para la Rampa 1 (evitan que la bola caiga por los lados)
        CreateRail(root.transform, new Vector3(-16f, 12.3f, 1.25f), new Vector3(7f, 0.5f, 0.15f), -24f, matRamp);
        CreateRail(root.transform, new Vector3(-16f, 12.3f, -1.25f), new Vector3(7f, 0.5f, 0.15f), -24f, matRamp);

        // Bola 1 (StarterBall)
        var ball1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball1.name = "Ball_1_Starter";
        ball1.transform.SetParent(root.transform);
        ball1.transform.position = new Vector3(-18.5f, 13.8f, 0f);
        ball1.transform.localScale = Vector3.one * 1.2f;
        SetMaterial(ball1, matBall1);
        ball1.GetComponent<Collider>().material = smoothPhysMat;
        var rbBall1 = ball1.AddComponent<Rigidbody>();
        rbBall1.mass = 2.0f;
        rbBall1.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        tracker.Register(ball1, "Bola 1 (Starter)");

        // Fichas de dominó Lote 1 (6 fichas sobre la Plataforma 1)
        var dominoGroup1 = new GameObject("Dominoes_Set_1").transform;
        dominoGroup1.SetParent(root.transform);
        for (int i = 0; i < 6; i++)
        {
            var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.name = $"Domino_1_{i + 1}";
            d.transform.SetParent(dominoGroup1);
            d.transform.position = new Vector3(-11.5f + (i * 1.35f), 10.65f, 0f);
            d.transform.localScale = new Vector3(0.25f, 1.7f, 1.0f);
            SetMaterial(d, matDomino1);
            d.GetComponent<Collider>().material = dominoPhysMat;
            var rb = d.AddComponent<Rigidbody>();
            rb.mass = 0.8f;
            rb.angularDamping = 0.05f;
            tracker.Register(d, $"Dominó 1-{i + 1}");
        }

        // -------------------------------------------------------------
        // 2. SUBE Y BAJA (SEESAW) + BOLA 2
        // -------------------------------------------------------------
        var seesawGroup = new GameObject("Seesaw_Assembly").transform;
        seesawGroup.SetParent(root.transform);

        // Pivote / Base fija del sube y baja
        var fulcrum = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fulcrum.name = "Seesaw_Fulcrum";
        fulcrum.transform.SetParent(seesawGroup);
        fulcrum.transform.position = new Vector3(-1.0f, 6.8f, 0f);
        fulcrum.transform.localScale = new Vector3(0.8f, 2.0f, 2.0f);
        SetMaterial(fulcrum, matPlatform);

        // Tabla del sube y baja
        var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plank.name = "Seesaw_Plank";
        plank.transform.SetParent(seesawGroup);
        plank.transform.position = new Vector3(-1.0f, 7.9f, 0f);
        plank.transform.localScale = new Vector3(6.5f, 0.25f, 1.8f);
        // Inclinación inicial: lado izquierdo arriba (+14°), lado derecho abajo
        plank.transform.rotation = Quaternion.Euler(0f, 0f, 14f);
        SetMaterial(plank, matSeesaw);
        plank.GetComponent<Collider>().material = smoothPhysMat;

        var plankRb = plank.AddComponent<Rigidbody>();
        plankRb.mass = 1.8f;

        var hinge = plank.AddComponent<HingeJoint>();
        hinge.axis = Vector3.forward; // Eje Z
        hinge.useLimits = true;
        var limits = hinge.limits;
        limits.min = -20f;
        limits.max = 20f;
        hinge.limits = limits;
        tracker.Register(plank, "Sube y Baja");

        // Tope inferior para que el sube y baja repose en su ángulo inicial
        var stopper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stopper.name = "Seesaw_Stopper";
        stopper.transform.SetParent(seesawGroup);
        stopper.transform.position = new Vector3(1.4f, 6.7f, 0f);
        stopper.transform.localScale = new Vector3(0.5f, 0.8f, 1.8f);
        SetMaterial(stopper, matPlatform);

        // Bola 2 (reposa sobre el lado derecho del sube y baja)
        var ball2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball2.name = "Ball_2_Secondary";
        ball2.transform.SetParent(root.transform);
        ball2.transform.position = new Vector3(0.6f, 7.7f, 0f);
        ball2.transform.localScale = Vector3.one * 1.0f;
        SetMaterial(ball2, matBall2);
        ball2.GetComponent<Collider>().material = smoothPhysMat;
        var rbBall2 = ball2.AddComponent<Rigidbody>();
        rbBall2.mass = 1.5f;
        rbBall2.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        tracker.Register(ball2, "Bola 2 (Secundaria)");

        // -------------------------------------------------------------
        // 3. PLATAFORMA 2 (Media) + RAMPA 2 + DOMINÓS LOTE 2
        // -------------------------------------------------------------
        var plat2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plat2.name = "Platform_2_Mid";
        plat2.transform.SetParent(root.transform);
        plat2.transform.position = new Vector3(8.5f, 4.5f, 0f);
        plat2.transform.localScale = new Vector3(12f, 0.6f, 2.6f);
        SetMaterial(plat2, matPlatform);

        // Pequeña rampa conectora entre Sube y Baja y Plataforma 2
        var ramp2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp2.name = "Ramp_2";
        ramp2.transform.SetParent(root.transform);
        ramp2.transform.position = new Vector3(3.4f, 5.7f, 0f);
        ramp2.transform.localScale = new Vector3(3.2f, 0.35f, 2.2f);
        ramp2.transform.rotation = Quaternion.Euler(0f, 0f, -22f);
        SetMaterial(ramp2, matRamp);
        ramp2.GetComponent<Collider>().material = smoothPhysMat;
        tracker.Register(ramp2, "Rampa 2");

        // Fichas de dominó Lote 2 (6 fichas sobre Plataforma 2)
        var dominoGroup2 = new GameObject("Dominoes_Set_2").transform;
        dominoGroup2.SetParent(root.transform);
        for (int i = 0; i < 6; i++)
        {
            var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.name = $"Domino_2_{i + 1}";
            d.transform.SetParent(dominoGroup2);
            d.transform.position = new Vector3(5.5f + (i * 1.35f), 5.65f, 0f);
            d.transform.localScale = new Vector3(0.25f, 1.7f, 1.0f);
            SetMaterial(d, matDomino2);
            d.GetComponent<Collider>().material = dominoPhysMat;
            var rb = d.AddComponent<Rigidbody>();
            rb.mass = 0.8f;
            rb.angularDamping = 0.05f;
            tracker.Register(d, $"Dominó 2-{i + 1}");
        }

        // -------------------------------------------------------------
        // 4. MARTILLO PÉNDULO (FIJO EN UN PUNTO PIVOTE)
        // -------------------------------------------------------------
        var hammerGroup = new GameObject("Hammer_Pendulum").transform;
        hammerGroup.SetParent(root.transform);

        // Viga soporte superior (Fija)
        var hammerBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hammerBeam.name = "Hammer_Beam";
        hammerBeam.transform.SetParent(hammerGroup);
        hammerBeam.transform.position = new Vector3(15.0f, 9.2f, 0f);
        hammerBeam.transform.localScale = new Vector3(1.2f, 0.5f, 2.4f);
        SetMaterial(hammerBeam, matPlatform);

        // Estructura móvil del Martillo (Brazo + Cabeza pesada)
        var hammerBody = new GameObject("Hammer_Body");
        hammerBody.transform.SetParent(hammerGroup);
        // Colocado en el punto pivote
        hammerBody.transform.position = new Vector3(15.0f, 9.0f, 0f);
        // Inclinación inicial: inclinado hacia atrás (-38°) listo para soltarse con gravedad
        hammerBody.transform.rotation = Quaternion.Euler(0f, 0f, -38f);

        // Mango
        var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Hammer_Handle";
        handle.transform.SetParent(hammerBody.transform);
        handle.transform.localPosition = new Vector3(0f, -2.0f, 0f);
        handle.transform.localScale = new Vector3(0.22f, 4.0f, 0.22f);
        handle.transform.localRotation = Quaternion.identity;
        SetMaterial(handle, matHammer);

        // Cabeza del martillo (pesada)
        var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Hammer_Head";
        head.transform.SetParent(hammerBody.transform);
        head.transform.localPosition = new Vector3(0f, -4.1f, 0f);
        head.transform.localScale = new Vector3(1.2f, 0.9f, 1.4f);
        head.transform.localRotation = Quaternion.identity;
        SetMaterial(head, matHammer);
        head.GetComponent<Collider>().material = smoothPhysMat;

        var hammerRb = hammerBody.AddComponent<Rigidbody>();
        hammerRb.mass = 4.5f;
        hammerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var hammerHinge = hammerBody.AddComponent<HingeJoint>();
        hammerHinge.axis = Vector3.forward;
        hammerHinge.anchor = Vector3.zero; // Pivota exactamente en (15, 9, 0)
        tracker.Register(head, "Cabeza del Martillo");

        // Soporte disparador: pequeño taco que retiene el martillo en -38°
        // El último dominó (Domino_2_6) choca contra este taco o el martillo directamente
        var hammerLatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hammerLatch.name = "Hammer_Latch";
        hammerLatch.transform.SetParent(hammerGroup);
        hammerLatch.transform.position = new Vector3(13.4f, 5.4f, 0f);
        hammerLatch.transform.localScale = new Vector3(0.3f, 1.1f, 0.8f);
        SetMaterial(hammerLatch, matDomino2);
        var latchRb = hammerLatch.AddComponent<Rigidbody>();
        latchRb.mass = 0.5f;
        tracker.Register(hammerLatch, "Gatillo del Martillo");

        // -------------------------------------------------------------
        // 5. BOLA 3 (FINAL) + RAMPA DE LLEGADA + OBJETIVOS FINALES
        // -------------------------------------------------------------
        // Pedestal donde reposa la Bola 3 esperando el golpe del martillo
        var ball3Stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ball3Stand.name = "Ball_3_Pedestal";
        ball3Stand.transform.SetParent(root.transform);
        ball3Stand.transform.position = new Vector3(16.2f, 4.5f, 0f);
        ball3Stand.transform.localScale = new Vector3(1.6f, 0.6f, 1.6f);
        SetMaterial(ball3Stand, matPlatform);

        // Bola 3 (Final)
        var ball3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball3.name = "Ball_3_Final";
        ball3.transform.SetParent(root.transform);
        ball3.transform.position = new Vector3(16.2f, 5.4f, 0f);
        ball3.transform.localScale = Vector3.one * 1.1f;
        SetMaterial(ball3, matBall3);
        ball3.GetComponent<Collider>().material = smoothPhysMat;
        var rbBall3 = ball3.AddComponent<Rigidbody>();
        rbBall3.mass = 1.2f;
        rbBall3.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        tracker.Register(ball3, "Bola 3 (Final)");

        // Rampa de descenso hacia el piso final
        var finalRamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finalRamp.name = "Final_Ramp";
        finalRamp.transform.SetParent(root.transform);
        finalRamp.transform.position = new Vector3(18.5f, 3.0f, 0f);
        finalRamp.transform.localScale = new Vector3(5.5f, 0.35f, 2.2f);
        finalRamp.transform.rotation = Quaternion.Euler(0f, 0f, -32f);
        SetMaterial(finalRamp, matRamp);
        finalRamp.GetComponent<Collider>().material = smoothPhysMat;
        tracker.Register(finalRamp, "Rampa Final");

        // Pirámide de bloques meta (3 en la base, 2 al medio, 1 arriba)
        var targetGroup = new GameObject("Final_Target_Blocks").transform;
        targetGroup.SetParent(root.transform);
        
        // Base (3)
        for (int i = 0; i < 3; i++)
        {
            CreateTargetBlock(targetGroup, new Vector3(22.0f + (i * 0.9f), 0.5f, 0f), matTarget, tracker, $"Meta Base {i + 1}");
        }
        // Medio (2)
        for (int i = 0; i < 2; i++)
        {
            CreateTargetBlock(targetGroup, new Vector3(22.45f + (i * 0.9f), 1.5f, 0f), matTarget, tracker, $"Meta Medio {i + 1}");
        }
        // Cima (1)
        CreateTargetBlock(targetGroup, new Vector3(22.9f, 2.5f, 0f), matTarget, tracker, "Meta Cima (Premio)");

        // -------------------------------------------------------------
        // CÁMARA Y LUZ
        // -------------------------------------------------------------
        SetupCamera();
        SetupLight();

        return root;
    }

    private static void CreateRail(Transform parent, Vector3 pos, Vector3 scale, float rotZ, Material mat)
    {
        var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "Rail";
        rail.transform.SetParent(parent);
        rail.transform.position = pos;
        rail.transform.localScale = scale;
        rail.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        SetMaterial(rail, mat);
    }

    private static void CreateTargetBlock(Transform parent, Vector3 pos, Material mat, GoldbergCollisionTracker tracker, string blockName)
    {
        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = blockName;
        b.transform.SetParent(parent);
        b.transform.position = pos;
        b.transform.localScale = new Vector3(0.8f, 0.95f, 0.8f);
        SetMaterial(b, mat);
        var rb = b.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        tracker.Register(b, blockName);
    }

    private static void SetupCamera()
    {
        var cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null)
        {
            var cameraObject = new GameObject("Main Camera");
            cam = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        // Posición panorámica óptima para encuadrar las 3 secciones
        cam.transform.position = new Vector3(3.5f, 9.5f, -25.5f);
        cam.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
        cam.fieldOfView = 52f;
    }

    private static void SetupLight()
    {
        var light = Object.FindAnyObjectByType<Light>();
        if (light == null)
        {
            var lightObject = new GameObject("Directional Light");
            light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
        }

        light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        light.intensity = 1.15f;
        light.color = new Color(1f, 0.96f, 0.9f);
    }

    private static Material CreateMaterial(Shader shader, Color color)
    {
        var mat = new Material(shader) { color = color };
        return mat;
    }

    private static void SetMaterial(GameObject obj, Material mat)
    {
        var renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null && mat != null)
        {
            renderer.material = mat;
        }
    }
}
