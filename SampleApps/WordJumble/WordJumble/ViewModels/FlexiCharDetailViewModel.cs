using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WordJumble.Models;
using Xamarin.Forms;

namespace WordJumble.ViewModels
{
    public class FlexiCharDetailViewModel : XamarinFormsMvvmAdaptor.AdaptorViewModel
    {
        readonly Random random = new Random();

        FlexiChar flexiChar;
        public FlexiChar FlexiChar
        {
            get => flexiChar;
            set => SetProperty(ref flexiChar, value);
        }
        public FlexiCharDetailViewModel()
        {
        }

        public override Task InitializeAsync(object navigationData)
        {
            FlexiChar = navigationData as FlexiChar;
            return base.InitializeAsync(navigationData);
        }

        public ICommand PopPageCommand
            => new Command(
                async () =>
                    await App.MainNavController.PopModalAsync());

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
