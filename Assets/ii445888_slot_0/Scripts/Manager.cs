using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

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

    const int minPrizeValue = 9;
    const int maxPrizeValue = 299;

    const string saveKey = "coins";
    const int initCoinsCount = 1000000;
    int coinsCount;

    int totalBet;

    [SerializeField] InputField betField;
    [SerializeField] Text totalBetWin;
    [SerializeField] Text winText;

    private void Start()
    {
        coinsCount = Load();
        CacheCopmonets();
        UpdateCoinsCount();
    }

    void CacheCopmonets()
    {
        betField.onEndEdit.AddListener((s) =>
        {
            if(string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
            {
                return;
            }

            totalBet = int.Parse(s);

            if(s.Length > 4)
            {
                totalBet = 9999;
                s = totalBet.ToString();
            }

            //betField.contentType = InputField.ContentType.Standard;
            betField.text = totalBet.ToString("BET: ##,# $", CultureInfo.CurrentCulture);
        });
    }

    public void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void Save()
    {
        PlayerPrefs.SetInt(saveKey, coinsCount);
        PlayerPrefs.Save();
    }

    int Load()
    {
        int _coins = PlayerPrefs.HasKey(saveKey) ? PlayerPrefs.GetInt(saveKey) : initCoinsCount;
        if(_coins <= 0)
        {
            _coins = minPrizeValue;
        }

        return _coins;
    }

    bool CanSpin()
    {
        return coinsCount >= totalBet && totalBet > 0;
    }

    public void TrySpin()
    {
        if(!CanSpin())
        {
            return;
        }

        coinsCount -= totalBet;
        if(coinsCount < 0)
        {
            coinsCount = 0;
        }

        UpdateCoinsCount();
        Save();
    }

    public void CalculatePrize()
    {
        int prize = Random.Range(minPrizeValue, maxPrizeValue);
        UpdateCoinsCount(prize);
        Save();
    }

    public void UpdateCoinsCount(int amount = 0)
    {
        coinsCount += amount;
        winText.text = coinsCount.ToString("WIN: ##,# $", CultureInfo.CurrentCulture);
        Save();
    }
}
