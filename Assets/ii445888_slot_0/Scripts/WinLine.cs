using UnityEngine;
using System.Collections;

public class WinLine : MonoBehaviour
{
    private IEnumerator Start()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        for(int i = 0; i < 6; i++)
        {
            lineRenderer.enabled = i % 2 == 0;
            yield return new WaitForSeconds(0.25f);
        }

        Destroy(gameObject);
    }
}
