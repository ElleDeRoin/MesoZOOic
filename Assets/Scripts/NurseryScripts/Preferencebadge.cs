using UnityEngine;
public class PreferenceBadge : MonoBehaviour
{
    [SerializeField] private GameObject favoriteBadge;
    [SerializeField] private GameObject dislikedBadge;

    public void Show(Preference preference)
    {
        if (favoriteBadge != null)
            favoriteBadge.SetActive(preference == Preference.Favorite);

        if (dislikedBadge != null)
            dislikedBadge.SetActive(preference == Preference.Disliked);
    }
}