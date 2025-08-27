using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SimpleGame simpleGame;

    public void OnReceiveAuthToken(string token)
    {
        Debug.Log("Configurando token");
        if (simpleGame != null)
        {
            simpleGame.authToken = token;
            Debug.Log("Token recebido: " + token);
        }
    }

    public void OnReceiveParams(string jsonParams)
    {
        Debug.Log("Parâmetros recebidos: " + jsonParams);

        if (simpleGame != null)
        {
            try
            {
                var data = JsonUtility.FromJson<ParamsData>(jsonParams);
                simpleGame.gameId = data.gameId;
                Debug.Log("GameID: " + simpleGame.gameId);
            }
            catch
            {
                Debug.LogWarning("Falha ao parsear params JSON.");
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
