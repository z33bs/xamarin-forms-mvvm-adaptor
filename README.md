# ![Logo](https://raw.githubusercontent.com/z33bs/xamarin-forms-mvvm-adaptor/master/XamarinFormsMvvmAdaptor/Art/icon.png) Xamarin Forms Mvvm Adaptor
**ViewModel-First** Mvvm framework for Xamarin.Forms. Lightweight, it adapts Xamarin's existing Mvvm engine.

**Build Status:** [![Build Status](https://dev.azure.com/guy-antoine/xamarin-forms-mvvm-adaptor/_apis/build/status/z33bs.xamarin-forms-mvvm-adaptor%20(1)?branchName=master)](https://dev.azure.com/guy-antoine/xamarin-forms-mvvm-adaptor/_build/latest?definitionId=2&branchName=master)

**NuGets:**

| Name                    |                             Info                             |
| :---------------------- | :----------------------------------------------------------: |
| XamarinFormsMvvmAdaptor | [![NuGet](https://buildstats.info/nuget/XamarinFormsMvvmAdaptor?includePreReleases=true)](https://www.nuget.org/packages/XamarinFormsMvvmAdaptor/) |
| Development Feed        | [![MyGet Badge](https://buildstats.info/myget/zeebz-open-source/XamarinFormsMvvmAdaptor)](https://www.myget.org/feed/zeebz-open-source/package/nuget/XamarinFormsMvvmAdaptor) |

## Why?

This library was inspired by the [Enterprise Application Patterns Using Xamarin.Forms](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/) eBook.

Xamarin has fantastic Mvvm functionality, however the pattern is geared towards View-First navigation. I prefer to keep the View "dumb", and put all my logic in the ViewModel. This library allows you to use the Xamarin engine with a ViewModel-First pattern, keeping a strong separation of concerns and more readable code. Using this library, you will find that its rare that you need to put anything in your `.xaml.cs` code-behind files.

**Features:**

* Familiar syntax, e.g. `PushAsync<TViewModel>()` is similar to Xamarin's `PushAsync(Page page)`
* Easily pass data to the appearing view-model with `PushAsync<TViewModel>(object navigationData)`
  * `InitializeAsync(object navigationData)` is triggered automatically when the ViewModel is pushed. This method is also handy if you want to run Async code directly after construction.
* `OnAppearingAsync()` method within the view-model that is triggered automatically when its associated page appears at the top of the stack.
* Supports full dependency injection with the DI engine of your choice.
* Packaged with an optional `AdaptorViewModel` that uses James Montemagno's [MvvmHelpers](https://github.com/jamesmontemagno/mvvm-helpers) (my prefered implementation). Otherwise, just implement the `IAdaptorViewModel` interface.
* Supports multiple navigation-stacks. Useful if you want each tab of a TabbedPage to have its own separate navigation tree.
* Lightweight because its adapts Xamarin's existing engine.

## Getting Started

MvvmAdaptor can be consumed in **two flavours**:

* [Vanilla](#Vanilla implementation) implementation
* With [Dependency Injection (DI)](#DI implementation)

Skip to the flavour that suits you.

### Vanilla implementation

**Initialize the controller** in a way that can easily be accessed throughout your project as a single instance. One way to do this is with a static property in your `app.xaml.cs` file:

```c#
public partial class App : Application
{
  //Initialize the controller with its RootPage
  public static NavController NavController { get; } = new NavController(
    new MainPage());

```

You would access the NavController by calling `App.NavController` anywhwere in your application.

Another way would be to use a dependency service. If you prefer this approach, you will probably be consuming the [Dependency Injection (DI)](#DI implementation) flavour.

**Keep to the following naming conventions**. The MvvmAdaptor works by assuming that you name your View and ViewModel classes consistently. The default expectation is that:

* Views end with the <u>suffix 'Page'</u>, and view-models with the <u>suffix 'ViewModel'</u>. For example `MainPage` and `MainViewModel`.
* All views are in the <u>`.Views` sub-namespace</u>, and view-models in the <u>`.ViewModels` sub-namespace</u>. For example: MainPage will be in the `MyApp.Views` namespace, and MainViewModel will be in the `MyApp.ViewModels` namespace.

You can change the expected naming convention to your personal style with the `SetNamingConventions()` method.

**ViewModels must implement the `IAdaptorViewModel` interface**. The easiest way to achieve this is to extend the `AdaptorViewModel`. This has the added benefit of including all the mvvm boilerplate code and more because it uses James Montemagno's [MvvmHelpers](https://github.com/jamesmontemagno/mvvm-helpers).

```c#
public class MainViewModel : XamarinFormsMvvmAdaptor.AdaptorViewModel
	{
```

Alternatively, you can roll your own BaseViewModel. Note that the two interface methods:

* Are marked `virtual` so that you can `override` them in your derived view-model classes.
* Return `Task.FromResult(false);` as the default implementation.

```c#
public abstract class BaseViewModel : IAdaptorViewModel
{
    public virtual Task InitializeAsync(object navigationData)
    {
        return Task.FromResult(false);
    }

    public virtual Task OnAppearingAsync()
    {
        return Task.FromResult(false);
    }
  
  // Add your own implementations of INotifyPropertyChanged, and custom code
  // ...
}
```

In your view-model instance, if your override implementation of `InitializeAsync()` or `OnAppearingAsync()` is synchronous, you can just return the task `base.InitializeAsync(null)`:

```c#
public override Task InitializeAsync(object navigationData)
{
  DoSomeSynchronousWork(navigationData as MyDocument);
    
  return base.InitializeAsync(null);
}
```

**Bind to your view-model as you normally would** with Xamarin. I prefer to link in the xaml file so that intellisense picks up your bindings.

```xaml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             ... 
             xmlns:vm="clr-namespace:MyApp.ViewModels"/>
  <ContentPage.BindingContext>
      <vm:MainViewModel/>
  </ContentPage.BindingContext>
	
	<ContentPage.Content>
    ...
```

**Set you app's MainPage** in to your NavController's RootPage. 

```c#
public partial class App : Application
{
  //Initialize the controller with its RootPage
  public static NavController NavController { get; } = new NavController(
    new MainPage());

  public App()
  {
    InitializeComponent();
    //=> Set the app's MainPage
    MainPage = NavController.RootPage;
  }
```
**Initialize the RootViewModel** by running `InitAsync()` in the `OnStart()` override.

```c#
public partial class App : Application
{
	...	
  protected override async void OnStart()
  {
    //InitializeAsync and OnAppearing won't run on the RootViewModel
	  //unless you do this. For consistency make it a habit to run Init()
    //even if the above methods are empty.
    await NavController.InitAsync();
  }

```
**Start Navigating!** 

Navigate forwards from your view-models as follows:

```c#
//To Push
await App.NavController.PushAsync<DetailViewModel>();
//or, if you want to pass data
await App.NavController.PushAsync<DetailViewModel>(listItem);

//MODAL NAVIGATION
await App.NavController.PushModalAsync<DetailViewModel>();
```

Navigate backwards with pop:

```c#
//To Push
await App.NavController.PopAsync();

//MODAL NAVIGATION
await App.NavController.PopModalAsync();
```

Checkout the [Navigation](#Navigation) section for a menu of the additional navigation properties and methods.

Checkout the [WordJumble Sample App](#WordJumble) for a simple example using this framework.

***

### DI implementation

*Comming soon...*



***

### Navigation

The Navigation framework simply follows the Xamarin.Forms approach. If you are unfamiliar or rusty, please refer to Microsoft's docs on [Performing Navigation](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/navigation/hierarchical#performing-navigation) (10min read). In addition to the familar methods, I have added some [additional stack manipulation helpers](#Additional helpers).

#### Available in Xamarin.Forms:

* Properties
  * NavigationStack
  * ModalStack
  * RootPage and TopPage (called CurrentPage in Xamarin)
* Methods:
  * PushAsync & PushModalAsync
  * PopAsync & PopModalAsync
  * InsertPageBefore
  * RemovePage

#### Additional helpers:

* Properties:
  * HiddenPage (see [detail](#Shortcuts for accessing Pages and ViewModels in the stack) below for explaination) in addition to RootPage and TopPage
  * RootViewModel, TopViewModel, and HiddenViewModel corresponding to the pages above
* Methods:
  * CollapseStack()
  * RemoveHiddenPageFromStack()
* Static methods that don't need a NavController instance:
  * CreatePageForAsync(IAdaptorViewModel viewmodel)


#### Shortcuts for accessing Pages and ViewModels in the stack

The image below represents a stack of four pages, with labels corresponding
to properties that allow you to access these pages or their corresponding
view-models. The Top, and Root pages are self-explanatory. The Hidden page is
always beneath the Top page. It is 'hidden' by the top page, and will always
be the <u>first to appear when the Top page is popped off the stack</u>. 
![Stack](XamarinFormsMvvmAdaptor\Art\stack.png)
This last point is important to consider if you have pages in a modal stack. Remember that the modal-stack always hides the navigation-stack. If there is one page in the modal-stack, the hidden page is at the top of the navigation-stack (see below), as it will appear next when the modal stack is popped.
![Stack](XamarinFormsMvvmAdaptor\Art\stack_with_1modal.png)
If the modal stack has two pages, the 'hidden' page is beneath the top page of the modal-stack (figure below) because it will appear next when the top modal page is popped.
![Stack](XamarinFormsMvvmAdaptor\Art\stack_with_2modals.png)

For any other pages you can always specify the index of the stack:

```c#
// To access a page
Page page = NavController.NavigationStack[3];

// To access it's corresponding viewmodel
var viewModel = page.BindingContext as IAdaptorViewModel;
```



## Sample Apps

### WordJumble

On the MainPage the user types a four letter word into an entry dialogue. A new page appears with the word's letters jumbled randomly on the page. The user can tap a letter to open a dialogue which allows her to rotate the letter.



WordJumble demonstrates the [Vanilla implementation](#Vanilla implementation) of the XamarinFormsMvvmAdaptor. Specifically:

* Initialising the Navigation Controller
* Push a page onto the stack from the view-model
  * Passing data to that page (the user-entered word),
  * Triggering the WordJumbleViewModel's `InitializeAsync()` method
* Push a modal page onto the modal stack
  * Again passing data to the modal page
* `OnAppearing()` triggered in the WordJumbleViewModel when the user has finished rotating the letter in the modal dialogue
* Popping of pages

In addition to XamarinFormsMvvmAdaptor, the sample uses the following features which you may or may-not be familiar with:

* Xamarin.Forms Material Visual: [docs here](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/visual/material-visual) , and [blog here](https://devblogs.microsoft.com/xamarin/beautiful-material-design-android-ios/).
* Designtime Data (great with Xaml Hot Reload): [docs here](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/xaml/xaml-previewer/design-time-data) , and [blog here](https://montemagno.com/xamarin-forms-design-time-data-tips-best-practices/).

### WordJumbleDi

Identical to [WordJumble](#WordJumble), but implemented with the [DI flavour](#DI implementation) of XamarinFormsMvvmAdaptor. For this example, I have used [AutoFac](https://autofac.org) for the dependecy injection.

