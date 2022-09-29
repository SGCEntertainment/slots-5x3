using System.Collections;
using UnityEngine;

public class Row : MonoBehaviour
{
    float[] pos;
    float minY;

    private void Start()
    {
        pos = new float[transform.childCount];
        for(int i = 0; i < pos.Length; i++)
        {
            pos[i] = transform.GetChild(i).localPosition.y;
        }

        SlotMachine.width = Mathf.Abs(transform.GetChild(1).localPosition.y - transform.GetChild(0).localPosition.y);
        minY = transform.GetChild(transform.childCount - 1).localPosition.y - SlotMachine.width;

        SlotMachine.Instance.OnPullEvent += (cycles) =>
        {
            int myCycle = cycles[transform.GetSiblingIndex()];
            StartCoroutine(RollMe(myCycle));
        };
    }

    void UpdateRow(float time)
    {
        foreach (Transform t in transform)
        {
            t.localPosition += SlotMachine.Instance.speedCurve.Evaluate(time) * Time.deltaTime * Vector3.down;

            if (t.localPosition.y <= minY)
            {
                t.localPosition = GetUpElementPosition() + Vector3.up * SlotMachine.width;
                t.SetAsFirstSibling();
            }
        }
    }

    Vector3 GetUpElementPosition()
    {
        return transform.GetChild(0).localPosition;
    }

    IEnumerator RollMe(int myCycle)
    {
        float elDistance = 0.0f;
        float totalDistance = myCycle * SlotMachine.width;
        while (elDistance < totalDistance)
        {
            float time = elDistance / totalDistance;

            UpdateRow(time);

            elDistance = Mathf.MoveTowards(elDistance, totalDistance, SlotMachine.Instance.speedCurve.Evaluate(time) * Time.deltaTime);
            yield return null;
        }

        foreach(Transform t in transform)
        {
            t.localPosition = new Vector2(t.localPosition.x, pos[t.GetSiblingIndex()]);
        }

        float et = 0.0f;
        float stoppingTime = 0.1f;

        Vector2 initPos = transform.localPosition;
        Vector2 targetPos = initPos + Vector2.down * 100;

        while(et < stoppingTime)
        {
            transform.localPosition = Vector2.Lerp(initPos, targetPos, et / stoppingTime);
            et += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
        Vector2 c = initPos;
        et = 0.0f;

        initPos = transform.localPosition;
        targetPos = c;

        while (et < stoppingTime)
        {
            transform.localPosition = Vector2.Lerp(initPos, targetPos, et / stoppingTime);
            et += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
        SlotMachine.Instance.PlayStoppingSound();
    }
}
