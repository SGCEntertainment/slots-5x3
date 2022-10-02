using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    private static SlotMachine instance;
    public static SlotMachine Instance
    {
        get
        {
            if(!instance)
            {
                instance = FindObjectOfType<SlotMachine>();
            }

            return instance;
        }
    }

    int maxCycle;
    public static float width;

    public delegate void HandlePulledDelegate(ReelData[] reelDatas);
    public event HandlePulledDelegate OnPullEvent;

    [Space(10)]
    public AnimationCurve speedCurve;

    [Space(10)]
    [SerializeField] Button spinBtn;

    [Space(10)]
    [SerializeField] AudioSource spinSource;
    [SerializeField] AudioSource stoppingSource;

    public static bool autoSpin;

    [SerializeField] Row[] rows;
    [SerializeField] SlotData[] slotDatas;

    public void Pull(Manager.ReelData[] reelData)
    {
        int rowID = 0;
        ReelData[] reelDatas = new ReelData[rows.Length];

        for (int i = 0; i < reelDatas.Length;)
        {
            int startPoint = rowID * 75;
            int rv = UnityEngine.Random.Range(150 + startPoint, startPoint + 250);

            SlotData[] _slotDatas = GetResultSlotDataInRow(reelData[i].iconNames);
            reelDatas[i] = new ReelData(rv, _slotDatas);

            rowID++;
            i++;
        }

        maxCycle = reelDatas[reelDatas.Length - 1].cycles;

        OnPullEvent?.Invoke(reelDatas);
        StartCoroutine(nameof(Rolling));
    }

    public SlotData GetSlotDataByString(string _name) => _name switch
    {
        "Strawberry" => slotDatas[0],
        "Orange" => slotDatas[1],
        "Banana" => slotDatas[2],
        "Blueberry" => slotDatas[3],
        "Lemon" => slotDatas[4],
        "Plum" => slotDatas[5],
        "Pear" => slotDatas[6],
        "Cherry" => slotDatas[7],
    };

    public SlotData[] GetResultSlotDataInRow(string[] names)
    {
        SlotData[] _slotDatas = new SlotData[3];
        for(int i = 0; i < names.Length; i++)
        {
            _slotDatas[i] = GetSlotDataByString(names[i]);
        }

        return _slotDatas;
    }

    public void PlayStoppingSound()
    {
        stoppingSource.Play();
    }

    IEnumerator Rolling()
    {
        spinSource.Play();
        spinBtn.interactable = autoSpin && false;

        float elDistance = 0.0f;
        float totalDistance = maxCycle * width;
        while(elDistance < totalDistance)
        {
            float time = elDistance / totalDistance;
            elDistance = Mathf.MoveTowards(elDistance, totalDistance, speedCurve.Evaluate(time) * Time.deltaTime);
            yield return null;
        }

        Manager.Instance.CalculatePrize();
        yield return new WaitForSeconds(0.5f);

        if(autoSpin)
        {
            yield return new WaitForSeconds(0.5f);
            Manager.Instance.TrySpin();
        }

        spinBtn.interactable = !autoSpin;
        spinSource.Stop();
    }

    [Serializable]
    public class SlotData
    {
        public string value;
        public Sprite icon;
    }

    [Serializable]
    public class ReelData
    {
        public int cycles;
        public SlotData[] slotDatas;

        public ReelData(int cycles, SlotData[] slotDatas)
        {
            this.cycles = cycles;
            this.slotDatas = slotDatas;
        }
    }
}
