using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Rg.Plugins.Popup.Windows.Renderers;
using Rg.Plugins.Popup.WPF.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using XPlatform = Xamarin.Forms.Platform.WPF.Platform;
//using System.Windows;
//using System.Windows.Controls;

[assembly: Dependency(typeof(PopupPlatformWPF))]
namespace Rg.Plugins.Popup.WPF.Impl
{
    [Preserve(AllMembers = true)]
    class PopupPlatformWPF : IPopupPlatform
    {
        private IPopupNavigation PopupNavigationInstance => PopupNavigation.Instance;
        private HashSet<PopupPage> set = new HashSet<PopupPage>();

        public event EventHandler OnInitialized
        {
            add => Popup.OnInitialized += value;
            remove => Popup.OnInitialized -= value;
        }

        public bool IsInitialized => Popup.IsInitialized;

        public bool IsSystemAnimationEnabled => true;

        [Preserve]
        public PopupPlatformWPF()
        {
        }

        private async void OnBackRequested(object sender, EventArgs e)
        {
            var lastPopupPage = PopupNavigationInstance.PopupStack.LastOrDefault();

            if (lastPopupPage != null)
            {
                var isPrevent = lastPopupPage.IsBeingDismissed || lastPopupPage.SendBackButtonPressed();

                if (!isPrevent)
                {
                    //e.Handled = true;
                    await PopupNavigationInstance.PopAsync();
                }
            }
        }

        public async Task AddAsync(PopupPage page)
        {
            if (set.Contains(page))
                return;

            set.Add(page);

            page.Parent = Application.Current.MainPage;

            var renderer = (PopupPageRenderer)XPlatform.GetOrCreateRenderer(page);
            renderer.Prepare();

            //page.ForceLayout();
            await Task.Delay(5);
        }

        public async Task RemoveAsync(PopupPage page)
        {
            set.Remove(page);
            var renderer = (PopupPageRenderer)XPlatform.GetOrCreateRenderer(page);
            renderer.Destroy();

            Cleanup(page);
            page.Parent = null;
            await Task.Delay(5);
        }

        internal static void Cleanup(VisualElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var elementRenderer = XPlatform.GetRenderer(element);
            foreach (Element descendant in element.Descendants())
            {
                if (descendant is VisualElement child)
                {
                    var childRenderer = XPlatform.GetRenderer(child);
                    if (childRenderer != null)
                    {
                        childRenderer.Dispose();
                        XPlatform.SetRenderer(child, null);
                    }
                }
            }
            if (elementRenderer == null)
                return;

            elementRenderer.Dispose();
            XPlatform.SetRenderer(element, null);
        }
    }
}
