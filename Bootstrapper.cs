using Caliburn.Micro;
using EMT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EMT
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer _container = new SimpleContainer();

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void Configure()
        {
            base.Configure();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();

            _container.Singleton<ShellViewModel>();
            _container.Singleton<StartViewModel>();
            _container.Singleton<MissionViewModel>();
            _container.Singleton<MissionTreeViewModel>();
            _container.Singleton<MissionDetailsViewModel>();
            _container.Singleton<BranchDetailsViewModel>();

            _container.PerRequest<GfxDialogViewModel>();
        }
    }
}
