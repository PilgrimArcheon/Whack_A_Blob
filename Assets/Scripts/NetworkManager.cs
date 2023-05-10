using System.Collections;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

[Serializable]
public class SeasonData
{
    public int season;
}

[Serializable]
public class healthcheck
{
    public int season;
}

[Serializable]
public class PlayerPoints
{
    public int score;
    public int season;
    public int player;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public string serverUrl = "https://whack-a-blob.herokuapp.com/";
    public string userAddress, userSignature, message;
    public string accessToken;
    public Text info;
    public Text healthText;
    public int livesLeft;
    public bool canPlay;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        StartCoroutine(Login());
    }
    public IEnumerator Login()
    {
        info.text = "Started Logging In....";
        Debug.Log("Started Logging In....");
        WWWForm form = new WWWForm();
        form.AddField("address", userAddress);
        form.AddField("signature", userSignature);
        form.AddField("message", message);
        Debug.Log(userAddress + " " + message + " " + userSignature);
        UnityWebRequest www = UnityWebRequest.Post(serverUrl + "users/login", form);
        www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
        www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
        www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        www.timeout = 15;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error!!! " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Logged In...");
            info.text = "Logged In.";
            var loginDetails = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            accessToken = loginDetails["access_token"].ToString();
            Debug.Log("Logged In: " + www.downloadHandler.text);
            StartCoroutine(CheckHealth());
            MenuManager.Instance.OpenMenu("intro");
        }
    }

    public IEnumerator CheckHealth()
    {
        SeasonData seasonData = new SeasonData { season = 1 };
        string seasonDataJson = JsonUtility.ToJson(seasonData);
        Debug.Log(seasonDataJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/player-lives", "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(seasonDataJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
        www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
        www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _currentAttempts = _myData["current_attempts"].ToString();
            string _livesLeft = _myData["lives_left"].ToString();
            livesLeft = Int32.Parse(_livesLeft);
            // livesLeft = 1;
            healthText.text = livesLeft.ToString();
        }
    }

    public IEnumerator GetLeaderboardInfo(LeaderboardManager leaderboard)
    {
        SeasonData seasonData = new SeasonData { season = 1 };
        string seasonDataJson = JsonUtility.ToJson(seasonData);
        Debug.Log(seasonDataJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/scoreboard", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(seasonDataJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
        www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
        www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("LEADERBOARD: " + @www.downloadHandler.text);
            JArray boardData = JsonConvert.DeserializeObject<JArray>(@www.downloadHandler.text);
            int _count = boardData.Count;
            Debug.Log("SCOREBOARD INFO");
            for (int i = 0; i < _count; i++)
            {
                var _playerName = (string)boardData[i]["player"];
                var _score = (int)boardData[i]["score"];
                var _position = (int)boardData[i]["position"];
                Debug.Log(_playerName + ", Score: " + _score + ", Position: " + _position);
                AddToLeaderBoardInfo(leaderboard, _playerName, _score, _position);
            }

            leaderboard.UpdateLeaderBoard();
        }
    }

    public void AddToLeaderBoardInfo(LeaderboardManager leaderboard, string player, int score, int position)
    {
        PlayerData _data = new PlayerData();
        _data.player = player;
        _data.score = score;
        _data.position = position;
        leaderboard.scoreBoardData.Add(_data);
    }

    public IEnumerator GetPlayerInfo(LeaderboardManager leaderboard)
    {
        SeasonData seasonData = new SeasonData { season = 1 };
        string seasonDataJson = JsonUtility.ToJson(seasonData);
        Debug.Log(seasonDataJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/player-scoreboard", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(seasonDataJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
        www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
        www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _score = _myData["score"].ToString();
            string _position = _myData["position"].ToString();
            leaderboard.UpdatePlayerPersonalData(Int32.Parse(_score), Int32.Parse(_position));
        }
    }

    public void AddScore(int _score)
    {
        StartCoroutine(UpdatePlayerScore(_score));
    }

    public IEnumerator UpdatePlayerScore(int _newScore)
    {
        PlayerPoints playerScorePoints = new PlayerPoints { season = 1, score = _newScore };
        string scorePointJson = JsonUtility.ToJson(playerScorePoints);
        Debug.Log("Start Point Update" + scorePointJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/add-score", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(scorePointJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
        www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
        www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _score = _myData["score"].ToString();

            StartCoroutine(CheckHealth());
        }
    }


    public IEnumerator UpdateTaskScore(int _newScore)
    {
        PlayerPoints playerScorePoints = new PlayerPoints { season = 1, score = _newScore };
        string scorePointJson = JsonUtility.ToJson(playerScorePoints);
        Debug.Log("Task Point Update" + scorePointJson);

        UnityWebRequest www = new UnityWebRequest(serverUrl + "whack-a-blob/add-points-alone", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(scorePointJson);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
        www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
        www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        www.SetRequestHeader("Access-Control-Allow-Origin", "*");
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Success: " + www.downloadHandler.text);
            var _myData = MiniJSON.Json.Deserialize(www.downloadHandler.text) as IDictionary;
            string _score = _myData["score"].ToString();
        }
    }

    public void OpenTaskLink(string url)
    {
        Application.OpenURL(url);
        StartCoroutine(UpdateTaskScore(100));
    }
}
