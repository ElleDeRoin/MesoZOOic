using UnityEngine;

public class PenManager : MonoBehaviour
{
    public Transform penRoot;
    private GameObject currentPen;

    public void ShowPen(OwnedDino owned)
    {
        if (currentPen != null) Destroy(currentPen);

        var data = DinoCollection.Instance.GetData(owned.dataId);
        currentPen = Instantiate(data.penPrefab, penRoot);

        var stats = currentPen.GetComponentInChildren<DinoStats>();
        stats.Initialize(owned.instanceId, data);
    }

    public void ClosePen()
    {
        if (currentPen != null) Destroy(currentPen);
    }
}