using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;

public class Manager : MonoBehaviour
{
    private static Manager instance;
    public static Manager Instance
    {
        get
        {
            if(!instance)
            {
                instance = FindObjectOfType<Manager>();
            }

            return instance;
        }
    }

    bool trySpin;

    int idBet;
    int totalBet;

    [SerializeField] Text betText;
    [SerializeField] Text totalBetWin;
    [SerializeField] Text winText;

    [Space(10)]
    [SerializeField] GameObject statusAtoSpinGO;

    [Space(10)]
    [SerializeField] GameInfo gameInfo;
    [SerializeField] RollInfo rollInfo;

    private void Start()
    {
        StartCoroutine(GetGameInfo("https://disbark.ru/load?game_id=100&user_id=2", (_gameInfo) => 
        {
            gameInfo = _gameInfo;

            UpdateCoinsCount();
            ChangeBet(0);
        }));
    }

    private void Update()
    {
        statusAtoSpinGO.transform.Rotate(120.0f * Time.deltaTime * Vector3.back);
    }

    public void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    bool CanSpin()
    {
        return gameInfo.balance >= totalBet && totalBet > 0;
    }

    public void TrySpin()
    {
        if(trySpin)
        {
            return;
        }

        trySpin = true;

        StartCoroutine(GetRollInfo("https://disbark.ru/spin?game_id=100&user_id=2&bid=10", (_rollInfo) =>
        {
            rollInfo = _rollInfo;
            UpdateCoinsCount(rollInfo.win_amount);

            if (!CanSpin())
            {
                return;
            }

            gameInfo.balance -= totalBet;
            if (gameInfo.balance < 0)
            {
                gameInfo.balance = 0;
            }

            UpdateCoinsCount();
            SlotMachine.Instance.Pull(rollInfo.result);

            trySpin = false;
        }));
    }

    public void CalculatePrize()
    {
        
    }

    public void SetAutoSpin()
    {
        SlotMachine.autoSpin = !SlotMachine.autoSpin;
        statusAtoSpinGO.SetActive(SlotMachine.autoSpin);
    }

    public void UpdateCoinsCount(int amount = 0)
    {
        gameInfo.balance += amount;
        winText.text = gameInfo.balance.ToString("WIN: ##,# $", CultureInfo.CurrentCulture);
    }

    public void SetMaxBet()
    {
        idBet = gameInfo.bids.Length - 1;
        ChangeBet(0);
    }

    public void ChangeBet(int dir)
    {
        idBet += dir;
        if(idBet > gameInfo.bids.Length - 1)
        {
            idBet = gameInfo.bids.Length - 1;
        }
        else if(idBet < 0)
        {
            idBet = 0;
        }

        totalBet = gameInfo.bids[idBet];
        betText.text = totalBet.ToString("BET: ##,# $", CultureInfo.CurrentCulture);
    }

    IEnumerator GetGameInfo(string uri, Action<GameInfo> gameInfoAction)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        yield return webRequest.SendWebRequest();

        if(webRequest.result == UnityWebRequest.Result.Success)
        {
            GameInfo gameInfo = JsonUtility.FromJson<GameInfo>(webRequest.downloadHandler.text);
            gameInfoAction.Invoke(gameInfo);
        }
        else
        {
            Debug.LogError(webRequest.error);
        }
    }

    IEnumerator GetRollInfo(string uri, Action<RollInfo> rollInfoAction)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            RollInfo rollInfo = JsonUtility.FromJson<RollInfo>(webRequest.downloadHandler.text);
            rollInfoAction.Invoke(rollInfo);
        }
        else
        {
            Debug.LogError(webRequest.error);
        }
    }

    [Serializable]
    public class GameInfo
    {
        public int balance;
        public int[] bids;
    }

    [Serializable]
    public class RollInfo
    {
        public bool status;
        public bool win;
        public int win_amount;
        public int balance;
        public ReelData[] result;
    }

    [Serializable]
    public class ReelData
    {
        public string[] iconNames;
    }
}
