using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;

public class SimpleGame : MonoBehaviour
{
    public Transform goal;
    public float speed = 5f;
    public GameObject restartButton3D;
    public string backendUrl = "https://adapt2learn-895112363610.us-central1.run.app/api/events/game";

    [HideInInspector] public string gameId;
    [HideInInspector] public string authToken;

    private bool finished = false;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        if (restartButton3D != null)
            restartButton3D.SetActive(false);
    }

    void Update()
    {
        if (finished)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, goal.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, goal.position) < 0.1f)
        {
            finished = true;
            float elapsed = Time.time - startTime;
            Debug.Log($"Chegou no objetivo! Tempo: {elapsed:F2}");

            FindObjectOfType<TextMeshTimer>()?.StopTimer();

            if (restartButton3D != null)
                restartButton3D.SetActive(true);

            StartCoroutine(SendGameEvent(elapsed));
        }
    }

    IEnumerator SendGameEvent(float elapsedTime)
    {
        string elapsedStr = elapsedTime.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
        string json = $"{{\"event_type\":\"reach_goal\",\"payload\":{{\"time\":{elapsedStr}}},\"game_id\":\"{gameId}\"}}";

        Debug.Log(json);
        Debug.Log(authToken);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(backendUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + authToken);

            yield return req.SendWebRequest();

            Debug.Log(req.result);
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Erro ao enviar evento: {req.error}. Resp: {req.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Evento enviado com sucesso: {req.downloadHandler.text}");
            }
        }
    }
}
