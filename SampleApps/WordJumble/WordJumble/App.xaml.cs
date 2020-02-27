using Xamarin.Forms;
using XamarinFormsMvvmAdaptor;

//Vanilla
using WordJumble.Views;

//WITH_DI
using Autofac;
using WordJumble.Services;
using WordJumble.ViewModels;

namespace WordJumble
{
    public partial class App : Application
    {
        // Instantiate the controller in a manner that it is available as a single instance
        // anywhere in your app
#if WITH_DI
        // In this example we're going DI all the way!
        // The NavController is put into this DiContainer
        public static IContainer DiContainer { get; private set; }
#else
        public static NavController NavController { get; } = new NavController();
#endif

        public App()
        {
            InitializeComponent();

#if WITH_DI
            // Set up the container for Dependency Injection
            var builder = new ContainerBuilder();
            // Add services
            builder.RegisterType<FlexiCharGeneratorService>().As<IFlexiCharGeneratorService>().SingleInstance();
            // Add view-models
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<JumbleViewModel>().AsSelf();
            builder.RegisterType<FlexiCharDetailViewModel>().AsSelf();
            // Add the NavController
            builder.RegisterType<NavController>().As<INavController>().SingleInstance();
            DiContainer = builder.Build();
#endif
        }

        protected override async void OnStart()
        {
            //Important to Initialize the NavController before setting MainPage
#if WITH_DI
            var navController = DiContainer.Resolve<INavController>();
            await navController.DiInitAsync(DiContainer.Resolve<MainViewModel>());
            MainPage = navController.NavigationRoot;
#else            
            await NavController.InitAsync(new MainPage());
            MainPage = NavController.NavigationRoot;
#endif
        }
    }
}
