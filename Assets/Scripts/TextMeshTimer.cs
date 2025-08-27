using UnityEngine;

public class TextMeshTimer : MonoBehaviour
{
    private TextMesh textMesh;
    private float startTime;
    private bool running = true;

    void Start()
    {
        textMesh = GetComponent<TextMesh>();
        startTime = Time.time;
    }

    void Update()
    {
        if (!running || textMesh == null) return;
        float elapsed = Time.time - startTime;
        textMesh.text = $"Tempo: {elapsed:F2}s";
    }

    public void StopTimer()
    {
        running = false;
    }

    public void RestartTimer()  // Opcional
    {
        startTime = Time.time;
        running = true;
    }
}
