using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public ExampleManager exampleManager;

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

        Debug.Log("Finalizando Awake");
    }

    public void OnReceiveAuthToken(string token)
    {
        Debug.Log("Configurando token");
        AuthToken = token;
        Debug.Log("Token recebido: " + AuthToken);

        TryStartExamples();
    }

    public void OnReceiveParams(string jsonParams)
    {
        try
        {
            Debug.Log("Parâmetros recebidos: " + jsonParams);
            var data = JsonUtility.FromJson<ParamsData>(jsonParams);

            SchoolId = data.schoolId;
            Discipline = data.discipline;
            Subarea = data.subarea;
            GameId = data.gameId;
            SessionNumber = data.sessionNumber;

            Debug.Log($"SchoolId: {SchoolId}, Discipline: {Discipline}, Subarea: {Subarea}, GameId: {GameId}, SessionNumber: {SessionNumber}");

            TryStartExamples();
        }
        catch
        {
            Debug.LogWarning("Falha ao parsear params JSON.");
        }
    }

    private void TryStartExamples()
    {
        Debug.Log("Try Starting Examples");
        if (!string.IsNullOrEmpty(AuthToken) &&
            !string.IsNullOrEmpty(SchoolId) &&
            !string.IsNullOrEmpty(GameId) &&
            !string.IsNullOrEmpty(Discipline) &&
            !string.IsNullOrEmpty(Subarea))
        {
            if (exampleManager != null)
            {
                Debug.Log("Iniciando ExampleManager (já tem AuthToken e Params).");
                exampleManager.StartFetchingExamples();
            }
            else
            {
                Debug.LogWarning("ExampleManager não encontrado na cena.");
            }
        }
    }

    [System.Serializable]
    private class ParamsData
    {
        public string discipline;
        public string subarea;
        public string schoolId;
        public string gameId;
        public string sessionNumber;
    }
}
