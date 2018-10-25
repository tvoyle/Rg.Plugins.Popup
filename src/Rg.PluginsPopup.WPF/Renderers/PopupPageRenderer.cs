using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Windows.Renderers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.WPF;
using WinPopup = global::System.Windows.Window;

[assembly: ExportRenderer(typeof(PopupPage), typeof(PopupPageRenderer))]
namespace Rg.Plugins.Popup.Windows.Renderers
{
    [Preserve(AllMembers = true)]
    public class PopupPageRenderer : PageRenderer
    {
        internal WinPopup Container { get; private set; }

        private PopupPage CurrentElement => (PopupPage)Element;

        [Preserve]
        public PopupPageRenderer()
        {

        }

        public FrameworkElement ContainerElement
        {

            get { return Control; }
        }

        internal void Prepare(WinPopup container)
        {
            Container = container;
            Container.WindowStyle = WindowStyle.None;
            Container.AllowsTransparency = true;
            Container.Owner = System.Windows.Application.Current.MainWindow;
            Container.Background = Brushes.Transparent;
            Container.Content = ContainerElement;
            Container.Show();
            Container.Closing += OnClosing;

            System.Windows.Application.Current.MainWindow.MouseDown += OnClosing;
            System.Windows.Application.Current.MainWindow.LocationChanged += OnLocationChanged;
            System.Windows.Application.Current.MainWindow.SizeChanged += OnSizeChanged;

            ContainerElement.MouseDown += OnBackgroundClick;
            UpdateElementSize();
        }

        private void OnClosing(object sender, EventArgs e)
        {
            CurrentElement.SendBackgroundClick();
        }

        internal void Destroy()
        {
            Container.Owner = null;
            Container.Closed -= OnClosing;            
            Container = null;

            System.Windows.Application.Current.MainWindow.MouseDown -= OnClosing;
            System.Windows.Application.Current.MainWindow.LocationChanged -= OnLocationChanged;
            System.Windows.Application.Current.MainWindow.SizeChanged -= OnSizeChanged;

            ContainerElement.MouseDown -= OnBackgroundClick;
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            UpdateElementSize();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateElementSize();
        }

        private void OnBackgroundClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == Control || e.OriginalSource is System.Windows.Controls.Border)
            {
                CurrentElement.SendBackgroundClick();
            }
        }

        private async void UpdateElementSize()
        {
            await Task.Delay(50);
            if (Container == null)
                return;
            var window = System.Windows.Application.Current.MainWindow;
            var windowBound = new Rect(window.PointToScreen(new System.Windows.Point(0, 0)), new System.Windows.Size(window.ActualWidth, window.ActualHeight));

            var renderer = Platform.GetRenderer(Xamarin.Forms.Application.Current.MainPage);
            var content = renderer.GetNativeElement();
            content = GetVisibleParent(content);
            var visibleBounds = new Rect(content.PointToScreen(new System.Windows.Point(0, 0)), new System.Windows.Size(content.ActualWidth, content.ActualHeight));
            visibleBounds.Inflate(-6, -6);
            Container.Left = visibleBounds.Left;
            Container.Top = visibleBounds.Top;
            Container.Width = visibleBounds.Width;
            Container.Height = visibleBounds.Height;

            CurrentElement.BatchBegin();
            CurrentElement.Layout(new Rectangle(0, 0, visibleBounds.Width - 1, visibleBounds.Height - 1));
            CurrentElement.Layout(new Rectangle(0, 0, visibleBounds.Width, visibleBounds.Height));
            CurrentElement.BatchCommit();

        }

        private FrameworkElement GetVisibleParent(FrameworkElement content)
        {
            while (content.Parent is FrameworkElement parent && !(parent is System.Windows.Controls.Border) && parent.IsVisible)
            {
                content = parent;
            }
            return content;
        }
    }
}
