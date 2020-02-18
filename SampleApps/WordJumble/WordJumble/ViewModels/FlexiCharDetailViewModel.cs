using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WordJumble.Models;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;

namespace WordJumble.ViewModels
{
    public class FlexiCharDetailViewModel : XamarinFormsMvvmAdaptor.AdaptorViewModel
    {
        readonly Random random = new Random();
#if WITH_DI
        readonly INavController navController;
#endif

#if WITH_DI
        public FlexiCharDetailViewModel(INavController navController)
        {
            this.navController = navController;
        }
#else
        public FlexiCharDetailViewModel()
        {
        }
#endif

        public override Task InitializeAsync(object navigationData)
        {
            FlexiChar = navigationData as FlexiChar;
            return base.InitializeAsync(navigationData);
        }

        FlexiChar flexiChar;
        public FlexiChar FlexiChar
        {
            get => flexiChar;
            set => SetProperty(ref flexiChar, value);
        }

        public ICommand PopPageCommand
            => new Command(async () =>
                {
#if WITH_DI
                    await navController.PopModalAsync();
#else
                    await App.NavController.PopModalAsync();
#endif
                });

        public ICommand RotateCommand
            => new Command(Rotate);
        void Rotate()
        {
            var angle = random.Next(Constants.ROTATION_MIN_ANGLE, Constants.ROTATION_MAX_ANGLE);
            FlexiChar.Rotation = angle;
            OnPropertyChanged(nameof(FlexiChar));
        }
    }
}
