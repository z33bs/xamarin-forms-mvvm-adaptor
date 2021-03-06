﻿using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.Views;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using System.Threading.Tasks;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class Stack2Modal2Pages
    {
        INavController navController;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            navController = new NavController();
            await navController.InitAsync(new TestPage0());
            await navController.PushAsync<TestViewModel1>();
            await navController.PushModalAsync<TestViewModel2>();
            await navController.PushModalAsync<TestViewModel3>();
        }

        [Test]
        public void TopPage_returns_top_page()
        {
            Assert.Multiple(() =>
            {
                Assert.AreSame(navController.ModalStack[1], navController.TopPage);
                Assert.IsInstanceOf<TestPage3>(navController.TopPage);
            });
        }

        [Test]
        public void HiddenPage_returns_hidden_page()
        {
            var stack = navController.MainStack;
            Assert.Multiple(() =>
            {
                Assert.AreSame(navController.ModalStack[0], navController.HiddenPage);
                Assert.IsInstanceOf<TestPage2>(navController.HiddenPage);
            });
        }

        [Test]
        public void RemoveHiddenPageFromStack()
        {
            Assume.That(navController.MainStack.Count == 2);
            Assume.That(navController.ModalStack.Count == 2);
            Assume.That(
                navController.MainStack
                .GetCurrentPage()
                is TestPage1);
            Assume.That(
                navController.MainStack
                .GetPreviousPage()
                is TestPage0);

            navController.RemovePreviousPageFromMainStack();
            Assert.Multiple(() =>
            {
                Assert.IsTrue(navController.ModalStack.Count == 2);
                Assert.IsTrue(navController.MainStack.Count == 1);
                Assert.IsInstanceOf<TestPage1>(
                    navController.MainStack
                    .GetCurrentPage());
                Assert.IsNull(
                    navController.MainStack
                    .GetPreviousPage());
            });
        }

        [Test]
        public void GetCurrentViewModel_returns_correct_vm_from_MainStack()
        {
            Assert.IsInstanceOf<TestViewModel1>(navController.MainStack.GetCurrentViewModel());
        }

        [Test]
        public void GetCurrentViewModel_returns_correct_vm_from_ModalStack()
        {
            Assert.IsInstanceOf<TestViewModel3>(navController.ModalStack.GetCurrentViewModel());
        }

        [Test]
        public void GetPreviousViewModel_returns_correct_vm_from_MainStack()
        {
            Assert.IsInstanceOf<TestViewModel0>(navController.MainStack.GetPreviousViewModel());
        }

        [Test]
        public void GetPreviousViewModel_returns_correct_vm_from_ModalStack()
        {
            Assert.IsInstanceOf<TestViewModel2>(navController.ModalStack.GetPreviousViewModel());
        }

    }
}
