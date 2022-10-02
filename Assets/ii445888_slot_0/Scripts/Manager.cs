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
    [SerializeField] GameObject warningGO;

    [Space(10)]
    [SerializeField] Transform winlineParent;
    [SerializeField] GameObject[] winLinePrefabs;

    GameInfo gameInfo;
    RollInfo rollInfo;

    private void Start()
    {
        #if !UNITY_EDITOR

        if(!string.Equals(Application.absoluteURL, "https://slots5x3.netlify.app/"))
        {
            warningGO.SetActive(true);
            return;
        }

        #endif

        StartCoroutine(GetGameInfo("https://disbark.ru/load?game_id=100&user_id=2", (_gameInfo) => 
        {
            gameInfo = _gameInfo;

            UpdateCoinsCount(gameInfo.balance);
            ChangeBet(0);
        }));
    }

    private void Update()
    {
        if(!statusAtoSpinGO.activeSelf)
        {
            return;
        }

        statusAtoSpinGO.transform.Rotate(120.0f * Time.deltaTime * Vector3.back);
    }

    bool CanSpin()
    {
        return gameInfo.balance >= totalBet && totalBet > 0;
    }

    public void TrySpin()
    {

        if (!CanSpin() || trySpin)
        {
            if (gameInfo.balance <= 0)
            {
                SlotMachine.autoSpin = false;
                statusAtoSpinGO.SetActive(false);
            }
            return;
        }

        trySpin = true;

        StartCoroutine(GetRollInfo($"https://disbark.ru/spin?game_id=100&user_id=2&bid={totalBet}", (_rollInfo) =>
        {
            rollInfo = _rollInfo;

            UpdateCoinsCount(rollInfo.balance);
            SlotMachine.Instance.Pull(rollInfo.result);

            trySpin = false;
        }));
    }

    GameObject GetWinLineById(int id) => id switch
    {
        1 => winLinePrefabs[0],
        2 => winLinePrefabs[1],
        3 => winLinePrefabs[2]
    };

    public void CalculatePrize()
    {
        foreach (int i in rollInfo.winlines)
        {
            Instantiate(GetWinLineById(i), winlineParent);
        }
    }

    public void SetAutoSpin()
    {
        SlotMachine.autoSpin = !SlotMachine.autoSpin;
        statusAtoSpinGO.SetActive(SlotMachine.autoSpin);
    }

    public void UpdateCoinsCount(int amount)
    {
        gameInfo.balance = amount;
        winText.text = gameInfo.balance > 0 ? gameInfo.balance.ToString("WIN: ##,# $", CultureInfo.CurrentCulture) : $"WIN: {0} $";
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
        public int[] winlines;
        public ReelData[] result;
    }

    [Serializable]
    public class ReelData
    {
        public string[] iconNames;
    }
}
