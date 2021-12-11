using Klavogonki;
using MigomanUpdater.Services;
using Ninject.Modules;

namespace MigomanUpdater
{
    internal class NinjectRegistrations : NinjectModule
    {
        public override void Load()
        {
            Bind<ITopService>().To<TopService>().InTransientScope();
            Bind<IModeStatService>().To<ModeStatService>().InTransientScope();
            Bind<IQuickStatService>().To<QuickStatService>().InTransientScope();
            Bind<IPeriodStatService>().To<PeriodStatService>().InTransientScope();
            Bind<IDetailedStatService>().To<DetailedStatService>().InTransientScope();
            Bind<IGoogleSheetService>().To<GoogleSheetService>().InTransientScope();
            Bind<IMigomanResultService>().To<MigomanResultService>().InTransientScope();
        }
    }
}
