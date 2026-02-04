using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;
public class FirebaseController : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseUser user;
    bool isSignIn = false;

    public GameObject loginPanel, signupPanel, profilePanel ,forgetPasswordPanel, notificationPanel;
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupConfirmPassword, signupUsername, forgetPasswordEmail;
    public Text notificationText, profileNameText, profileEmailText;

    void Start()
    {

        OpenLoginPanel();
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        closeNotif_Panel();
        forgetPasswordPanel.SetActive(false);
    }
    public void OpenSignupPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        profilePanel.SetActive(false);
        closeNotif_Panel();
        forgetPasswordPanel.SetActive(false);
    }
    public void OpenProfilePanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(true);
        closeNotif_Panel();
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenForgetPasswordPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        closeNotif_Panel();
        forgetPasswordPanel.SetActive(true);
    }

    public void LoginUser() { 
        string email = loginEmail.text;
        string password = loginPassword.text;
        
        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
            Debug.Log("Please enter email and password.");
            showNotificationMessage("Please enter email and password.");
            return;
        }
        //Firebase login logic
        SigninUser(email, password);
    }

    public void SignupUser() {
        string email = signupEmail.text;
        string password = signupPassword.text;
        string confirmPassword = signupConfirmPassword.text;
        string username = signupUsername.text;

        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(username)) {
            Debug.Log("Please fill all the fields.");
            showNotificationMessage("Please fill all the fields.");
            return;
        }
        if(password != confirmPassword) {
            Debug.Log("Passwords do not match.");
            showNotificationMessage("Passwords do not match.");
            return;
        }
        //Firebase signup logic
        CreateUser(email, password, username);
    }
    public void ForgetPassword() {
        string email = forgetPasswordEmail.text;
        // Add Firebase forgot password logic here
        if(string.IsNullOrEmpty(email)) {
            Debug.Log("Please enter your email.");
            showNotificationMessage("Please enter your email.");
            return;
        }
        forgetPasswordsSubmit(email);
    }
    private void showNotificationMessage(string message) {
        notificationPanel.SetActive(true);
        notificationText.text = message;
    }
    public void closeNotif_Panel() {
        notificationPanel.SetActive(false);
        notificationText.text = "";
    }

    public void LogoutUser() {
        //Firebase logout logic
        auth.SignOut();
        profileEmailText.text = "";
        profileNameText.text = "";
        OpenLoginPanel();
    }
    public void CreateUser(string email, string password, string username) {
        //Firebase create user logic

        auth = FirebaseAuth.DefaultInstance;
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage(GetErrorMessage(errorCode));
                    }
                }

                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            UpdateUserProfile(username);
        });
    }
    public void SigninUser(string email, string password)
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage(GetErrorMessage(errorCode));
                    }
                }


                return;
            }

            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            profileNameText.text = result.User.DisplayName;
            profileEmailText.text = result.User.Email;
            OpenProfilePanel();
        });
    }
    private static string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "The account already exists with different credentials.";
                break;
            case AuthError.MissingPassword:
                message = "Password is required.";
                break;
            case AuthError.WeakPassword:
                message = "The password is weak.";
                break;
            case AuthError.WrongPassword:
                message = "The password is incorrect.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "The account already exists with that email address.";
                break;
            case AuthError.InvalidEmail:
                message = "Invalid email address.";
                break;
            case AuthError.MissingEmail:
                message = "Email is required.";
                break;
            default:
                message = "An error occurred.";
                break;
        }
        return message;
    }
    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                isSignIn = true;
            }
        }
    }


    public void UpdateUserProfile(string username)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            UserProfile profile = new UserProfile
            {
                DisplayName = username,
                PhotoUrl = new System.Uri("https://hainguyen007.github.io/public-assets/img/quangcaoviet/tick-icon-01.png"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
                showNotificationMessage("Signup successful! You can now log in.");
            });
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    bool isSigned = false;

    private void Update()
    {
        if(isSignIn)
        {
           if(!isSigned)
           {
                isSigned = true;
                profileNameText.text = user.DisplayName;
                profileEmailText.text = user.Email;
                OpenProfilePanel();
           }
        }
    }

    public void forgetPasswordsSubmit(string email)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("SendPasswordResetEmailAsync encountered an error: " + task.Exception);

                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage(GetErrorMessage(errorCode));
                    }
                }

                return;
            }
            Debug.Log("Password reset email sent successfully.");
            showNotificationMessage("Password reset email sent successfully.");
        });
    }
}
