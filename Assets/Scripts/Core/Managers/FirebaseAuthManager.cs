using System;

using System.Text.RegularExpressions;

using UnityEngine;

using Firebase;

using Firebase.Auth;

using Firebase.Extensions;



namespace JourneyOfMemories.Core.Managers

{

    /// <summary>

    /// Quản lý xác thực người chơi bằng Firebase Authentication.

    /// Hỗ trợ Đăng ký, Đăng nhập, Xác minh Email (IsEmailVerified), Quên mật khẩu (SendPasswordResetEmail),

    /// và cơ chế Chơi Ngoại Tuyến (Offline) an toàn trên thiết bị cho tài khoản đã từng xác thực trực tuyến.

    /// 

    /// LƯU Ý BẢO MẬT CHẾ ĐỘ OFFLINE:

    /// - Mật khẩu KHÔNG BAO GIỜ lưu dạng văn bản thô. Chỉ sử dụng hàm băm có muối (Salted Hash) để đối chiếu khi mất mạng.

    /// - Tài khoản mới hoặc chưa từng xác minh email thành công trên thiết bị này KHÔNG ĐƯỢC PHÉP đăng nhập offline.

    /// - Dữ liệu local trên thiết bị người dùng (persistentDataPath) về mặt lý thuyết có thể bị chỉnh sửa bởi các công cụ can thiệp tệp,

    ///   nên không thể thay thế máy chủ có thẩm quyền đầy đủ khi có kết nối mạng.

    /// </summary>

    public class FirebaseAuthManager : MonoBehaviour

    {

        public static FirebaseAuthManager Instance { get; private set; }



        public bool IsFirebaseInitialized { get; private set; } = false;

        public FirebaseApp App { get; private set; }

        public FirebaseAuth Auth { get; private set; }



        public FirebaseUser CurrentUser => Auth != null ? Auth.CurrentUser : null;

        public FirebaseUser LastUnverifiedUser { get; private set; }



        public event Action OnFirebaseReady;



        private void Awake()

        {

            if (Instance != null && Instance != this)

            {

                Destroy(gameObject);

                return;

            }



            Instance = this;

            if (Application.isPlaying) DontDestroyOnLoad(gameObject);

            InitializeFirebase();

        }



        private void OnDestroy()

        {

            if (Instance == this) Instance = null;

        }



        /// <summary>

        /// Khởi tạo Firebase App và kiểm tra các dependency

        /// </summary>

        public void InitializeFirebase(Action onReady = null)

        {

            if (IsFirebaseInitialized)

            {

                onReady?.Invoke();

                return;

            }



            try

            {

                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>

                {

                    if (task.IsCompleted && !task.IsFaulted)

                    {

                        var dependencyStatus = task.Result;

                        if (dependencyStatus == DependencyStatus.Available)

                        {

                            App = FirebaseApp.DefaultInstance;

                            Auth = FirebaseAuth.DefaultInstance;

                            IsFirebaseInitialized = true;

                            Debug.Log("[FirebaseAuthManager] Firebase Authentication đã sẵn sàng.");

                            OnFirebaseReady?.Invoke();

                            onReady?.Invoke();

                        }

                        else

                        {

                            Debug.LogError($"[FirebaseAuthManager] Không thể nạp dependencies Firebase: {dependencyStatus}");

                        }

                    }

                    else

                    {

                        Debug.LogWarning($"[FirebaseAuthManager] Kiểm tra dependencies thất bại hoặc thiết bị đang ngoại tuyến: {task.Exception?.Message}");

                    }

                });

            }

            catch (Exception ex)

            {

                Debug.LogWarning($"[FirebaseAuthManager] Ngoại lệ khi nạp Firebase: {ex.Message}");

            }

        }



        /// <summary>

        /// Đăng ký tài khoản mới bằng Email và Mật khẩu qua Firebase.

        /// Tự động gửi email xác minh và hướng dẫn người chơi quay lại đăng nhập sau khi kích hoạt.

        /// </summary>

        public void RegisterWithEmailAndPassword(

            string username,

            string email,

            string password,

            Action<bool, string> onComplete)

        {

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))

            {

                onComplete?.Invoke(false, GetText("Vui lòng điền đầy đủ Email và Mật khẩu.", "Please fill in Email and Password."));

                return;

            }



            if (Auth == null)

            {

                onComplete?.Invoke(false, GetText("Dịch vụ Firebase chưa sẵn sàng. Vui lòng kiểm tra kết nối mạng.", "Firebase service is not ready. Please check your network connection."));

                return;

            }



            email = email.Trim();

            username = username?.Trim();



            Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>

            {

                if (task.IsFaulted || task.IsCanceled)

                {

                    string err = GetFriendlyErrorMessage(task.Exception);

                    Debug.LogError($"[FirebaseAuthManager] Đăng ký thất bại: {err}");

                    onComplete?.Invoke(false, err);

                    return;

                }



                var user = task.Result.User;

                Debug.Log($"[FirebaseAuthManager] Tạo tài khoản Firebase thành công: UID = {user.UserId}, Email = {user.Email}");



                // 1. Cập nhật DisplayName nếu có

                if (!string.IsNullOrEmpty(username))

                {

                    UserProfile profile = new UserProfile { DisplayName = username };

                    user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(pTask =>

                    {

                        SendVerificationEmailAndFinalize(user, username, email, password, onComplete);

                    });

                }

                else

                {

                    SendVerificationEmailAndFinalize(user, username, email, password, onComplete);

                }

            });

        }



        private void SendVerificationEmailAndFinalize(

            FirebaseUser user,

            string username,

            string email,

            string password,

            Action<bool, string> onComplete)

        {

            // 2. Gửi email xác minh

            user.SendEmailVerificationAsync().ContinueWithOnMainThread(vTask =>

            {

                if (vTask.IsFaulted)

                {

                    Debug.LogWarning($"[FirebaseAuthManager] Không thể gửi email xác minh ngay: {vTask.Exception?.Message}");

                }

                else

                {

                    Debug.Log("[FirebaseAuthManager] Đã gửi email xác minh đến: " + email);

                }



                // 3. Liên kết với hồ sơ local để lưu UID và salted hash (không lưu password thô)

                if (LocalAccountManager.Instance != null)

                {

                    LocalAccountManager.Instance.TryLinkFirebaseUser(

                        user.UserId,

                        email,

                        username,

                        emailVerified: false,

                        passwordForOfflineHash: password,

                        out var localAcc,

                        out var linkErr

                    );

                }



                LastUnverifiedUser = user;



                string successMsg = GetText(

                    "Đăng ký thành công! Đã gửi email xác minh tới " + email + ". Vui lòng kiểm tra hộp thư (cả mục Spam) và nhấp vào liên kết để kích hoạt tài khoản trước khi đăng nhập.",

                    "Registration successful! A verification email was sent to " + email + ". Please check your inbox (including Spam folder) and click the link to activate before logging in."

                );



                onComplete?.Invoke(true, successMsg);

            });

        }



        /// <summary>

        /// Đăng nhập bằng Email và Mật khẩu qua Firebase Authentication.

        /// Tải lại trạng thái từ Firebase để kiểm tra IsEmailVerified.

        /// Nếu mất mạng, tự động đối chiếu hồ sơ local đã từng xác minh trên thiết bị này.

        /// </summary>

        public void SignInWithEmailAndPassword(

            string email,

            string password,

            Action<bool, bool, string> onComplete)

        {

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))

            {

                onComplete?.Invoke(false, false, GetText("Vui lòng điền Email và Mật khẩu.", "Please enter Email and Password."));

                return;

            }



            email = email.Trim();



            // Nếu thiết bị đang ngoại tuyến hoặc Firebase chưa sẵn sàng, thử đăng nhập Offline

            if (Application.internetReachability == NetworkReachability.NotReachable || Auth == null)

            {

                TryOfflineFallback(email, password, onComplete, wasOfflineFromStart: true);

                return;

            }



            Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>

            {

                if (task.IsFaulted || task.IsCanceled)

                {

                    // Nếu lỗi do mạng, thử fallback đăng nhập offline cho tài khoản đã xác minh

                    if (IsNetworkError(task.Exception))

                    {

                        TryOfflineFallback(email, password, onComplete, wasOfflineFromStart: false);

                        return;

                    }



                    string err = GetFriendlyErrorMessage(task.Exception);

                    Debug.LogError($"[FirebaseAuthManager] Đăng nhập thất bại: {err}");

                    onComplete?.Invoke(false, false, err);

                    return;

                }



                var user = task.Result.User;



                // Tải lại trạng thái người dùng từ máy chủ để lấy giá trị IsEmailVerified mới nhất

                user.ReloadAsync().ContinueWithOnMainThread(reloadTask =>

                {

                    if (reloadTask.IsFaulted)

                    {

                        Debug.LogWarning("[FirebaseAuthManager] ReloadAsync thất bại, sử dụng trạng thái hiện thời: " + reloadTask.Exception?.Message);

                    }



                    bool isVerified = user.IsEmailVerified;

                    Debug.Log($"[FirebaseAuthManager] Đăng nhập thành công. UID: {user.UserId}, Email: {user.Email}, Verified: {isVerified}");



                    if (!isVerified)

                    {

                        // Chặn vào game nếu email chưa xác minh

                        LastUnverifiedUser = user;

                        string unverifiedMsg = GetText(

                            "Email chưa được xác minh! Vui lòng mở hộp thư email (hoặc thư rác) và bấm liên kết kích hoạt, sau đó đăng nhập lại.",

                            "Email not verified! Please open your email inbox (or spam) and click the activation link, then try logging in again."

                        );

                        onComplete?.Invoke(false, false, unverifiedMsg);

                        return;

                    }



                    // Email đã xác minh: Liên kết hồ sơ local và chuyển file save sang theo UID/accountId

                    if (LocalAccountManager.Instance != null)

                    {

                        bool linkOk = LocalAccountManager.Instance.TryLinkFirebaseUser(

                            user.UserId,

                            email,

                            user.DisplayName,

                            emailVerified: true,

                            passwordForOfflineHash: password,

                            out var localAcc,

                            out var linkErr

                        );



                        if (!linkOk)

                        {

                            Debug.LogError($"[FirebaseAuthManager] Lỗi liên kết hồ sơ local: {linkErr}");

                            onComplete?.Invoke(false, true, linkErr);

                            return;

                        }

                    }



                    LastUnverifiedUser = null;

                    string successMsg = GetText("Đăng nhập thành công!", "Login successful!");

                    onComplete?.Invoke(true, true, successMsg);

                });

            });

        }



        private void TryOfflineFallback(

            string email,

            string password,

            Action<bool, bool, string> onComplete,

            bool wasOfflineFromStart)

        {

            string msg = string.Empty;

            PlayerAccount account = null;

            bool success = LocalAccountManager.Instance != null &&

                LocalAccountManager.Instance.TryOfflineLogin(email, password, out account, out msg);



            if (success)

            {

                string note = wasOfflineFromStart

                    ? GetText("Đang chơi ở chế độ Ngoại tuyến (Đã xác minh trước đó trên thiết bị này).",

                              "Playing in Offline mode (Previously verified on this device).")

                    : GetText("Mất kết nối mạng. Đã chuyển sang chế độ Ngoại tuyến an toàn.",

                              "Network lost. Switched to secure Offline mode.");

                onComplete?.Invoke(true, true, note);

            }

            else

            {

                string errMsg = string.IsNullOrEmpty(msg)

                    ? GetText("Không có kết nối mạng. Tài khoản mới hoặc chưa từng xác minh trực tuyến trên thiết bị này không thể đăng nhập ngoại tuyến.",

                              "No internet connection. New accounts or accounts not yet verified online cannot log in offline.")

                    : msg;

                onComplete?.Invoke(false, false, errMsg);

            }

        }



        /// <summary>

        /// Gửi lại email xác minh cho người dùng đang chờ

        /// </summary>

        public void ResendVerificationEmail(Action<bool, string> onComplete)

        {

            var user = Auth?.CurrentUser ?? LastUnverifiedUser;

            if (user == null)

            {

                onComplete?.Invoke(false, GetText("Không có tài khoản nào đang chờ xác minh trên phiên này.", "No pending account to resend verification."));

                return;

            }



            user.SendEmailVerificationAsync().ContinueWithOnMainThread(task =>

            {

                if (task.IsFaulted || task.IsCanceled)

                {

                    string err = GetFriendlyErrorMessage(task.Exception);

                    onComplete?.Invoke(false, GetText($"Không thể gửi lại email xác minh: {err}", $"Could not resend verification email: {err}"));

                }

                else

                {

                    onComplete?.Invoke(true, GetText("Đã gửi lại email xác minh! Vui lòng kiểm tra hộp thư của bạn.", "Verification email resent! Please check your inbox."));

                }

            });

        }



        /// <summary>

        /// Gửi email đặt lại mật khẩu bằng Firebase

        /// </summary>

        public void SendPasswordResetEmail(string email, Action<bool, string> onComplete)

        {

            if (string.IsNullOrWhiteSpace(email))

            {

                onComplete?.Invoke(false, GetText("Vui lòng nhập Email để nhận hướng dẫn khôi phục!", "Please enter Email to receive recovery instructions!"));

                return;

            }



            if (Auth == null)

            {

                onComplete?.Invoke(false, GetText("Hệ thống xác thực chưa sẵn sàng. Vui lòng kiểm tra kết nối mạng.", "Authentication system is not ready. Please check network."));

                return;

            }



            email = email.Trim();

            Auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>

            {

                if (task.IsFaulted || task.IsCanceled)

                {

                    string err = GetFriendlyErrorMessage(task.Exception);

                    onComplete?.Invoke(false, err);

                }

                else

                {

                    string msg = GetText(

                        $"Đã gửi hướng dẫn đặt lại mật khẩu đến '{email}'. Vui lòng kiểm tra hộp thư!",

                        $"Password reset instructions sent to '{email}'. Please check your inbox!"

                    );

                    onComplete?.Invoke(true, msg);

                }

            });

        }



        /// <summary>

        /// Đăng xuất khỏi Firebase Auth và ngắt phiên local

        /// </summary>

        public void SignOut()

        {

            try

            {

                Auth?.SignOut();

            }

            catch (Exception ex)

            {

                Debug.LogWarning("[FirebaseAuthManager] Ngoại lệ khi đăng xuất: " + ex.Message);

            }



            LastUnverifiedUser = null;

            LocalAccountManager.Instance?.Logout();

            SaveLoadManager.Instance?.SetCurrentAccount(null);

        }



        public static bool IsNetworkError(Exception ex)

        {

            if (ex == null) return false;

            string s = ex.ToString();

            return s.Contains("NetworkRequestFailed") ||

                   s.Contains("SocketException") ||

                   s.Contains("WebException") ||

                   s.Contains("NameResolutionFailure") ||

                   s.Contains("ConnectFailure");

        }



        /// <summary>

        /// Dịch mã lỗi Firebase Auth thành thông báo thân thiện và dễ hiểu cho người chơi

        /// </summary>

        public static string GetFriendlyErrorMessage(Exception exception)

        {

            if (exception == null) return "Đã xảy ra lỗi không xác định.";



            FirebaseException fbEx = null;

            if (exception is AggregateException agg)

            {

                foreach (var inner in agg.Flatten().InnerExceptions)

                {

                    if (inner is FirebaseException fbe)

                    {

                        fbEx = fbe;

                        break;

                    }

                }

            }

            else if (exception is FirebaseException fe)

            {

                fbEx = fe;

            }



            if (fbEx != null)

            {

                AuthError code = (AuthError)fbEx.ErrorCode;

                switch (code)

                {

                    case AuthError.EmailAlreadyInUse:

                        return GetText("Email này đã được sử dụng bởi một tài khoản khác.", "This email is already in use by another account.");

                    case AuthError.InvalidEmail:

                        return GetText("Địa chỉ email không đúng định dạng.", "Invalid email address format.");

                    case AuthError.WrongPassword:

                    case AuthError.InvalidCredential:

                        return GetText("Email hoặc mật khẩu không chính xác.", "Incorrect email or password.");

                    case AuthError.UserNotFound:

                        return GetText("Tài khoản không tồn tại. Vui lòng đăng ký tài khoản mới.", "Account not found. Please register a new account.");

                    case AuthError.WeakPassword:

                        return GetText("Mật khẩu quá yếu. Vui lòng chọn mật khẩu từ 6 ký tự trở lên.", "Weak password. Please use at least 6 characters.");

                    case AuthError.UserDisabled:

                        return GetText("Tài khoản này đã bị vô hiệu hóa. Vui lòng liên hệ hỗ trợ.", "This account has been disabled. Please contact support.");

                    case AuthError.TooManyRequests:

                        return GetText("Quá nhiều lần thử không thành công. Vui lòng thử lại sau vài phút.", "Too many failed attempts. Please try again later.");

                    case AuthError.NetworkRequestFailed:

                        return GetText("Không thể kết nối đến máy chủ. Vui lòng kiểm tra đường truyền mạng.", "Cannot connect to server. Please check your network connection.");

                    case AuthError.OperationNotAllowed:

                        return GetText("Phương thức đăng nhập bằng Email/Password chưa được kích hoạt trên Firebase Console.", "Email/Password sign-in method is not enabled in Firebase Console.");

                    default:

                        return GetText($"Lỗi xác thực: {code}", $"Authentication error: {code}");

                }

            }



            return exception.GetBaseException().Message;

        }



        private static string GetText(string vi, string en)

        {

            return (LocalizationManager.CurrentLanguage == GameLanguage.Vietnamese) ? vi : en;

        }

    }

}

