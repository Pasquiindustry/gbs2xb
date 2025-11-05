using System;
using System.IO;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace gbs2xb
{
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer EnableAudioTimer = new DispatcherTimer();

        public MainPage()
        {
            // Initialize the XAML and other things
            InitializeComponent();

            // Wait for the UI to be loaded
            Loaded += MainPage_Loaded;

            // Set the default background color of the WebView2 to black just to avoid seeing white flases. It's an ARGB value
            Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "FF000000");

            // Enable the timer that will try to enable the audio of the GB Studio emulator
            EnableAudioTimer.Interval = TimeSpan.FromSeconds(1);
            EnableAudioTimer.Tick += EnableAudioTimer_Tick;

            // Listen for the keyboard
            CoreWindow.GetForCurrentThread().KeyDown += CoreWindow_KeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += CoreWindow_KeyUp;
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Ensure the CoreWebView
            await UiWebView.EnsureCoreWebView2Async();

            // Some settings
            UiWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            UiWebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            UiWebView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            UiWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            UiWebView.CoreWebView2.Settings.IsReputationCheckingRequired = false;
            UiWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            UiWebView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;

            // Create a local host for the game. Replace "mygbstudio.game" string with what you want
            UiWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "mygbstudio.game",
                Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Game"),
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            // Set the source. In this case there's a launcher. Use the same string as the one used in SetVirtualHostNameToFolderMapping
            UiWebView.Source = new Uri("https://mygbstudio.game/index.html");

            // Start the timer to auto-enable the audio
            EnableAudioTimer.Start();
        }

        /// <summary>
        /// This function will try to enable the audio of the GB Studio file as soon as the emulator is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EnableAudioTimer_Tick(object? sender, object e)
        {
            // Create a function to check if the audio of the emulator works
            var isAudioAvailable = await UiWebView.CoreWebView2.ExecuteScriptAsync("function gbs2xbTryEnableAudio () { if (typeof emulator !== 'undefined' && emulator.audio) {emulator.audio.startPlayback(); return true;} else { return false; }} gbs2xbTryEnableAudio();");

            // When returns a string "true", the command to enable the audio is issued, we can stop doing this
            if (isAudioAvailable.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                EnableAudioTimer.Stop();
            }
        }

        async void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            await SendKeyToWebView(args, true);
        }

        async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            await SendKeyToWebView(args, false);
        }

        /// <summary>
        /// Send a key to the WebView.
        /// </summary>
        /// <param name="args">The same KeyEventArgs of the CoreWindow events</param>
        /// <param name="isKeyUp">If true, it's a KeyUp event</param>
        async Task SendKeyToWebView(KeyEventArgs args, bool isKeyUp)
        {
            // Initialize the parameters
            string? keyJsParam = null;
            string? codeJsParam = null;

            // Check which key is pressed. These are based on the default values of GB Studio.
            // Note that the B button on the gamepad won't currently work
            switch (args.VirtualKey)
            {
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.Down:
                case VirtualKey.S:
                    keyJsParam = "ArrowDown";
                    codeJsParam = "ArrowDown";
                    break;

                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.Left:
                case VirtualKey.A:
                    keyJsParam = "ArrowLeft";
                    codeJsParam = "ArrowLeft";
                    break;

                case VirtualKey.GamepadDPadRight:
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.Right:
                case VirtualKey.D:
                    keyJsParam = "ArrowRight";
                    codeJsParam = "ArrowRight";
                    break;

                case VirtualKey.GamepadDPadUp:
                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.Up:
                case VirtualKey.W:
                    keyJsParam = "ArrowUp";
                    codeJsParam = "ArrowUp";
                    break;

                case VirtualKey.GamepadA:
                case VirtualKey.Z:
                case VirtualKey.J:
                    keyJsParam = "j";
                    codeJsParam = "j";
                    break;

                case VirtualKey.GamepadX:
                case VirtualKey.K:
                case VirtualKey.X:
                    keyJsParam = "k";
                    codeJsParam = "k";
                    break;

                case VirtualKey.GamepadView:
                case VirtualKey.Shift:
                    keyJsParam = "Shift";
                    codeJsParam = "Shift";
                    break;

                case VirtualKey.GamepadMenu:
                case VirtualKey.Enter:
                    keyJsParam = "Enter";
                    codeJsParam = "Enter";
                    break;
            }

            // Create the parameter that will define the event that will be triggered in JS
            string eventJsParam = isKeyUp ? "keyup" : "keydown";

            // Create the JS to execute
            string jsToExecute = $@"(function(){{
                var evtOptions = {{ key: '{keyJsParam}', code: '{codeJsParam}', keyCode: 0, which: 0, bubbles: true, cancelable: true }};
                window.dispatchEvent(new KeyboardEvent('{eventJsParam}', evtOptions));
            }})();";

            // Execute the JS
            await UiWebView.CoreWebView2.ExecuteScriptAsync(jsToExecute);
        }
    }
}
