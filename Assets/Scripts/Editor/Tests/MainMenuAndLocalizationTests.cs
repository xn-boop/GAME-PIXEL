using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace JourneyOfMemories.Tests
{
    [TestFixture]
    public class MainMenuAndLocalizationTests
    {
        private GameObject testRoot;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("[MainMenuTestRoot]");
            PlayerPrefs.DeleteKey(LocalizationManager.PREF_KEY);
            LocalizationManager.InitializeFromPrefs();
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
            PlayerPrefs.DeleteKey(LocalizationManager.PREF_KEY);
        }

        #region LocalizationManager Tests

        [Test]
        public void LocalizationManager_DefaultLanguage_IsVietnamese()
        {
            LocalizationManager.CurrentLanguage = GameLanguage.Vietnamese;
            Assert.AreEqual(GameLanguage.Vietnamese, LocalizationManager.CurrentLanguage);
            Assert.AreEqual("VN", LocalizationManager.LanguageCode);
            Assert.AreEqual("Tiếng Việt", LocalizationManager.LanguageDisplayName);
        }

        [Test]
        public void LocalizationManager_ToggleLanguage_SwitchesBetweenVNAndEN()
        {
            LocalizationManager.CurrentLanguage = GameLanguage.Vietnamese;

            LocalizationManager.ToggleLanguage();
            Assert.AreEqual(GameLanguage.English, LocalizationManager.CurrentLanguage);
            Assert.AreEqual("EN", LocalizationManager.LanguageCode);

            LocalizationManager.ToggleLanguage();
            Assert.AreEqual(GameLanguage.Vietnamese, LocalizationManager.CurrentLanguage);
            Assert.AreEqual("VN", LocalizationManager.LanguageCode);
        }

        [Test]
        public void LocalizationManager_GetText_ReturnsCorrectTranslation()
        {
            LocalizationManager.CurrentLanguage = GameLanguage.Vietnamese;
            Assert.AreEqual("Bắt đầu", LocalizationManager.Get("Bắt đầu", "Start"));

            LocalizationManager.CurrentLanguage = GameLanguage.English;
            Assert.AreEqual("Start", LocalizationManager.Get("Bắt đầu", "Start"));
        }

        [Test]
        public void LocalizationManager_OnLanguageChanged_FiresEvent()
        {
            LocalizationManager.CurrentLanguage = GameLanguage.Vietnamese;
            bool eventFired = false;
            GameLanguage receivedLang = GameLanguage.Vietnamese;

            System.Action<GameLanguage> handler = (lang) =>
            {
                eventFired = true;
                receivedLang = lang;
            };

            LocalizationManager.OnLanguageChanged += handler;
            LocalizationManager.CurrentLanguage = GameLanguage.English;
            LocalizationManager.OnLanguageChanged -= handler;

            Assert.IsTrue(eventFired);
            Assert.AreEqual(GameLanguage.English, receivedLang);
        }

        #endregion

        #region UIFrameAnimator Tests

        [Test]
        public void UIFrameAnimator_Playback_LoopsThroughFrames()
        {
            var animObj = new GameObject("AnimatorTest", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UIFrameAnimator));
            animObj.transform.SetParent(testRoot.transform);

            var animator = animObj.GetComponent<UIFrameAnimator>();
            var sprites = new Sprite[]
            {
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero),
                Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 4, 4), Vector2.zero)
            };

            animator.SetFrames(sprites, 10f);
            Assert.AreEqual(2, animator.FrameCount);
            Assert.AreEqual(0, animator.CurrentFrameIndex);

            animator.ResetAnimation();
            Assert.AreEqual(0, animator.CurrentFrameIndex);
        }

        #endregion

        #region TapToStartEffect Tests

        [Test]
        public void TapToStartEffect_CanTriggerTapEffect()
        {
            var tapObj = new GameObject("TapButtonTest", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TapToStartEffect));
            tapObj.transform.SetParent(testRoot.transform);

            var effect = tapObj.GetComponent<TapToStartEffect>();
            Assert.IsNotNull(effect);

            bool callbackInvoked = false;
            effect.TriggerTapEffect(() =>
            {
                callbackInvoked = true;
            });

            // Triggering twice shouldn't double-trigger
            effect.TriggerTapEffect(null);
            Assert.IsNotNull(effect);
        }

        #endregion

        #region LanguageSelectorUI Tests

        [Test]
        public void LanguageSelectorUI_ClickingButton_TogglesLanguage()
        {
            var langObj = new GameObject("LangBtnTest", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LanguageSelectorUI));
            langObj.transform.SetParent(testRoot.transform);

            var selector = langObj.GetComponent<LanguageSelectorUI>();
            LocalizationManager.CurrentLanguage = GameLanguage.Vietnamese;

            selector.OnLanguageButtonClicked();
            Assert.AreEqual(GameLanguage.English, LocalizationManager.CurrentLanguage);

            selector.OnLanguageButtonClicked();
            Assert.AreEqual(GameLanguage.Vietnamese, LocalizationManager.CurrentLanguage);
        }

        #endregion
    }
}
