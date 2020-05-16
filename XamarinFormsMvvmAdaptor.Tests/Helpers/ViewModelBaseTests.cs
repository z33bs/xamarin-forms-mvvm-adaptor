using System.ComponentModel;
using XamarinFormsMvvmAdaptor.Helpers;
using Xunit;

namespace XamarinFormsMvvmAdaptor.Tests.Helpers
{
    public class ViewModelBaseTests
    {
		class PersonViewModel : ViewModelBase { }
		[Fact]
		public void TitleFact()
		{
			PropertyChangedEventArgs updated = null;
			var vm = new PersonViewModel();

			vm.PropertyChanged += (sender, args) =>
			{
				updated = args;
			};

			vm.Title = "Hello";
			Assert.NotNull(updated);//, "Property changed didn't raise");
			Assert.Equal(nameof(vm.Title), updated.PropertyName);//, "Correct Property name didn't get raised");
		}


		[Fact]
		public void Icon()
		{
			PropertyChangedEventArgs updated = null;
			var vm = new PersonViewModel();

			vm.PropertyChanged += (sender, args) =>
			{
				updated = args;
			};

			vm.Icon = "Hello";
			Assert.NotNull(updated);//, "Property changed didn't raise");
			Assert.Equal(nameof(vm.Icon), updated.PropertyName);//, "Correct Property name didn't get raised");
		}

		[Fact]
		public void IsBusy()
		{
			PropertyChangedEventArgs updated = null;
			var vm = new PersonViewModel();

			vm.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "IsBusy")
					updated = args;
			};

			vm.IsBusy = true;
			Assert.NotNull(updated);//, "Property changed didn't raise");
			Assert.Equal(nameof(vm.IsBusy), updated.PropertyName);//, "Correct Property name didn't get raised");

			Assert.False(vm.IsNotBusy, "Is Not Busy didn't change.");
		}

		[Fact]
		public void IsNotBusy()
		{
			PropertyChangedEventArgs updated = null;
			var vm = new PersonViewModel();

			vm.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "IsNotBusy")
					updated = args;
			};

			vm.IsNotBusy = false;
			Assert.NotNull(updated);//, "Property changed didn't raise");
			Assert.Equal(nameof(vm.IsNotBusy), updated.PropertyName);//, "Correct Property name didn't get raised");

			Assert.True(vm.IsBusy, "Is Busy didn't change.");
		}
	}
}
