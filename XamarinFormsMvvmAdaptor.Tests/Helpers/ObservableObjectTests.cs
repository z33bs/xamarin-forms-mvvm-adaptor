using System;
using System.ComponentModel;
using Xunit;
namespace XamarinFormsMvvmAdaptor.Tests.Helpers
{
    public class ObservableObjectTests
	{
		Person person;

		private void Setup()
		{
			person = new Person();
			person.FirstName = "James";
			person.LastName = "Montemagno";
		}

		[Fact]
		public void OnPropertyChanged()
		{
			Setup();

			PropertyChangedEventArgs updated = null;
			person.PropertyChanged += (sender, args) =>
			{
				updated = args;
			};

			person.FirstName = "Motz";


			Assert.NotNull(updated);//, "Property changed didn't raise");
			Assert.Equal(nameof(person.FirstName), updated.PropertyName);//, "Correct Property name didn't get raised");
		}

		[Fact]
		public void OnDidntChange()
		{
			Setup();

			PropertyChangedEventArgs updated = null;
			person.PropertyChanged += (sender, args) =>
			{
				updated = args;
			};

			person.FirstName = "James";


			Assert.Null(updated);//, "Property changed was raised, but shouldn't have been");
		}

		[Fact]
		public void OnChangedEvent()
		{
			Setup();


			var triggered = false;
			person.Changed = () =>
			{
				triggered = true;
			};

			person.FirstName = "Motz";

			Assert.True(triggered, "OnChanged didn't raise");
		}

		[Fact]
		public void ValidateEvent()
		{
			Setup();

			var contol = "Motz";
			var triggered = false;
			person.Validate = (oldValue, newValue) =>
			{
				triggered = true;
				return oldValue != newValue;
			};

			person.FirstName = contol;

			Assert.True(triggered, "ValidateValue didn't raise");
			Assert.Equal(person.FirstName, contol);//, "Value was not set correctly.");

		}

		[Fact]
		public void NotValidateEvent()
		{
			Setup();

			var contol = person.FirstName;
			var triggered = false;
			person.Validate = (oldValue, newValue) =>
			{
				triggered = true;
				return false;
			};

			person.FirstName = "Motz";

			Assert.True(triggered, "ValidateValue didn't raise");
			Assert.Equal(person.FirstName, contol);//, "Value should not have been set.");

		}

		[Fact]
		public void ValidateEventException()
		{
			Setup();

			person.Validate = (oldValue, newValue) =>
			{
				throw new ArgumentOutOfRangeException();
			};

			Assert.Throws<ArgumentOutOfRangeException>(() => person.FirstName = "Motz");//, "Should throw ArgumentOutOfRangeException");

		}
	}
}
