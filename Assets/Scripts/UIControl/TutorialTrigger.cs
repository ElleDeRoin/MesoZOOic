using System;
using UnityEngine;

public enum TutorialAction
{
    None,
    ShopOpened,
    FoodFed,
    DecorTabOpened,
    DecorationPlaced,
    DinoPetted,
    DinoWashed,
    DinoBrushed
}

public static class TutorialEvents
{
    public static event Action<TutorialAction> Raised;
    public static void Raise(TutorialAction action) => Raised?.Invoke(action);
}

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialAction action;

    public void Fire() => TutorialEvents.Raise(action);
}