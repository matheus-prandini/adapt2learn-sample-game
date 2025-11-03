using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ExampleManager exampleManager; // Gerencia exemplos
    public PuzzleManager puzzleManager;   // Gerencia puzzles de pesos

    private bool exampleHandled = false;

    public string AuthToken { get; private set; }
    public string SchoolId { get; private set; }
    public string Discipline { get; private set; }
    public string Subarea { get; private set; }
    public string GameId { get; private set; }
    public string SessionNumber { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        Debug.Log("GameManager Awake finalizado.");
    }

    /// <summary>
    /// Recebe token de autenticação e tenta iniciar managers
    /// </summary>
    public void OnReceiveAuthToken(string token)
    {
        Debug.Log("[GameManager] Configurando AuthToken");
        AuthToken = token;
        Debug.Log("[GameManager] Token recebido: " + AuthToken);

        TryStartManagers();
    }

    /// <summary>
    /// Recebe parâmetros do jogo e tenta iniciar managers
    /// </summary>
    public void OnReceiveParams(string jsonParams)
    {
        try
        {
            Debug.Log("[GameManager] Parâmetros recebidos: " + jsonParams);
            var data = JsonUtility.FromJson<ParamsData>(jsonParams);

            SchoolId = data.schoolId;
            Discipline = data.discipline;
            Subarea = data.subarea;
            GameId = data.gameId;
            SessionNumber = data.sessionNumber;

            Debug.Log($"[GameManager] SchoolId: {SchoolId}, Discipline: {Discipline}, Subarea: {Subarea}, GameId: {GameId}, SessionNumber: {SessionNumber}");

            TryStartManagers();
        }
        catch
        {
            Debug.LogWarning("[GameManager] Falha ao parsear params JSON.");
        }
    }

    /// <summary>
    /// Verifica se todos os parâmetros essenciais estão configurados e inicia managers
    /// </summary>
    private void TryStartManagers()
    {
        Debug.Log("[GameManager] Tentando iniciar managers...");
        if (!string.IsNullOrEmpty(AuthToken) &&
            !string.IsNullOrEmpty(SchoolId) &&
            !string.IsNullOrEmpty(GameId) &&
            !string.IsNullOrEmpty(Discipline) &&
            !string.IsNullOrEmpty(Subarea))
        {
            // Inicia ExampleManager
            if (exampleManager != null)
            {
                Debug.Log("[GameManager] Iniciando ExampleManager...");
                exampleManager.StartFetchingExamples();

                // Hook para receber a primeira pergunta carregada e disparar PuzzleManager
                exampleManager.OnExampleFetched += HandleExampleForPuzzle;
            }
            else
            {
                Debug.LogWarning("[GameManager] ExampleManager não encontrado na cena.");
            }
        }
    }

    /// <summary>
    /// Recebe um exemplo carregado e dispara o PuzzleManager para gerar puzzles
    /// </summary>
    private void HandleExampleForPuzzle(ExampleItemDTO example)
    {
        if (exampleHandled) return;
        exampleHandled = true;
        if (puzzleManager != null)
        {
            Debug.Log("[GameManager] Iniciando PuzzleManager para a pergunta carregada...");

            string questionText = example.question;
            string answerRaw = example.math_solution;

            // Tenta converter para float
            float answerFloat;
            if (!float.TryParse(answerRaw, out answerFloat))
            {
                Debug.LogError($"[GameManager] math_solution não é um número válido: {answerRaw}");
                return;
            }

            int minWeights = 3;
            int maxWeights = 6;

            puzzleManager.StartFetchingPuzzles(questionText, answerFloat, minWeights, maxWeights);
        }
        else
        {
            Debug.LogWarning("[GameManager] PuzzleManager não encontrado na cena.");
        }
    }

    /// <summary>
    /// Desregistra eventos ao destruir o GameManager
    /// </summary>
    void OnDestroy()
    {
        if (exampleManager != null)
            exampleManager.OnExampleFetched -= HandleExampleForPuzzle;
    }

    [Serializable]
    private class ParamsData
    {
        public string discipline;
        public string subarea;
        public string schoolId;
        public string gameId;
        public string sessionNumber;
    }
}
