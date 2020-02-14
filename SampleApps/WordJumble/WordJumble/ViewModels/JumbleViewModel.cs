using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers;
using WordJumble.Models;
using WordJumble.Services;
using Xamarin.Forms;

namespace WordJumble.ViewModels
{
    public class JumbleViewModel : XamarinFormsMvvmAdaptor.AdaptorViewModel
    {
        readonly FlexiCharGeneratorService flexiCharGenerator;

        public JumbleViewModel()
        {
            flexiCharGenerator = new FlexiCharGeneratorService();
        }

        public override async Task InitializeAsync(object navigationData)
        {
            //foreach (var character in navigationData as string)
            //{
            //    FlexiChars.Add(flexiCharGenerator.GetRandomFlexiChar(character));
            //    await Task.Delay(100).ConfigureAwait(false);
            //    OnPropertyChanged(nameof(FlexiChars));
            //}
            word = navigationData as string;
            await DrawWord().ConfigureAwait(false);
        }

        public Task OnAppearing(object data)
        {
            OnPropertyChanged(nameof(Flexi0));
            OnPropertyChanged(nameof(Flexi1));
            OnPropertyChanged(nameof(Flexi2));
            OnPropertyChanged(nameof(Flexi3));
            return Task.FromResult(false);
        }

        Task Pause()
        {
            return Task.Delay(Constants.DRAWING_DELAY_MS);
        }

        string word;

        async Task DrawWord()
        {
            var characters = word.ToCharArray();
            Flexi0 = flexiCharGenerator.GetRandomFlexiChar(characters[0]);
            await Pause().ConfigureAwait(false);
            Flexi1 = flexiCharGenerator.GetRandomFlexiChar(characters[1]);
            await Pause().ConfigureAwait(false);
            Flexi2 = flexiCharGenerator.GetRandomFlexiChar(characters[2]);
            await Pause().ConfigureAwait(false);
            Flexi3 = flexiCharGenerator.GetRandomFlexiChar(characters[3]);
            await Pause().ConfigureAwait(false);
        }


        public int GridMargin { get; } = (Constants.FONTSIZE_MAX - Constants.FONTSIZE_MIN)/2;

        FlexiChar flexi0;
        public FlexiChar Flexi0
        {
            get => flexi0;
            set => SetProperty(ref flexi0, value);
        }

        FlexiChar flexi1;
        public FlexiChar Flexi1
        {
            get => flexi1;
            set => SetProperty(ref flexi1, value);
        }

        FlexiChar flexi2;
        public FlexiChar Flexi2
        {
            get => flexi2;
            set => SetProperty(ref flexi2, value);
        }

        FlexiChar flexi3;
        public FlexiChar Flexi3
        {
            get => flexi3;
            set => SetProperty(ref flexi3, value);
        }

        public ICommand RedrawCommand => new Command(async () => await DrawWord());
        public ICommand ClosePageCommand => new Command(
            async () =>
            {
                await App.MainNavController.Navigation.PopAsync();
            });
        public ICommand LaunchDetailCommand => new Command<FlexiChar>(
            async (flexiChar) =>
            {
                await App.MainNavController.PushModalAsync<FlexiCharDetailViewModel>(flexiChar);
            });
    }
}
