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

    public delegate void HandlePulledDelegate(int[] cycles);
    public event HandlePulledDelegate OnPullEvent;

    [Space(10)]
    public AnimationCurve speedCurve;

    [Space(10)]
    [SerializeField] Button spinBtn;

    [Space(10)]
    [SerializeField] AudioSource spinSource;
    [SerializeField] AudioSource stoppingSource;

    [SerializeField] Row[] rows;

    public void Pull()
    {
        Manager.Instance.TrySpin();

        int[] cycles = new int[rows.Length];
        for(int i = 0; i < cycles.Length;)
        {
            int rv = UnityEngine.Random.Range(1, 3);
            if (Array.Exists(cycles, element => element != rv))
            {
                cycles[i] = rv;
                i++;
            }
        }

        for (int i = 0; i < cycles.Length;)
        {
            int rv = cycles[i] + UnityEngine.Random.Range(50, 250);
            if (Array.Exists(cycles, element => element != rv))
            {
                cycles[i] = rv;
                i++;
            }
        }

        var sorted = cycles.OrderBy(i => i);
        cycles = sorted.ToArray();

        maxCycle = cycles[cycles.Length - 1];

        OnPullEvent?.Invoke(cycles);
        StartCoroutine(nameof(Rolling));
    }

    public void PlayStoppingSound()
    {
        stoppingSource.Play();
    }

    IEnumerator Rolling()
    {
        spinSource.Play();
        spinBtn.interactable = false;

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

        spinBtn.interactable = true;
        spinSource.Stop();
    }
}
