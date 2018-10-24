using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Windows.Renderers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.WPF;
using WinPopup = global::System.Windows.Controls.Primitives.Popup;

[assembly: ExportRenderer(typeof(PopupPage), typeof(PopupPageRenderer))]
namespace Rg.Plugins.Popup.Windows.Renderers
{
    [Preserve(AllMembers = true)]
    public class PopupPageRenderer : PageRenderer
    {
        private Rect _keyboardBounds;
        // private FrameworkElement content;

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

        //private void OnKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        //{
        //    _keyboardBounds = Rect.Empty;
        //    UpdateElementSize();
        //}

        //private void OnKeyboardShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        //{
        //    _keyboardBounds = sender.OccludedRect;
        //    UpdateElementSize();
        //}

        //private void OnOrientationChanged(DisplayInformation sender, object args)
        //{
        //    UpdateElementSize();
        //}

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    UpdateElementSize();

        //    return base.ArrangeOverride(finalSize);
        //}

        internal void Prepare(WinPopup container)
        {
            Container = container;
            Container.AllowsTransparency = true;
            Container.Placement = PlacementMode.Absolute;
            Container.PlacementTarget = System.Windows.Application.Current.MainWindow;
            Container.Child = ContainerElement;
            Container.IsOpen = true;
            Container.Closed += OnContainerIsVisibleChanged;

            System.Windows.Application.Current.MainWindow.LocationChanged += OnLocationChanged;
            System.Windows.Application.Current.MainWindow.SizeChanged += OnSizeChanged;
            //DisplayInformation.GetForCurrentView().OrientationChanged += OnOrientationChanged;

            //InputPane inputPane = InputPane.GetForCurrentView();
            //inputPane.Showing += OnKeyboardShowing;
            //inputPane.Hiding += OnKeyboardHiding;
            ContainerElement.MouseDown += OnBackgroundClick;
            UpdateElementSize();
        }

        private void OnContainerIsVisibleChanged(object sender, EventArgs e)
        {
            CurrentElement.SendBackgroundClick();
        }

        internal void Destroy()
        {
            Container.Closed -= OnContainerIsVisibleChanged;
            Container = null;

            System.Windows.Application.Current.MainWindow.LocationChanged -= OnLocationChanged;
            System.Windows.Application.Current.MainWindow.SizeChanged -= OnSizeChanged;
            //DisplayInformation.GetForCurrentView().OrientationChanged -= OnOrientationChanged;

            //InputPane inputPane = InputPane.GetForCurrentView();
            //inputPane.Showing -= OnKeyboardShowing;
            //inputPane.Hiding -= OnKeyboardHiding;

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

        private void OnBackgroundClick(object sender, RoutedEventArgs e)
        {
            //if (e.OriginalSource == this)
            //{
            CurrentElement.SendBackgroundClick();
            //}
        }

        private async void UpdateElementSize()
        {
            await Task.Delay(50);
            if (Container == null)
                return;
            var window = System.Windows.Application.Current.MainWindow;
            var windowBound = new Rect(window.PointToScreen(new System.Windows.Point(0, 0)), new System.Windows.Size(window.Width, window.Height));

            Container.PlacementRectangle = windowBound;

            var visibleBounds = new Rect(ContainerElement.PointToScreen(new System.Windows.Point(0, 0)), new System.Windows.Size(ContainerElement.Width, ContainerElement.Height));

            var top = visibleBounds.Top - windowBound.Top;
            var bottom = windowBound.Bottom - visibleBounds.Bottom;
            var left = visibleBounds.Left - windowBound.Left;
            var right = windowBound.Right - visibleBounds.Right;

            top = Math.Max(0, top);
            bottom = Math.Max(0, bottom);
            left = Math.Max(0, left);
            right = Math.Max(0, right);

            if (_keyboardBounds != Rect.Empty)
                bottom += _keyboardBounds.Height;

            var systemPadding = new Xamarin.Forms.Thickness(left, top, right, bottom);

            CurrentElement.BatchBegin();
            //CurrentElement.SetSystemPadding(systemPadding);
            CurrentElement.Layout(new Rectangle(0, 0, windowBound.Width - 1, windowBound.Height - 1));
            CurrentElement.Layout(new Rectangle(0, 0, windowBound.Width + 1, windowBound.Height + 1));
            CurrentElement.BatchCommit();

        }
    }
}
