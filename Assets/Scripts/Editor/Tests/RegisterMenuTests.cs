using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace JourneyOfMemories.Tests
{
    [TestFixture]
    public class RegisterMenuTests
    {
        private GameObject testRoot;
        private RegisterMenuController controller;
        private InputField userInput;
        private InputField emailInput;
        private InputField passInput;
        private Toggle termsToggle;
        private Button regBtn;
        private Button backBtn;
        private Text statusText;
        private CanvasGroup fadeOverlay;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("[RegisterMenuTestRoot]");

            // 1. Tạo Canvas ảo
            GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObj.transform.SetParent(testRoot.transform);

            // 2. Tạo Inputs
            userInput = CreateInputField("UserInput", canvasObj.transform);
            emailInput = CreateInputField("EmailInput", canvasObj.transform);
            passInput = CreateInputField("PassInput", canvasObj.transform);

            // 3. Tạo Toggle
            GameObject toggleObj = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(canvasObj.transform);
            termsToggle = toggleObj.GetComponent<Toggle>();
            termsToggle.isOn = false;

            // 4. Tạo Buttons
            regBtn = CreateButton("RegButton", canvasObj.transform);
            backBtn = CreateButton("BackButton", canvasObj.transform);

            // 5. Tạo Status Text
            GameObject textObj = new GameObject("StatusText", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(canvasObj.transform);
            statusText = textObj.GetComponent<Text>();

            // 6. Tạo ScreenFadeOverlay
            GameObject fadeObj = new GameObject("FadeOverlay", typeof(RectTransform), typeof(CanvasGroup));
            fadeObj.transform.SetParent(canvasObj.transform);
            fadeOverlay = fadeObj.GetComponent<CanvasGroup>();

            // 7. Tạo Controller
            GameObject ctrlObj = new GameObject("Controller", typeof(RegisterMenuController));
            ctrlObj.transform.SetParent(testRoot.transform);
            controller = ctrlObj.GetComponent<RegisterMenuController>();

            controller.usernameInput = userInput;
            controller.emailInput = emailInput;
            controller.passwordInput = passInput;
            controller.termsToggle = termsToggle;
            controller.registerButton = regBtn;
            controller.backButton = backBtn;
            controller.statusText = statusText;
            controller.screenFadeOverlay = fadeOverlay;
            controller.fadeDuration = 0.05f;

            // Dọn dẹp PlayerPrefs test
            PlayerPrefs.DeleteKey("RegisteredUser_Name");
            PlayerPrefs.DeleteKey("RegisteredUser_Email");
            PlayerPrefs.DeleteKey("RegisteredUser_IsMock");
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
            PlayerPrefs.DeleteKey("RegisteredUser_Name");
            PlayerPrefs.DeleteKey("RegisteredUser_Email");
            PlayerPrefs.DeleteKey("RegisteredUser_IsMock");
        }

        private InputField CreateInputField(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(InputField));
            obj.transform.SetParent(parent);
            InputField inf = obj.GetComponent<InputField>();

            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(obj.transform);
            inf.textComponent = txtObj.GetComponent<Text>();

            return inf;
        }

        private Button CreateButton(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(RegisterButtonEffect));
            obj.transform.SetParent(parent);
            return obj.GetComponent<Button>();
        }

        [Test]
        public void Validation_Fails_WhenUsernameEmpty()
        {
            userInput.text = "";
            emailInput.text = "test@example.com";
            passInput.text = "123456";
            termsToggle.isOn = true;

            controller.OnRegisterClicked();

            Assert.IsNotEmpty(statusText.text);
            StringAssert.Contains("Tên người chơi", statusText.text);
            Assert.AreEqual(0, PlayerPrefs.GetInt("RegisteredUser_IsMock", 0));
        }

        [Test]
        public void Validation_Fails_WhenUsernameTooShort()
        {
            userInput.text = "ab";
            emailInput.text = "test@example.com";
            passInput.text = "123456";
            termsToggle.isOn = true;

            controller.OnRegisterClicked();

            Assert.IsNotEmpty(statusText.text);
            StringAssert.Contains("3 ký tự", statusText.text);
            Assert.AreEqual(0, PlayerPrefs.GetInt("RegisteredUser_IsMock", 0));
        }

        [Test]
        public void Validation_Fails_WhenEmailInvalid()
        {
            userInput.text = "PlayerOne";
            emailInput.text = "invalid-email-address";
            passInput.text = "123456";
            termsToggle.isOn = true;

            controller.OnRegisterClicked();

            Assert.IsNotEmpty(statusText.text);
            StringAssert.Contains("Email", statusText.text);
            Assert.AreEqual(0, PlayerPrefs.GetInt("RegisteredUser_IsMock", 0));
        }

        [Test]
        public void Validation_Fails_WhenPasswordTooShort()
        {
            userInput.text = "PlayerOne";
            emailInput.text = "valid@gmail.com";
            passInput.text = "123";
            termsToggle.isOn = true;

            controller.OnRegisterClicked();

            Assert.IsNotEmpty(statusText.text);
            StringAssert.Contains("6 ký tự", statusText.text);
            Assert.AreEqual(0, PlayerPrefs.GetInt("RegisteredUser_IsMock", 0));
        }

        [Test]
        public void Validation_Fails_WhenTermsNotAgreed()
        {
            userInput.text = "PlayerOne";
            emailInput.text = "valid@gmail.com";
            passInput.text = "123456";
            termsToggle.isOn = false;

            controller.OnRegisterClicked();

            Assert.IsNotEmpty(statusText.text);
            StringAssert.Contains("Điều khoản", statusText.text);
            Assert.AreEqual(0, PlayerPrefs.GetInt("RegisteredUser_IsMock", 0));
        }

        [Test]
        public void RegisterMenu_Scene_ExistsInBuildSettings()
        {
            bool found = false;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path.Contains("RegisterMenu.unity") && scene.enabled)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "RegisterMenu.unity phải có mặt trong Scenes In Build và được bật!");
        }

        [Test]
        public void MainMenu_HasRegisterMenuAsTargetScene()
        {
            GameObject mmObj = new GameObject("MMController", typeof(MainMenuController));
            var mm = mmObj.GetComponent<MainMenuController>();
            Assert.AreEqual("RegisterMenu", mm.registerSceneName, "MainMenuController phải cấu hình registerSceneName là RegisterMenu!");
            Object.DestroyImmediate(mmObj);
        }

        [Test]
        public void TermsToggle_SynchronizesGraphicState_WhenToggled()
        {
            GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkObj.transform.SetParent(termsToggle.transform);
            termsToggle.graphic = checkObj.GetComponent<Image>();

            // Khi toggle tắt, checkmark phải tắt
            controller.OnTermsToggleChanged(false);
            Assert.IsFalse(checkObj.activeSelf, "Checkmark phải bị ẩn khi toggle không được tích chọn!");

            // Khi toggle bật, checkmark phải bật
            controller.OnTermsToggleChanged(true);
            Assert.IsTrue(checkObj.activeSelf, "Checkmark phải được hiển thị khi toggle được tích chọn!");
        }

        [Test]
        public void RegisterMenu_Scene_ContainsOnlySingleCheckboxAndNoDuplicateElements()
        {
            string scenePath = "Assets/Scenes/RegisterMenu.unity";
            Assert.IsTrue(System.IO.File.Exists(scenePath), "RegisterMenu.unity phải tồn tại!");

            string content = System.IO.File.ReadAllText(scenePath);
            Assert.IsFalse(content.Contains("m_Name: Background\n") || content.Contains("m_Name: Background\r\n"),
                "Không được có GameObject 'Background' trùng lặp đè ô checkbox!");
            Assert.IsTrue(content.Contains("m_Name: Checkmark\n") || content.Contains("m_Name: Checkmark\r\n"),
                "Phải có GameObject 'Checkmark' hiển thị dấu tích khi bật toggle!");
            Assert.IsTrue(content.Contains("m_Name: Label\n") || content.Contains("m_Name: Label\r\n"),
                "Phải có đúng GameObject 'Label' hiển thị dòng chữ Điều khoản sử dụng rõ nét!");
        }

        [Test]
        public void LoginPrompt_ExistsAndTriggersTabSwitch_WhenClicked()
        {
            string scenePath = "Assets/Scenes/RegisterMenu.unity";
            Assert.IsTrue(System.IO.File.Exists(scenePath), "RegisterMenu.unity phải tồn tại!");

            string content = System.IO.File.ReadAllText(scenePath);
            Assert.IsTrue(content.Contains("m_Name: LoginPrompt"), "Phải có GameObject 'LoginPrompt'!");
            Assert.IsTrue(content.Contains("ĐĂNG NHẬP") || content.Contains("\\u0110\\u0102NG NH\\u1EACP"), "Phải có chữ 'ĐĂNG NHẬP'!");
            Assert.IsTrue(content.Contains("loginLinkButton:"), "Controller phải nối loginLinkButton!");

            // Test OnLoginClicked
            controller.OnLoginClicked();
            StringAssert.Contains("ĐĂNG NHẬP", statusText.text);
        }
    }
}
