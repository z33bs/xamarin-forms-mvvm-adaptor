using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using WordJumble.Models;
using WordJumble.Services;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;

namespace WordJumble.ViewModels
{
    public class JumbleViewModel : XamarinFormsMvvmAdaptor.AdaptorViewModel
    {
        readonly IFlexiCharGeneratorService flexiCharGenerator;
#if WITH_DI
        readonly INavController navController;
#endif

#if WITH_DI
        public JumbleViewModel(INavController navController, IFlexiCharGeneratorService flexiCharGeneratorService)
        {
            flexiCharGenerator = flexiCharGeneratorService;
            this.navController = navController;
        }
#else
        public JumbleViewModel()
        {
            flexiCharGenerator = new FlexiCharGeneratorService();
        }
#endif

        public override async Task InitializeAsync(object navigationData)
        {
            Title = "Jumble";
            word = navigationData as string;
            await DrawWord().ConfigureAwait(false);
        }

        public override Task OnAppearingAsync()
        {
            OnPropertyChanged(nameof(Flexi0));
            OnPropertyChanged(nameof(Flexi1));
            OnPropertyChanged(nameof(Flexi2));
            OnPropertyChanged(nameof(Flexi3));

            return base.OnAppearingAsync();
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


        public int GridMargin { get; } = (Constants.FONTSIZE_MAX - Constants.FONTSIZE_MIN) / 2;

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

        public ICommand RedrawCommand => new Command(
            async () => await DrawWord());

        public ICommand ClosePageCommand => new Command(
            async () =>
            {
#if WITH_DI
                await navController.PopAsync();
#else
                await App.NavController.PopAsync();
#endif
            });

        public ICommand LaunchDetailCommand => new Command<FlexiChar>(
            async (flexiChar) =>
            {
#if WITH_DI
                await navController.PushModalAsync(App.DiContainer.Resolve<FlexiCharDetailViewModel>(),flexiChar);
#else
                await App.NavController.PushModalAsync<FlexiCharDetailViewModel>(flexiChar);
#endif
            });
    }
}
