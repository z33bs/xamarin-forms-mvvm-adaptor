using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;

namespace WordJumble.ViewModels
{
    public class MainViewModel : XamarinFormsMvvmAdaptor.AdaptorViewModel
    {
#if WITH_DI
        readonly INavController navController;
#endif

#if WITH_DI
        public MainViewModel(INavController navController)
        {
            this.navController = navController;
        }
#else
        public MainViewModel()
        {
        }
#endif

        const string DEFAULT_INSTRUCTION = "Enter a four letter word:";
        string instruction = DEFAULT_INSTRUCTION;
        public string Instruction
        {
            get => instruction;
            set => SetProperty(ref instruction, value);
        }

        string word;
        public string Word
        {
            get => word;
            set => SetProperty(ref word, value);
        }

        public Command JumbleWordCommand => new Command<string>(execute: async (obj) => await JumbleWord(obj));
        //, canExecute: (string arg) => arg.Length == 4);
        async Task JumbleWord(string word)
        {
            if (IsBusy || word.Length != 4)
            {
                var stub = " _";
                Instruction =
                    "Finish the word please: "
                    + word
                    + string.Concat(Enumerable.Repeat(stub, 4 - word.Length));
                return;
            }
            
            IsBusy = true;
#if WITH_DI
            await navController.DiPushAsync(App.DiContainer.Resolve<JumbleViewModel>(), word);
#else
            await App.NavController.PushAsync<JumbleViewModel>(word);
#endif
            Instruction = DEFAULT_INSTRUCTION;
            Word = string.Empty;
            IsBusy = false;
        }
    }
}
