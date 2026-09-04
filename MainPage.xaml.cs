using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace gbs2xb
{
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer EnableAudioTimer = new DispatcherTimer();

        string EnableAudioScript = "";
        string WindowActivatedScript = "";
        string WindowDeactivatedScript = "";

        public MainPage()
        {
            // Initialize the XAML and other things
            InitializeComponent();

            // Set the default background color of the WebView2 to black just to avoid seeing white flases. It's an ARGB value
            Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "FF202850");

            // Wait for the UI to be loaded
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Load the JavaScripts. It's a little bit slower, but they're in separate files so we can use the JS Language Server
            EnableAudioScript = await LoadStringFromFile(new Uri("ms-appx:///Js/enableaudio.js"));
            WindowActivatedScript = await LoadStringFromFile(new Uri("ms-appx:///Js/windowactivated.js"));
            WindowDeactivatedScript = await LoadStringFromFile(new Uri("ms-appx:///Js/windowdeactivated.js"));

            // Enable the timer that will try to enable the audio of the GB Studio emulator
            EnableAudioTimer.Interval = TimeSpan.FromSeconds(1);
            EnableAudioTimer.Tick += EnableAudioTimer_Tick;

            // Listen for the keyboard events for the entire window. Listening from other places may cause to not triggering anything since they may require to be focused
            CoreWindow.GetForCurrentThread().KeyDown += CoreWindow_KeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += CoreWindow_KeyUp;

            // Override the default action for the B button on Xbox. If we don't do this, the game will close as soon the player presses the B button on the Xbox controller
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Listen to the Activated event of the Main Window. There we'll do things to avoid unwanted inputs while some popups are open, especially on Xbox consoles
            CoreWindow.GetForCurrentThread().Activated += MainPage_Activated;

            // Ensure the CoreWebView
            await UiWebView2.EnsureCoreWebView2Async();

            // Some settings that may or may not be useful. Usually these are enabled to avoid showing popups or things that may unexpectedly interrupt the gameplay.
            UiWebView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            UiWebView2.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            UiWebView2.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            UiWebView2.CoreWebView2.Settings.IsStatusBarEnabled = false;
            UiWebView2.CoreWebView2.Settings.IsReputationCheckingRequired = false;
            UiWebView2.CoreWebView2.Settings.AreDevToolsEnabled = true;
            UiWebView2.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;

            // ➡️ Replace "mygb.game" string with what you want. Do not leave this string to avoid conflicts with other games that uses this project
            string virtualHostName = "mygb.game";

            // Create a virtual local server for the game. 
            UiWebView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
                virtualHostName,
                Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Game"),
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            // Set the source of the WebView2. We'll use the same string as the one used in SetVirtualHostNameToFolderMapping
            UiWebView2.Source = new Uri($"https://{virtualHostName}/index.html");

            // Start the timer that will auto-enable the audio without requiring user input.
            EnableAudioTimer.Start();

            // We've loaded everything. Disable the Progress Ring to slightly improve performances
            UiLoading.Visibility = Visibility.Collapsed;
            UiLoading.IsActive = false;
        }

        /// <summary>
        /// Load a file into a string
        /// </summary>
        /// <param name="uri">The Uri of the file. In UWP, ms-appx:/// will refer to the files in the package. You can use direct File System paths</param>
        /// <returns></returns>
        async Task<string> LoadStringFromFile(Uri uri)
        {
            StorageFile storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            return await FileIO.ReadTextAsync(storageFile);
        }

        /// <summary>
        /// Run some scripts when this page gets activated or deactivated. Pressing the Xbox Button will display some menus, triggering this event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MainPage_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            // Do this only if the WebView2 is ready and initialized
            if (UiWebView2?.CoreWebView2 != null)
            {
                if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
                {
                    // Focus the window
                    this.Focus(FocusState.Programmatic);

                    // Run the de-activation script
                    await UiWebView2.ExecuteScriptAsync(WindowDeactivatedScript);
                }
                else
                {
                    // Focus the WebView2
                    UiWebView2.Focus(FocusState.Programmatic);

                    // Run the activation script
                    await UiWebView2.ExecuteScriptAsync(WindowActivatedScript);
                }
            }
        }

        /// <summary>
        /// Event triggered when the "Back" key is pressed. This is triggered on Xbox when the player presses the B button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnBackRequested(object? sender, BackRequestedEventArgs e)
        {
            // Don't go back. We need the B button for gameplay purposes
            e.Handled = true;
        }

        /// <summary>
        /// This function will try to enable the audio of the GB Studio file as soon as the emulator is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EnableAudioTimer_Tick(object? sender, object e)
        {
            // Create a function to check if the audio of the emulator works
            var isAudioAvailable = await UiWebView2.CoreWebView2.ExecuteScriptAsync(EnableAudioScript);

            // When returns a string "true", the command to enable the audio is issued, so we can stop doing this
            if (isAudioAvailable.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                EnableAudioTimer.Stop();
            }
        }

        /// <summary>
        /// Intercept the input issued to the entire Window. This is for when a button starts being pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            await SendKeyToWebView(args, true);
        }

        /// <summary>
        /// Intercept the output issued to the entire Window. This is for when a button isn't pressed anymore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            await SendKeyToWebView(args, false);
        }

        /// <summary>
        /// Sends a key to the WebView.
        /// </summary>
        /// <param name="args">The same KeyEventArgs of the CoreWindow events</param>
        /// <param name="isKeyUp">If true, it's a KeyUp event</param>
        async Task SendKeyToWebView(KeyEventArgs args, bool isKeyUp)
        {
            // Initialize the parameters
            string? keyJsParam = null;
            string? codeJsParam = null;

            // Check which key is pressed. These are based on the default values of GB Studio.
            // ➡️ Update this part if you're using a different configuration.
            switch (args.VirtualKey)
            {
                // Game Boy button - DPad Up
                case VirtualKey.GamepadDPadUp:
                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.Up:
                case VirtualKey.W:
                    keyJsParam = "ArrowUp";
                    codeJsParam = "ArrowUp";
                    break;

                // Game Boy button - DPad Down
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.Down:
                case VirtualKey.S:
                    keyJsParam = "ArrowDown";
                    codeJsParam = "ArrowDown";
                    break;

                // Game Boy button - DPad Left
                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.Left:
                case VirtualKey.A:
                    keyJsParam = "ArrowLeft";
                    codeJsParam = "ArrowLeft";
                    break;

                // Game Boy button - DPad Right
                case VirtualKey.GamepadDPadRight:
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.Right:
                case VirtualKey.D:
                    keyJsParam = "ArrowRight";
                    codeJsParam = "ArrowRight";
                    break;

                // Game Boy button - A
                case VirtualKey.GamepadB:
                case VirtualKey.Z:
                case VirtualKey.J:
                    keyJsParam = "j";
                    codeJsParam = "j";
                    break;

                // Game Boy button - B
                case VirtualKey.GamepadA:
                case VirtualKey.K:
                case VirtualKey.X:
                    keyJsParam = "k";
                    codeJsParam = "k";
                    break;

                // Game Boy button - Start
                case VirtualKey.GamepadView:
                case VirtualKey.Shift:
                    keyJsParam = "Shift";
                    codeJsParam = "Shift";
                    break;

                // Game Boy button - Select
                case VirtualKey.GamepadMenu:
                case VirtualKey.Enter:
                    keyJsParam = "Enter";
                    codeJsParam = "Enter";
                    break;

                // If you made some modifications to the Binjgb Web Template and added other keys, you can add them here
                /*
                case VirtualKey.GamepadLeftShoulder:
                case VirtualKey.Number1:
                    keyJsParam = "1";
                    codeJsParam = "Digit1";
                    break;
                */

                default:
                    break;
            }

            if (keyJsParam != null)
            {
                // Create the parameter that will define the event that will be triggered in JS
                string eventJsParam = isKeyUp ? "keyup" : "keydown";

                // Run the function
                string jsToExecute = $@"
                    // Create options and trigger the keyboard event in the browser window
                    window.dispatchEvent(new KeyboardEvent('{eventJsParam}', {{
                        key: '{keyJsParam}',
                        code: '{codeJsParam}',
                        keyCode: 0,
                        which: 0,
                        bubbles: true,
                        cancelable: true
                    }}));";

                // Execute the JS
                await UiWebView2.CoreWebView2.ExecuteScriptAsync(jsToExecute);
            }
        }
    }
}
