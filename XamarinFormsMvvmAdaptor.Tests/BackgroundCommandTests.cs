﻿using System;
using Xunit;
using XamarinFormsMvvmAdaptor;

namespace XamarinFormsMvvmAdaptor.Tests
{
	public class BackgroundCommandTests
	{
		[Fact]
		public void Constructor()
		{
			var cmd = new BackgroundCommand(() => { });
			Assert.True(cmd.CanExecute(null));
		}

		[Fact]
		public void ThrowsWithNullConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand((Action)null));
		}

		[Fact]
		public void ThrowsWithNullParameterizedConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand((Action<object>)null));
		}

		[Fact]
		public void ThrowsWithNullCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand(() => { }, null));
		}

		[Fact]
		public void ThrowsWithNullParameterizedCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand(o => { }, null));
		}

		[Fact]
		public void ThrowsWithNullExecuteValidCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand(null, () => true));
		}

		[Fact]
		public void Execute()
		{
			bool executed = false;
			var cmd = new BackgroundCommand(() => executed = true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Fact]
		public void ExecuteParameterized()
		{
			object executed = null;
			var cmd = new BackgroundCommand(o => executed = o);

			var expected = new object();
			cmd.Execute(expected);

			Assert.Equal(expected, executed);
		}

		[Fact]
		public void ExecuteWithCanExecute()
		{
			bool executed = false;
			var cmd = new BackgroundCommand(() => executed = true, () => true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void CanExecute(bool expected)
		{
			bool canExecuteRan = false;
			var cmd = new BackgroundCommand(() => { }, () => {
				canExecuteRan = true;
				return expected;
			});

			Assert.Equal(expected, cmd.CanExecute(null));
			Assert.True(canExecuteRan);
		}

		[Fact]
		public void ChangeCanExecute()
		{
			bool signaled = false;
			var cmd = new BackgroundCommand(() => { });

			cmd.CanExecuteChanged += (sender, args) => signaled = true;

			cmd.ChangeCanExecute();
			Assert.True(signaled);
		}

		[Fact]
		public void GenericThrowsWithNullExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand<string>(null));
		}

		[Fact]
		public void GenericThrowsWithNullExecuteAndCanExecuteValid()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand<string>(null, s => true));
		}

		[Fact]
		public void GenericThrowsWithValidExecuteAndCanExecuteNull()
		{
			Assert.Throws<ArgumentNullException>(() => new BackgroundCommand<string>(s => { }, null));
		}

		[Fact]
		public void GenericExecute()
		{
			string result = null;
			var cmd = new BackgroundCommand<string>(s => result = s);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Fact]
		public void GenericExecuteWithCanExecute()
		{
			string result = null;
			var cmd = new BackgroundCommand<string>(s => result = s, s => true);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void GenericCanExecute(bool expected)
		{
			string result = null;
			var cmd = new BackgroundCommand<string>(s => { }, s => {
				result = s;
				return expected;
			});

			Assert.Equal(expected, cmd.CanExecute("Foo"));
			Assert.Equal("Foo", result);
		}

		class FakeParentContext
		{
		}

		// ReSharper disable once ClassNeverInstantiated.Local
		class FakeChildContext
		{
		}

		[Fact]
		public void CanExecuteReturnsFalseIfParameterIsWrongReferenceType()
		{
			var command = new BackgroundCommand<FakeChildContext>(context => { }, context => true);

			Assert.False(command.CanExecute(new FakeParentContext()), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteReturnsFalseIfParameterIsWrongValueType()
		{
			var command = new BackgroundCommand<int>(context => { }, context => true);

			Assert.False(command.CanExecute(10.5), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfReferenceTypeAndSetToNull()
		{
			var command = new BackgroundCommand<FakeChildContext>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a reference type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfNullableAndSetToNull()
		{
			var command = new BackgroundCommand<int?>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a Nullable<int> type");
		}

		[Fact]
		public void CanExecuteIgnoresParameterIfValueTypeAndSetToNull()
		{
			var command = new BackgroundCommand<int>(context => { }, context => true);

			Assert.False(command.CanExecute(null), "null is not a valid valid for int");
		}

		[Fact]
		public void ExecuteDoesNotRunIfParameterIsWrongReferenceType()
		{
			int executions = 0;
			var BackgroundCommand = new BackgroundCommand<FakeChildContext>(context => executions += 1);

//			Assert.DoesNotThrow(() => BackgroundCommand.Execute(new FakeParentContext()), "the BackgroundCommand should not execute, so no exception should be thrown");
			var exception = Record.Exception(() => BackgroundCommand.Execute(new FakeParentContext()));
			Assert.Null(exception);

			Assert.True(executions == 0, "the BackgroundCommand should not have executed");
		}

		[Fact]
		public void ExecuteDoesNotRunIfParameterIsWrongValueType()
		{
			int executions = 0;
			var BackgroundCommand = new BackgroundCommand<int>(context => executions += 1);

//			Assert.DoesNotThrow(() => BackgroundCommand.Execute(10.5), "the BackgroundCommand should not execute, so no exception should be thrown");
			var exception = Record.Exception(() => BackgroundCommand.Execute(10.5));
			Assert.Null(exception);

			Assert.True(executions == 0, "the BackgroundCommand should not have executed");
		}

		[Fact]
		public void ExecuteRunsIfReferenceTypeAndSetToNull()
		{
			int executions = 0;
			var BackgroundCommand = new BackgroundCommand<FakeChildContext>(context => executions += 1);

			var exception = Record.Exception(() => BackgroundCommand.Execute(null));
			Assert.Null(exception);
			//"null is a valid value for a reference type");
			Assert.True(executions == 1, "the BackgroundCommand should have executed");
		}

		[Fact]
		public void ExecuteRunsIfNullableAndSetToNull()
		{
			int executions = 0;
			var BackgroundCommand = new BackgroundCommand<int?>(context => executions += 1);

			var exception = Record.Exception(() => BackgroundCommand.Execute(null));
			//"null is a valid value for a Nullable<int> type");
			Assert.Null(exception);
			Assert.True(executions == 1, "the BackgroundCommand should have executed");
		}

		[Fact]
		public void ExecuteDoesNotRunIfValueTypeAndSetToNull()
		{
			int executions = 0;
			var BackgroundCommand = new BackgroundCommand<int>(context => executions += 1);

			var exception = Record.Exception(() => BackgroundCommand.Execute(null));
			Assert.Null(exception);
			Assert.True(executions == 0, "the BackgroundCommand should not have executed");
		}
	}
}