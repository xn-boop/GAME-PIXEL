using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JourneyOfMemories.Tests
{
    /// <summary>
    /// Kiểm tra cấu hình bảo mật hiển thị của ô mật khẩu trong scene LoginMenu thật.
    /// Các test này chỉ xác nhận giao diện che ký tự; chúng không biến Local Mock Demo
    /// thành một hệ thống xác thực hoặc mã hóa mật khẩu.
    /// </summary>
    [TestFixture]
    public class LoginPasswordInputTests
    {
        private const string LoginScenePath = "Assets/Scenes/LoginMenu.unity";

        private Scene previousScene;
        private string previousScenePath;
        private LoginMenuController controller;
        private InputField passwordInput;
        private string originalPasswordText;

        [SetUp]
        public void SetUp()
        {
            Assert.IsTrue(File.Exists(LoginScenePath), "Không tìm thấy scene LoginMenu để kiểm tra.");

            previousScene = SceneManager.GetActiveScene();
            previousScenePath = previousScene.path;

            EditorSceneManager.OpenScene(LoginScenePath, OpenSceneMode.Single);
            controller = Object.FindAnyObjectByType<LoginMenuController>();

            Assert.IsNotNull(controller, "LoginMenu phải có LoginMenuController.");
            Assert.IsNotNull(controller.passwordInput, "LoginMenuController phải được gán Password Input.");

            passwordInput = controller.passwordInput;
            originalPasswordText = passwordInput.text;
        }

        [TearDown]
        public void TearDown()
        {
            if (passwordInput != null)
            {
                passwordInput.text = originalPasswordText;
                passwordInput.ForceLabelUpdate();
            }

            // Không lưu scene sau test để test không thay đổi asset của dự án.
            if (!string.IsNullOrEmpty(previousScenePath) && File.Exists(previousScenePath))
            {
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }
        }

        [Test]
        public void PasswordField_UsesPasswordContentType()
        {
            Assert.AreEqual(
                InputField.ContentType.Password,
                passwordInput.contentType,
                "Rủi ro lộ mật khẩu: Content Type không phải Password thì ký tự nhập có thể hiện nguyên văn.");
        }

        [Test]
        public void PasswordField_UsesAsteriskAsMaskCharacter()
        {
            Assert.AreEqual(
                '*',
                passwordInput.asteriskChar,
                "Yêu cầu UI: ô mật khẩu phải hiển thị ký tự che là '*'.");
        }

        [Test]
        public void PasswordField_MasksVisibleText_ButRetainsActualValueForLogin()
        {
            const string rawPassword = "Secret123";
            const string expectedMask = "*********";

            passwordInput.text = rawPassword;
            passwordInput.ForceLabelUpdate();

            Assert.AreEqual(
                rawPassword,
                passwordInput.text,
                "Logic đăng nhập phải nhận được mật khẩu thật; không được ghi '*' vào passwordInput.text.");
            Assert.IsNotNull(passwordInput.textComponent, "Password Input phải có Text Component để hiển thị.");
            Assert.AreEqual(
                expectedMask,
                passwordInput.textComponent.text,
                "Rủi ro lộ mật khẩu: giao diện đang không che đúng toàn bộ ký tự đã nhập.");
        }

        [Test]
        public void PasswordField_IsSingleLine()
        {
            Assert.AreEqual(
                InputField.LineType.SingleLine,
                passwordInput.lineType,
                "Rủi ro UX: ô mật khẩu không nên cho xuống dòng hoặc gửi ký tự newline vào quá trình xác thực.");
        }

        [Test]
        public void EmailField_RemainsEmailInput_AndIsNotMasked()
        {
            Assert.IsNotNull(controller.emailInput, "LoginMenuController phải được gán Email Input.");
            Assert.AreEqual(
                InputField.ContentType.EmailAddress,
                controller.emailInput.contentType,
                "Rủi ro UX: cấu hình Password không được áp nhầm sang ô Email.");
        }
    }
}
