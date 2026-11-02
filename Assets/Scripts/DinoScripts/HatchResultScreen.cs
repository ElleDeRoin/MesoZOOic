using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HatchResultScreen : MonoBehaviour
{
    [Header("Dino")]
    public Image dinoImage;
    public TMP_Text nameText;
    public TMP_Text rarityText;
    public TMP_Text nextButtonLabel;
    public Button nextButton;
    public UnityEvent onFinished;

    [Header("3D Display")]

    public Transform modelAnchor;
    public RawImage modelView;
    public Camera previewCamera;
    public float spinSpeed = 30f;
    public float displaySize = 2f;

    [Header("Egg animation")]
    public Image eggImage;
    public Sprite[] regularEggFrames;
    public Sprite[] paidEggFrames;
    public float framesPerSecond = 12f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hatchSound;
    [Range(0f, 1f)]
    [SerializeField] private float hatchSoundVolume = 1f;

    private int index;
    private bool revealed;
    private bool hatching;
    private Transform modelPivot;

    private Sprite[] CurrentFrames =>
        EggHatcher.LastHatchWasPaid ? paidEggFrames : regularEggFrames;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void OnEnable()
    {
        index = 0;
        hatching = false;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNext);
        nextButton.interactable = true;
        ShowEgg();
    }

    private void OnDisable()
    {
        ClearModel();
    }

    private void Update()
    {
        if (modelPivot != null)
            modelPivot.Rotate(0f, spinSpeed * Time.unscaledDeltaTime, 0f);
    }

    private void OnNext()
    {
        if (hatching) return;

        if (!revealed)
        {
            StartCoroutine(HatchRoutine());
            return;
        }

        index++;
        if (index >= EggHatcher.LastHatch.Count) { onFinished?.Invoke(); return; }
        ShowEgg();
    }

    private void ShowEgg()
    {
        ClearModel();

        dinoImage.gameObject.SetActive(false);
        eggImage.gameObject.SetActive(true);

        var frames = CurrentFrames;
        if (frames != null && frames.Length > 0)
            eggImage.sprite = frames[0];
        else
            Debug.LogWarning("You forgot to assign the frames Dawg.");

        nameText.text = "[???]!";
        nextButtonLabel.text = "Reveal";
        revealed = false;

        if (rarityText != null)
            rarityText.text = "";
    }

    private IEnumerator HatchRoutine()
    {
        hatching = true;
        nextButton.interactable = false;

        PlayHatchSound();

        var frames = CurrentFrames;
        if (frames != null && frames.Length > 0)
        {
            float delay = 1f / framesPerSecond;
            for (int i = 0; i < frames.Length; i++)
            {
                eggImage.sprite = frames[i];
                yield return new WaitForSecondsRealtime(delay);
            }
        }

        Reveal();
        nextButton.interactable = true;
        hatching = false;
    }

    private void PlayHatchSound()
    {
        if (audioSource == null || hatchSound == null)
            return;

        audioSource.PlayOneShot(hatchSound, hatchSoundVolume);
    }

    private void Reveal()
    {
        var dino = EggHatcher.LastHatch[index];

        eggImage.gameObject.SetActive(false);
        ShowDino(dino);

        nameText.text = $"[{dino.displayName}]!";

        if (rarityText != null)
            rarityText.text = dino.rarity.ToString();

        bool last = index >= EggHatcher.LastHatch.Count - 1;
        nextButtonLabel.text = last ? "Done" : "Next";
        revealed = true;
    }

    private void ShowDino(DinoData dino)
    {
        ClearModel();

        bool canShowModel = dino.hatchModel != null && modelAnchor != null;

        if (canShowModel)
        {
            SpawnModel(dino.hatchModel);

            dinoImage.gameObject.SetActive(false);

            if (modelView != null)
                modelView.gameObject.SetActive(true);

            if (previewCamera != null)
                previewCamera.enabled = true;
        }
        else
        {
            dinoImage.sprite = dino.icon;
            dinoImage.color = Color.white;
            dinoImage.gameObject.SetActive(true);
        }
    }

    private void SpawnModel(GameObject prefab)
    {
        GameObject pivotObject = new GameObject("ModelPivot");
        modelPivot = pivotObject.transform;
        modelPivot.SetParent(modelAnchor, false);

        GameObject model = Instantiate(prefab, modelPivot);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        SetLayerRecursive(pivotObject, modelAnchor.gameObject.layer);

        Bounds bounds = GetBounds(model);
        float largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        if (largest > 0.0001f)
            modelPivot.localScale = Vector3.one * (displaySize / largest);

        bounds = GetBounds(model);
        model.transform.position += modelPivot.position - bounds.center;
    }

    private void ClearModel()
    {
        if (modelPivot != null)
            Destroy(modelPivot.gameObject);

        modelPivot = null;

        if (previewCamera != null)
            previewCamera.enabled = false;

        if (modelView != null)
            modelView.gameObject.SetActive(false);
    }

    private static Bounds GetBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        bool found = false;

        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer)
                continue;

            if (!found)
            {
                bounds = r.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        return bounds;
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}