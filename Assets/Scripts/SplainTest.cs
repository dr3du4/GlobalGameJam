using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class SplainTest : MonoBehaviour
{
    [SerializeField] private float xFrequency = 7f / 8f;
    [SerializeField] private float yFrequency = 5f / 4f;
    [SerializeField] private float yPhase = 2f;
    [SerializeField] private float zFrequency = 13f / 16f;
    [SerializeField] private float zPhase = 1f;

    private SplineContainer splineContainer;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
    }

    private void Update()
    {
        Vector3 position = new Vector3(
            Mathf.Sin(xFrequency * Time.time),
            Mathf.Sin(yPhase + yFrequency * Time.time),
            Mathf.Cos(zPhase + zFrequency * Time.time)
        );
        var knot = splineContainer.Splines[0][1];
        knot.Position = position;
        splineContainer.Splines[0][1] = knot;
    }
}
