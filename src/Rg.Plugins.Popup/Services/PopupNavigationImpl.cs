using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Rg.Plugins.Popup.Services
{
    internal class PopupNavigationImpl : IPopupNavigation
    {
        private readonly List<PopupPage> _popupStack = new List<PopupPage>();

        private IPopupPlatform PopupPlatform
        {
            get
            {
                var popupPlatform = DependencyService.Get<IPopupPlatform>();

                if (popupPlatform == null)
                    throw new InvalidOperationException("You MUST install Rg.Plugins.Popup to each project and call Rg.Plugins.Popup.Popup.Init(); prior to using it.\nSee more info: " + Config.InitializationDescriptionUrl);

                if (!popupPlatform.IsInitialized)
                    throw new InvalidOperationException("You MUST call Rg.Plugins.Popup.Popup.Init(); prior to using it.\nSee more info: " + Config.InitializationDescriptionUrl);

                return popupPlatform;
            }
        }

        public IReadOnlyList<PopupPage> PopupStack => _popupStack;

        public PopupNavigationImpl()
        {
            PopupPlatform.OnInitialized += OnInitialized;
        }

        private async void OnInitialized(object sender, EventArgs e)
        {
            if (PopupStack.Any())
                await PopAllAsync(false);
        }

        public async Task PushAsync(PopupPage page, bool animate = true)
        {
            lock (_popupStack)
            {
                if (_popupStack.Contains(page))
                {
                    return;
                }

                _popupStack.Add(page);
                page.IsBeingAppear = true;
            }
            try
            {
                animate = CanBeAnimated(animate);
                if (animate)
                {
                    page.PreparingAnimation();
                }
                await AddAsync(page);
                if (animate)
                {
                    await page.AppearingAnimation();
                }
            }
            finally
            {
                page.IsBeingAppear = false;
            }
        }

        public Task PopAsync(bool animate = true)
        {
            animate = CanBeAnimated(animate);

            if (PopupStack.Count == 0)
                return null;
            return RemovePageAsync(PopupStack.Last(), animate);
        }

        public async Task PopAllAsync(bool animate = true)
        {
            animate = CanBeAnimated(animate);

            var popupTasks = _popupStack.ToList().Select(page => RemovePageAsync(page, animate));

            await Task.WhenAll(popupTasks);
        }

        public async Task RemovePageAsync(PopupPage page, bool animate = true)
        {
            lock (_popupStack)
            {
                if (!_popupStack.Contains(page))
                {
                    return;
                }

                _popupStack.Remove(page);
                page.IsBeingDismissed = true;

            }
            while (page.IsBeingAppear)
            {
                await Task.Delay(50);
            }

            animate = CanBeAnimated(animate);

            try
            {
                if (animate)
                    await page.DisappearingAnimation();

                await RemoveAsync(page);

                if (animate)
                    page.DisposingAnimation();
            }
            finally
            {
                page.IsBeingDismissed = false;
            }
        }

        // Private

        private async Task AddAsync(PopupPage page)
        {
            await PopupPlatform.AddAsync(page);
        }

        private async Task RemoveAsync(PopupPage page)
        {
            await PopupPlatform.RemoveAsync(page);
        }

        // Internal 

        internal void RemovePopupFromStack(PopupPage page)
        {
            if (_popupStack.Contains(page))
                _popupStack.Remove(page);
        }

        #region Animation

        private bool CanBeAnimated(bool animate)
        {
            return animate && PopupPlatform.IsSystemAnimationEnabled;
        }

        #endregion
    }
}
