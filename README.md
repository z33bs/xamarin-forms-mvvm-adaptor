# ![Logo](https://raw.githubusercontent.com/z33bs/xamarin-forms-mvvm-adaptor/master/XamarinFormsMvvmAdaptor/Art/icon.png) Xamarin Forms Mvvm Adaptor
A **ViewModel-First** Mvvm framework for Xamarin.Forms. Lightweight, it adapts Xamarin's existing Mvvm engine.

**Build Status:** [![Build Status](https://dev.azure.com/guy-antoine/xamarin-forms-mvvm-adaptor/_apis/build/status/z33bs.xamarin-forms-mvvm-adaptor%20(1)?branchName=master)](https://dev.azure.com/guy-antoine/xamarin-forms-mvvm-adaptor/_build/latest?definitionId=2&branchName=master)

**NuGets:**

| Name                    |                             Info                             |
| :---------------------- | :----------------------------------------------------------: |
| XamarinFormsMvvmAdaptor | [![NuGet](https://buildstats.info/nuget/XamarinFormsMvvmAdaptor?includePreReleases=true)](https://www.nuget.org/packages/XamarinFormsMvvmAdaptor/) |
| Development Feed        | [![MyGet Badge](https://buildstats.info/myget/zeebz-open-source/XamarinFormsMvvmAdaptor)](https://www.myget.org/feed/zeebz-open-source/package/nuget/XamarinFormsMvvmAdaptor) |

## Getting Started

Comming soon.... give me a couple days



## Sample Apps

### WordJumble

Demonstrates:

* Instantiate the Navigation Controller
* Push a page onto the stack, while passing data to that page
* Pop a page off the stack
* Push a modal page onto the modal stack
* Pop a modal page

Uses the following features:

* Material Design for Visuals
  * link
* Designtime Data
  * link

### WordJumbleDi

Identical to WordJumble, but XFMvvmA is consumed with Dependency injection

Additional features:

* Uses AutoFac for dependency injection

### PageStackManipulator

In addition to features covered in WordJumble

* InsertPageBefore
* RemoveBackStack
* RemoveLastFromBackStack
* RemovePageFor

### PageStackManipulatorDi

Identical to PageStackManipulator, but XFMvvmA is consumed with Dependency injection