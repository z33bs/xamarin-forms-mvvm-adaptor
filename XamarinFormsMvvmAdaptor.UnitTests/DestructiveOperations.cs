using System;
using System.Threading.Tasks;
using NUnit.Framework;
using XamarinFormsMvvmAdaptor.UnitTests.ViewModels;
using XamarinFormsMvvmAdaptor.UnitTests.Views;

namespace XamarinFormsMvvmAdaptor.UnitTests
{
    public class DestructiveOperations
    {
        INavController navController;

        [OneTimeSetUp]
        public void SetupOnce()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        [SetUp]
        public async Task Setup()
        {
            navController = new NavController();
            await navController.InitAsync(new TestPage0());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OnAppearing_runs_after_PopAsync(bool isAnimated)
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PopAsync(isAnimated);
            Assume.That(navController.TopViewModel is TestViewModel1);
            var vm = navController.TopViewModel as TestViewModel1;
            Assert.IsTrue(vm.OnAppearingRuns == 2);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OnAppearing_doesnt_runs_after_PopAsync_if_modal(bool isAnimated)
        {
            navController = new NavController();
            await navController.InitAsync(new TestPage1());

            await navController.PushAsync<TestViewModel2>();
            await navController.PushModalAsync<TestViewModel3>();
            await navController.PopAsync(isAnimated);
            var vm = navController.MainStack.GetCurrentViewModel();
            Assume.That(vm is TestViewModel1);            
            Assert.AreEqual(1, (vm as TestViewModel1).OnAppearingRuns);
        }


        [TestCase(true)]
        [TestCase(false)]
        public async Task OnAppearing_runs_after_PopModalAsync(bool isAnimated)
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushModalAsync<TestViewModel2>();
            await navController.PopModalAsync(isAnimated);
            Assume.That(navController.TopViewModel is TestViewModel1);
            var vm = navController.TopViewModel as TestViewModel1;
            Assert.IsTrue(vm.OnAppearingRuns == 2);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OnAppearing_runs_after_PopModalAsync_even_if_modal_stack(bool isAnimated)
        {
            await navController.PushModalAsync<TestViewModel1>();
            await navController.PushModalAsync<TestViewModel2>();
            await navController.PopModalAsync(isAnimated);
            Assume.That(navController.TopViewModel is TestViewModel1);
            var vm = navController.TopViewModel as TestViewModel1;
            Assert.AreEqual(2,vm.OnAppearingRuns);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task PopToRoot_leaves_only_rootPage(bool isAnimated)
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PopToRootAsync(isAnimated);
            Assert.Multiple(() => {
                Assert.AreEqual(navController.MainStack.Count,1);
                Assert.IsInstanceOf<TestPage0>(navController.MainStack[0]);
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task PopToRoot_affects_only_MainStack(bool isAnimated)
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushModalAsync<TestViewModel2>();
            await navController.PopToRootAsync(isAnimated);
            Assert.Multiple(() => {
                Assert.AreEqual(navController.MainStack.Count, 1);
                Assert.AreEqual(navController.ModalStack.Count, 1);
                Assert.IsInstanceOf<TestPage0>(navController.MainStack[0]);
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OnAppearing_runs_after_PopToRootAsync(bool isAnimated)
        {
            navController = new NavController();
            await navController.InitAsync(new TestPage1());

            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();
            await navController.PopToRootAsync(isAnimated);
            Assume.That(navController.RootViewModel is TestViewModel1);
            var vm = navController.RootViewModel as TestViewModel1;
            Assert.IsTrue(vm.OnAppearingRuns == 2);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OnAppearing_doesnt_run_after_PopToRootAsync_if_modal(bool isAnimated)
        {
            navController = new NavController();
            await navController.InitAsync(new TestPage1());

            await navController.PushAsync<TestViewModel2>();
            await navController.PushModalAsync<TestViewModel3>();
            await navController.PopToRootAsync(isAnimated);
            Assume.That(navController.RootViewModel is TestViewModel1);
            var vm = navController.RootViewModel as TestViewModel1;
            Assert.AreEqual(1,vm.OnAppearingRuns);
        }

        [Test]
        public async Task CollapseMainStack_results_in_TopPage_being_RootPage()
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();
            await navController.PushAsync<TestViewModel4>();
            Assume.That(navController.MainStack.Count == 5);

            await navController.CollapseMainStack();

            Assert.Multiple(() => {
                Assert.AreEqual(1, navController.MainStack.Count);
                Assert.IsInstanceOf<TestViewModel4>(navController.TopViewModel);
                Assert.AreEqual(navController.TopPage, navController.RootPage);
            });
        }

        [Test]
        public async Task OnAppearing_runs_after_CollapseMainStack()
        {
            navController = new NavController();
            await navController.InitAsync(new TestPage3());

            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel1>();
            await navController.CollapseMainStack();
            Assume.That(navController.RootViewModel is TestViewModel1);
            var vm = navController.RootViewModel as TestViewModel1;
            Assert.AreEqual(2,vm.OnAppearingRuns);
        }

        [Test]
        public async Task OnAppearing_doesnt_run_after_CollapseMainStack_if_modal()
        {
            navController = new NavController();
            await navController.InitAsync(new TestPage0());

            await navController.PushAsync<TestViewModel1>();
            await navController.PushModalAsync<TestViewModel2>();
            await navController.CollapseMainStack();
            Assume.That(navController.RootViewModel is TestViewModel1);
            var vm = navController.RootViewModel as TestViewModel1;
            Assert.AreEqual(1, vm.OnAppearingRuns);
        }

        [Test]
        public async Task RemovePreviousPageFromMainStack_removes_previous_page()
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();
            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack.GetPreviousPage() is TestPage2);

            navController.RemovePreviousPageFromMainStack();

            Assert.Multiple(() => {
                Assert.AreEqual(3, navController.MainStack.Count);
                Assert.IsInstanceOf<TestPage1>(navController.MainStack.GetPreviousPage());
            });
        }

        [Test]
        public async Task RemovePreviousPageFromMainStack_still_removes_previous_page_of_main_if_modal()
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();
            await navController.PushModalAsync<TestViewModel4>();
            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack.GetPreviousPage() is TestPage2);

            navController.RemovePreviousPageFromMainStack();

            Assert.Multiple(() => {
                Assert.AreEqual(3, navController.MainStack.Count);
                Assert.IsInstanceOf<TestPage1>(navController.MainStack.GetPreviousPage());
            });
        }

        [Test]
        public async Task RemovePreviousPageFromMainStack_still_removes_previous_page_if_its_rootPage()
        {
            await navController.PushAsync<TestViewModel1>();
            Assume.That(navController.MainStack.Count == 2);
            Assume.That(navController.MainStack.GetPreviousPage() is TestPage0);

            navController.RemovePreviousPageFromMainStack();

            Assert.Multiple(() => {
                Assert.AreEqual(1, navController.MainStack.Count);
                Assert.IsInstanceOf<TestPage1>(navController.RootPage);
            });
        }

        [Test]
        public void RemovePreviousPageFromMainStack_does_nothing_if_stackCount1()
        {
            Assume.That(navController.MainStack.Count == 1);

            navController.RemovePreviousPageFromMainStack();

            Assert.Multiple(() => {
                Assert.AreEqual(1, navController.MainStack.Count);
                Assert.IsInstanceOf<TestPage0>(navController.RootPage);
            });
        }

        [Test]
        public async Task RemovePageFor_removes_appropriate_page()
        {
            await navController.PushAsync<TestViewModel1>();
            await navController.PushAsync<TestViewModel2>();
            await navController.PushAsync<TestViewModel3>();
            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack[1] is TestPage1);

            navController.RemovePageFor<TestViewModel1>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, navController.MainStack.Count);
                Assert.IsNotInstanceOf<TestPage1>(navController.MainStack[1]);
            });
        }

        [Test]
        public async Task RemovePageFor_works_with_Di_pages()
        {
            navController = new NavController();
            await navController.DiInitAsync(new DiTestViewModel0());
            await navController.DiPushAsync(new DiTestViewModel1());
            await navController.DiPushAsync(new DiTestViewModel2());
            await navController.DiPushAsync(new DiTestViewModel3());

            Assume.That(navController.MainStack.Count == 4);
            Assume.That(navController.MainStack[1] is DiTestPage1);

            navController.RemovePageFor<DiTestViewModel1>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, navController.MainStack.Count);
                Assert.IsNotInstanceOf<DiTestPage1>(navController.MainStack[1]);
            });
        }

    }
}
