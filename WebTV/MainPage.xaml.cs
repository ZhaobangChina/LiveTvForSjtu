using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace WebTV
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ViewModels.MainPageVM vm;

        bool isControlsVisible = true;

        DispatcherTimer hideCursorTimer = new DispatcherTimer();
        CoreCursor previousCursor = null;

        public MainPage()
        {
            vm = new ViewModels.MainPageVM(Dispatcher);
            this.InitializeComponent();

            hideCursorTimer.Interval = TimeSpan.FromSeconds(3);
            hideCursorTimer.Tick += HideCursorTimer_Tick;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync("MainPageVM.xml");
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    ViewModels.MainPageVMState state = new ViewModels.MainPageVMState
                    {
                        Channels = null,
                        SelectedIndex = null
                    };
                    await Task.Run(() =>
                    {
                        try
                        {
                            var serializer = new DataContractSerializer(typeof(ViewModels.MainPageVMState));
                            state = (ViewModels.MainPageVMState)serializer.ReadObject(stream);
                        }
                        catch
                        {
                            state.Channels = null;
                            state.SelectedIndex = null;
                        }
                    });
                    vm.SetState(state);
                }
            }
            catch (FileNotFoundException) { }
            vm.InitializeIfNeeded();
        }

        public async Task SaveStateAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("MainPageVM.xml", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var state = vm.GetState();
                var serializer = new DataContractSerializer(state.GetType());
                serializer.WriteObject(stream, state);
            }
        }

        private void ToggleFullScreen()
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
            else
            {
                if (ApplicationView.GetForCurrentView().TryEnterFullScreenMode())
                {
                    HideCursor();
                }
            }
        }

        private void HideCursor()
        {
            if (Window.Current.CoreWindow.PointerCursor != null)
            {
                previousCursor = Window.Current.CoreWindow.PointerCursor;
                Window.Current.CoreWindow.PointerCursor = null;
            }
        }

        private void ShowCursor()
        {
            if (Window.Current.CoreWindow.PointerCursor == null)
            {
                if (previousCursor != null)
                    Window.Current.CoreWindow.PointerCursor = previousCursor;
                else
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullScreen();
        }

        private void MediaPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
            ToggleFullScreen();
        }

        private void MediaPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (isControlsVisible)
                VisualStateManager.GoToState(this, nameof(controlsInvisibleState), true);
            else
                VisualStateManager.GoToState(this, nameof(controlsVisibleState), true);
            isControlsVisible = !isControlsVisible;
        }

        private void HideCursorTimer_Tick(object sender, object e)
        {
            hideCursorTimer.Stop();
            HideCursor();
        }

        private void MediaPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ShowCursor();
            hideCursorTimer.Start();
        }

        private void MediaPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ShowCursor();
            hideCursorTimer.Stop();
        }
    }
}
