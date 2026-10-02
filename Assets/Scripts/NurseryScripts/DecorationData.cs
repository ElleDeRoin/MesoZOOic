using UnityEngine;

public enum PropSize { Small, Large }

[CreateAssetMenu(menuName = "Zoo/Decoration", fileName = "NewDecoration")]
public class DecorationData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    public PropSize size;
    public float price = 50f;
    public float happinessPerSecond = 0.2f;
}