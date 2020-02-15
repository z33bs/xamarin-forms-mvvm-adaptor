using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;

namespace WordJumble.ViewModels
{
    public class MainViewModel : XamarinFormsMvvmAdaptor.AdaptorViewModel
    {

        public MainViewModel()
        {
        }

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
            await App.MainNavController.PushAsync<JumbleViewModel>(word);
            Instruction = DEFAULT_INSTRUCTION;
            Word = string.Empty;
            IsBusy = false;
        }
    }
}
