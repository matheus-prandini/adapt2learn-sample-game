using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


[Serializable]
public class ExampleItemDTO
{
    public string document_id;
    public string question;
    public string math_reasoning;
    public string math_solution;
    public List<string> alternatives;
    public string scratch_reasoning;
    public string scratch_solution;
}


public class ExampleManager : MonoBehaviour
{
    public string baseUrl = "https://adapt2learn-895112363610.us-central1.run.app";

    public void StartFetchingExamples()
    {
        Debug.Log("[ExampleManager] Fetching Examples");
        StartCoroutine(FetchExamples(
            GameManager.Instance.SchoolId,
            GameManager.Instance.GameId,
            GameManager.Instance.Discipline,
            GameManager.Instance.Subarea,
            10,
            null,
            HandleExamples));
    }

    public IEnumerator FetchExamples(
        string schoolId, string gameId, string discipline, string subarea,
        int limit = 15, string filename = null,
        Action<List<ExampleItemDTO>> onComplete = null)
    {
        var token = GameManager.Instance.AuthToken;

        var qs = new List<string>
        {
            "school_id=" + UnityWebRequest.EscapeURL(schoolId),
            "game_id=" + UnityWebRequest.EscapeURL(gameId),
            "discipline=" + UnityWebRequest.EscapeURL(discipline),
            "subarea=" + UnityWebRequest.EscapeURL(subarea),
            "limit=" + limit
        };
        if (!string.IsNullOrEmpty(filename))
            qs.Add("filename=" + UnityWebRequest.EscapeURL(filename));

        string url = $"{baseUrl}/api/examples?{string.Join("&", qs)}";
        Debug.Log($"[ExampleManager] GET {url}");

        using var www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + token);
        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
#else
        if (www.isNetworkError || www.isHttpError)
#endif
        {
            Debug.LogError($"[ExampleManager] Erro ao carregar exemplos: {www.error}\n{www.downloadHandler.text}");
            yield break;
        }

        var items = JsonHelper.FromJson<ExampleItemDTO>(www.downloadHandler.text);
        var list = new List<ExampleItemDTO>(items);
        Debug.Log($"[ExampleManager] Recebidos {list.Count} exemplos.");

        onComplete?.Invoke(list);
    }

    private void HandleExamples(List<ExampleItemDTO> examples)
    {
        Debug.Log($"Exemplos carregados: {examples.Count}");
        if (examples.Count > 0)
        {
            var sample = examples[UnityEngine.Random.Range(0, examples.Count)];
            Debug.Log($"Exemplo amostrado:\nPergunta: {sample.question}");
        }
    }
}
