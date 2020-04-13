using System;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor
{
    /// <summary>
    /// An optional BaseViewModel that implements the <see cref="IBaseViewModel"/> interface
    /// </summary>
    public abstract class BaseViewModel : ObservableObject, IBaseViewModel
    {
        /// <summary>
        /// Runs automatically once the associated page is pushed onto the <see cref="Mvvm.MainStack"/>
        /// </summary>
        /// <param name="navigationData">Any data which could be useful for ViewModel Initialisation</param>
        /// <returns></returns>
        public virtual Task OnViewPushedAsync(object navigationData)
        {
            return Task.FromResult(false);
        }

        public virtual Task OnViewRemovedAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Runs automatically after the associated page appears ontop of the stack
        /// More precisely, if the page is pushed, or if a page above it was popped
        /// </summary>
        /// <returns></returns>
        public virtual Task RefreshStateAsync(object data = null)
        {
            return Task.FromResult(false);
        }

        public virtual void OnViewAppearing(object sender, EventArgs e)
        {
        }

        public virtual void OnViewDisappearing(object sender, EventArgs e)
        {
        }

		#region James Montemagno's helpers
		string? title = string.Empty;

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string? Title
		{
			get => title;
			set => SetProperty(ref title, value);
		}

		string? subtitle = string.Empty;

		/// <summary>
		/// Gets or sets the subtitle.
		/// </summary>
		/// <value>The subtitle.</value>
		public string? Subtitle
		{
			get => subtitle;
			set => SetProperty(ref subtitle, value);
		}

		string? icon = string.Empty;

		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>The icon.</value>
		public string? Icon
		{
			get => icon;
			set => SetProperty(ref icon, value);
		}

		bool isBusy;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is busy.
		/// </summary>
		/// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
		public bool IsBusy
		{
			get => isBusy;
			set
			{
				if (SetProperty(ref isBusy, value))
					IsNotBusy = !isBusy;
			}
		}

		bool isNotBusy = true;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is not busy.
		/// </summary>
		/// <value><c>true</c> if this instance is not busy; otherwise, <c>false</c>.</value>
		public bool IsNotBusy
		{
			get => isNotBusy;
			set
			{
				if (SetProperty(ref isNotBusy, value))
					IsBusy = !isNotBusy;
			}
		}

		bool canLoadMore = true;

		/// <summary>
		/// Gets or sets a value indicating whether this instance can load more.
		/// </summary>
		/// <value><c>true</c> if this instance can load more; otherwise, <c>false</c>.</value>
		public bool CanLoadMore
		{
			get => canLoadMore;
			set => SetProperty(ref canLoadMore, value);
		}


		string? header = string.Empty;

		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		/// <value>The header.</value>
		public string? Header
		{
			get => header;
			set => SetProperty(ref header, value);
		}

		string? footer = string.Empty;

		/// <summary>
		/// Gets or sets the footer.
		/// </summary>
		/// <value>The footer.</value>
		public string? Footer
		{
			get => footer;
			set => SetProperty(ref footer, value);
		}
		#endregion
	}
}
