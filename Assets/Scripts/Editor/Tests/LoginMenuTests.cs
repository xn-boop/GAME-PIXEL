using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace JourneyOfMemories.Tests
{
    [TestFixture]
    public class LoginMenuTests
    {
        private GameObject testRoot;
        private LoginMenuController controller;
        private InputField emailInput;
        private InputField passInput;
        private Toggle rememberToggle;
        private Button loginBtn;
        private Button backBtn;
        private Button forgotBtn;
        private Button regLinkBtn;
        private Text statusText;
        private CanvasGroup fadeOverlay;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("[LoginMenuTestRoot]");

            GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObj.transform.SetParent(testRoot.transform);

            emailInput = CreateInputField("EmailInput", canvasObj.transform);
            passInput = CreateInputField("PassInput", canvasObj.transform);

            GameObject toggleObj = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(canvasObj.transform);
            rememberToggle = toggleObj.GetComponent<Toggle>();
            rememberToggle.isOn = false;

            loginBtn = CreateButton("LoginButton", canvasObj.transform);
            backBtn = CreateButton("BackButton", canvasObj.transform);
            forgotBtn = CreateButton("ForgotPasswordButton", canvasObj.transform);
            regLinkBtn = CreateButton("RegisterLinkButton", canvasObj.transform);

            GameObject textObj = new GameObject("StatusText", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(canvasObj.transform);
            statusText = textObj.GetComponent<Text>();

            GameObject fadeObj = new GameObject("FadeOverlay", typeof(RectTransform), typeof(CanvasGroup));
            fadeObj.transform.SetParent(canvasObj.transform);
            fadeOverlay = fadeObj.GetComponent<CanvasGroup>();

            GameObject ctrlObj = new GameObject("Controller", typeof(LoginMenuController));
            ctrlObj.transform.SetParent(testRoot.transform);
            controller = ctrlObj.GetComponent<LoginMenuController>();

            controller.emailInput = emailInput;
            controller.passwordInput = passInput;
            controller.rememberToggle = rememberToggle;
            controller.loginButton = loginBtn;
            controller.backButton = backBtn;
            controller.forgotPasswordButton = forgotBtn;
            controller.registerLinkButton = regLinkBtn;
            controller.statusText = statusText;
            controller.screenFadeOverlay = fadeOverlay;
            controller.fadeDuration = 0.05f;

            PlayerPrefs.DeleteKey("RememberLogin");
            PlayerPrefs.DeleteKey("SavedLogin_Email");
            PlayerPrefs.DeleteKey("LoggedInUser_Email");
            PlayerPrefs.DeleteKey("LoggedInUser_IsMock");
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
            PlayerPrefs.DeleteKey("RememberLogin");
            PlayerPrefs.DeleteKey("SavedLogin_Email");
            PlayerPrefs.DeleteKey("LoggedInUser_Email");
            PlayerPrefs.DeleteKey("LoggedInUser_IsMock");
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
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Button));
            obj.transform.SetParent(parent);
            return obj.GetComponent<Button>();
        }

        [Test]
        public void Validation_Fails_WhenEmailEmpty()
        {
            emailInput.text = "";
            passInput.text = "123456";

            controller.OnLoginClicked();

            Assert.IsTrue(statusText.text.Contains("Email"), "Phải báo lỗi khi Email rỗng.");
            Assert.AreEqual(new Color(0.92f, 0.25f, 0.2f, 1f), statusText.color, "Màu thông báo lỗi phải là màu đỏ.");
        }

        [Test]
        public void Validation_Fails_WhenEmailInvalidFormat()
        {
            emailInput.text = "invalid-email-no-at";
            passInput.text = "123456";

            controller.OnLoginClicked();

            Assert.IsTrue(statusText.text.Contains("định dạng") || statusText.text.Contains("format"), "Phải báo lỗi khi Email sai định dạng.");
        }

        [Test]
        public void Validation_Fails_WhenPasswordEmpty()
        {
            emailInput.text = "player@gmail.com";
            passInput.text = "";

            controller.OnLoginClicked();

            Assert.IsTrue(statusText.text.Contains("Mật khẩu") || statusText.text.Contains("Password"), "Phải báo lỗi khi Mật khẩu rỗng.");
        }

        [Test]
        public void Validation_Fails_WhenPasswordTooShort()
        {
            emailInput.text = "player@gmail.com";
            passInput.text = "123";

            controller.OnLoginClicked();

            Assert.IsTrue(statusText.text.Contains("6 ký tự") || statusText.text.Contains("6 characters"), "Phải báo lỗi khi Mật khẩu dưới 6 ký tự.");
        }

        [Test]
        public void Validation_Succeeds_WhenValidCredentials()
        {
            emailInput.text = "testuser@example.com";
            passInput.text = "secret123";

            controller.OnLoginClicked();

            Assert.IsTrue(statusText.text.Contains("Đang xác thực") || statusText.text.Contains("Verifying"),
                "Khi dữ liệu hợp lệ, phải hiển thị trạng thái bắt đầu xác thực.");
        }

        [Test]
        public void RememberToggle_SavesState_InPlayerPrefs()
        {
            controller.OnRememberToggleChanged(true);
            Assert.AreEqual(1, PlayerPrefs.GetInt("RememberLogin", 0), "Toggle ON phải lưu 1 vào PlayerPrefs.");

            controller.OnRememberToggleChanged(false);
            Assert.AreEqual(0, PlayerPrefs.GetInt("RememberLogin", 1), "Toggle OFF phải lưu 0 vào PlayerPrefs.");
        }

        [Test]
        public void ForgotPassword_ShowsAppropriateMessage()
        {
            emailInput.text = "";
            controller.OnForgotPasswordClicked();
            Assert.IsTrue(statusText.text.Contains("Email"), "Nhấn Quên mật khẩu khi chưa có email phải nhắc nhập Email.");

            emailInput.text = "user@example.com";
            controller.OnForgotPasswordClicked();
            Assert.IsTrue(statusText.text.Contains("phát triển") || statusText.text.Contains("development"),
                "Nhấn Quên mật khẩu khi đã có email phải thông báo tính năng đang phát triển.");
        }
    }
}
