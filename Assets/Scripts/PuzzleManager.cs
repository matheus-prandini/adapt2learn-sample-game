using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

// DTOs para mapear a resposta da API
[Serializable]
public class WeightPuzzleDTO
{
    public int num_weights;
    public List<float> weights;
    public List<int> solution_indices;
}

[Serializable]
public class PuzzleResponseDTO
{
    public string question;
    public float answer;
    public List<WeightPuzzleDTO> puzzles;
}

public class PuzzleManager : MonoBehaviour
{
    public string baseUrl = "https://adapt2learn-895112363610.us-central1.run.app";

    /// <summary>
    /// Inicia a requisição para gerar puzzles de pesos
    /// </summary>
    public void StartFetchingPuzzles(string question, float answer, int minWeights = 3, int maxWeights = 6)
    {
        Debug.Log("[PuzzleManager] Fetching puzzles...");
        StartCoroutine(FetchPuzzles(question, answer, minWeights, maxWeights, HandlePuzzles));
    }

    private IEnumerator FetchPuzzles(string question, float answer, int minWeights, int maxWeights, Action<PuzzleResponseDTO> onComplete)
    {
        string token = GameManager.Instance.AuthToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("[PuzzleManager] AuthToken não definido!");
            yield break;
        }

        // Monta JSON manualmente igual ao SimpleGame.cs
        string jsonData = $"{{\"question\":\"{question}\",\"answer\":{answer},\"min_weights\":{minWeights},\"max_weights\":{maxWeights}}}";
        Debug.Log($"[PuzzleManager] Payload JSON: {jsonData}");

        string url = $"{baseUrl}/api/puzzles";
        using var www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
#else
        if (www.isNetworkError || www.isHttpError)
#endif
        {
            Debug.LogError($"[PuzzleManager] Erro: {www.error}\n{www.downloadHandler.text}");
            yield break;
        }

        // Converte JSON para DTO
        try
        {
            PuzzleResponseDTO response = JsonUtility.FromJson<PuzzleResponseDTO>(www.downloadHandler.text);
            Debug.Log($"[PuzzleManager] Pergunta: {response.question}, Resposta: {response.answer}");
            onComplete?.Invoke(response);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PuzzleManager] Falha ao desserializar JSON: {ex.Message}\n{www.downloadHandler.text}");
        }
    }

    /// <summary>
    /// Callback padrão para exibir resultados no console
    /// </summary>
    private void HandlePuzzles(PuzzleResponseDTO response)
    {
        if (response.puzzles == null)
        {
            Debug.LogWarning("[PuzzleManager] Nenhum puzzle retornado.");
            return;
        }

        Debug.Log($"[PuzzleManager] Recebidos {response.puzzles.Count} puzzles para a pergunta: {response.question}");

        for (int i = 0; i < response.puzzles.Count; i++)
        {
            var puzzle = response.puzzles[i];
            Debug.Log($"Puzzle {i + 1} - num_weights: {puzzle.num_weights}");
            Debug.Log("Weights: " + string.Join(", ", puzzle.weights));
            Debug.Log("Solution indices: " + string.Join(", ", puzzle.solution_indices));
        }
    }
}
