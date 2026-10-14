using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tự động cập nhật nội dung Text UI theo ngôn ngữ được chọn trong LocalizationManager.
/// </summary>
[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    [TextArea(2, 5)]
    [Tooltip("Nội dung hiển thị bằng Tiếng Việt")]
    public string vietnameseText;

    [TextArea(2, 5)]
    [Tooltip("Nội dung hiển thị bằng Tiếng Anh")]
    public string englishText;

    private Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
    }

    private void OnEnable()
    {
        UpdateText(LocalizationManager.CurrentLanguage);
        LocalizationManager.OnLanguageChanged += UpdateText;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText(GameLanguage lang)
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<Text>();
        }

        if (textComponent != null)
        {
            textComponent.text = (lang == GameLanguage.Vietnamese) ? vietnameseText : englishText;
        }
    }
}
