using main_interface;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


namespace main_interface
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private MySplashScreen splash;

       // private Window? _window;

        public MainWindow main_window { get; private set; }




        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd, int id, int fsModifiers, int vk);
        // Parameters
        // hwnd = handle window , 
        // id = the id of the identified hotkey ,
        // hsModifiers = modifier keys eg., control 
        // virtual key code 



        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        /// 


        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            splash = new MySplashScreen();
            splash.Activate();





            //Startup taks ill put here first i want to stimulate delay 
            Task.Delay(3000).ContinueWith(t => // args
            {
                splash.DispatcherQueue.TryEnqueue(() =>
                {
                    main_window = new MainWindow(); // then actually go onto main window  - first init to an instance of such 
                    main_window.Activate();
                    var hwnd = WindowNative.GetWindowHandle(main_window); // Extacts the win32 hwnd from the winui window 

                    splash.Close();
                });

            });

        } // onLanch end
    }// partialclass end
}
// namespace end 





/*
 * 
 * 
namespace main_interface
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {

            _window = new MainWindow();
            _window.Activate();
        }
    }
}

*/