using UnityEngine;

public static class PhysicsBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildBasicMachine()
    {
        if (Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0)
        {
            return;
        }

        EnsureCamera();
        EnsureDirectionalLight();

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(30f, 1f, 30f);

        var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ramp";
        ramp.transform.position = new Vector3(-6f, 1f, 0f);
        ramp.transform.localScale = new Vector3(8f, 0.5f, 4f);
        ramp.transform.rotation = Quaternion.Euler(0f, 0f, -22f);

        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "StarterBall";
        ball.transform.position = new Vector3(-9f, 4f, 0f);
        var ballRb = ball.AddComponent<Rigidbody>();
        ballRb.mass = 1.5f;

        var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        target.name = "TargetBlock";
        target.transform.position = new Vector3(4f, 0.5f, 0f);
        target.transform.localScale = new Vector3(1f, 1f, 1f);
        var targetRb = target.AddComponent<Rigidbody>();
        targetRb.mass = 2f;

        var dominoParent = new GameObject("Dominoes").transform;
        for (var i = 0; i < 8; i++)
        {
            var domino = GameObject.CreatePrimitive(PrimitiveType.Cube);
            domino.name = $"Domino_{i + 1}";
            domino.transform.SetParent(dominoParent);
            domino.transform.position = new Vector3(6f + (i * 1.1f), 1f, 0f);
            domino.transform.localScale = new Vector3(0.25f, 2f, 1f);
            var rb = domino.AddComponent<Rigidbody>();
            rb.mass = 0.6f;
            rb.angularDamping = 0.05f;
        }
    }

    private static void EnsureCamera()
    {
        if (Object.FindFirstObjectByType<Camera>() != null)
        {
            return;
        }

        var cameraObject = new GameObject("Main Camera");
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 8f, -18f);
        camera.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
    }

    private static void EnsureDirectionalLight()
    {
        if (Object.FindFirstObjectByType<Light>() != null)
        {
            return;
        }

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
}
