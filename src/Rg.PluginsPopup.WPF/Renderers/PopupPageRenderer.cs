﻿using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Windows.Renderers;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(PopupPage), typeof(PopupPageRenderer))]
namespace Rg.Plugins.Popup.Windows.Renderers
{
    [Preserve(AllMembers = true)]
    public class PopupPageRenderer : PageRenderer
    {
        private PopupPage CurrentElement => (PopupPage)Element;

        [Preserve]
        public PopupPageRenderer()
        { }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == Page.IsVisibleProperty.PropertyName)
            {
                Control.Visibility = CurrentElement.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        internal void Prepare()
        {
            var grid = GetTopGridFromWindow();

            //Control.Background = Brushes.Transparent;
            Control.Visibility = Visibility.Visible;
            System.Windows.Controls.Panel.SetZIndex(Control, 10000);
            System.Windows.Controls.Grid.SetColumn(Control, 0);
            System.Windows.Controls.Grid.SetColumnSpan(Control, grid.ColumnDefinitions.Count + 1);
            System.Windows.Controls.Grid.SetRow(Control, 1);
            System.Windows.Controls.Grid.SetRowSpan(Control, grid.RowDefinitions.Count);

            grid.Children.Add(Control);

            //grid.UpdateLayout();
            //CurrentElement.ForceLayout();
            Control.LayoutUpdated += OnLayoutUpdated;
            Control.MouseDown += OnBackgroundClick;
        }

        internal void Destroy()
        {
            CurrentElement.IsVisible = false;
            GetTopGridFromWindow().Children.Remove(Control);

            Control.MouseDown -= OnBackgroundClick;
            Control.LayoutUpdated -= OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (CurrentElement.Width != Control.ActualWidth)
            {
                CurrentElement.Layout(new Rectangle(0, 0, Control.ActualWidth, Control.ActualHeight));
            }
        }

        private void OnBackgroundClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == Control || e.OriginalSource is System.Windows.Controls.Border)
            {
                CurrentElement.SendBackgroundClick();
            }
        }

        private System.Windows.Controls.Grid GetTopGridFromWindow()
        {
            return GetChildGrid(System.Windows.Application.Current.MainWindow);
        }

        private System.Windows.Controls.Grid GetChildGrid(DependencyObject content)
        {
            if (content is System.Windows.Controls.Grid grid)
                return grid;
            var count = VisualTreeHelper.GetChildrenCount(content);
            for (int i = 0; i < count; i++)
            {
                grid = GetChildGrid(VisualTreeHelper.GetChild(content, i));
                if (grid != null)
                    return grid;
            }
            return null;
        }


    }
}
