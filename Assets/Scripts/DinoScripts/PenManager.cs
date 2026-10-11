using UnityEngine;

public class PenManager : MonoBehaviour
{
    public Transform penRoot;
    private GameObject currentPen;

    private void Start()
    {
        if (GameSession.SelectedDino == null)
        {
            Debug.LogWarning("No dino selected");
            return;
        }
        ShowPen(GameSession.SelectedDino);
    }

    public void ShowPen(OwnedDino owned)
    {
        if (currentPen != null) Destroy(currentPen);

        var data = DinoCollection.Instance.GetData(owned.dataId);
        currentPen = Instantiate(data.penPrefab, penRoot);

        var stats = currentPen.GetComponentInChildren<DinoStats>();
        stats.Initialize(owned.instanceId, data);
    }
}