using Klavogonki;
using Klavogonki.Hrustyashki;
using Ninject.Modules;

namespace Examples
{
    internal class NinjectRegistrations : NinjectModule
    {
        public override void Load()
        {
            Bind<ISuccessService>().To<SuccessService>().InSingletonScope();
            Bind<IQuickStatService>().To<QuickStatService>().InTransientScope();
            Bind<IPlayerService>().To<PlayerService>().InTransientScope();
            Bind<ITopService>().To<TopService>().InTransientScope();
            Bind<IModeStatService>().To<ModeStatService>().InTransientScope();
            Bind<IUserInfoStatService>().To<UserInfoStatService>().InTransientScope();
            Bind<IVideoService>().To<VideoService>().InTransientScope();
            Bind<IHrustUpdater>().To<HrustUpdater>().InTransientScope();
            Bind<IFriendsService>().To<FriendsService>().InTransientScope();
            Bind<IForumService>().To<ForumService>().InTransientScope();
            Bind<IKgAutenticationService>().To<KgAuthenticationService>().InSingletonScope();
            Bind<IOpenStatService>().To<OpenStatService>().InTransientScope();
            Bind<IPeriodStatService>().To<PeriodStatService>().InTransientScope();
            Bind<IVocsService>().To<VocsService>().InTransientScope();
            Bind<IUserSummaryService>().To<UserSummaryService>().InTransientScope();
            Bind<IUserInfoService>().To<UserInfoService>().InTransientScope();
            Bind<IHiddenStatService>().To<HiddenStatService>().InTransientScope();
            Bind<IDetailedStatService>().To<DetailedStatService>().InTransientScope();
            Bind<ITeamCandidatService>().To<TeamCandidatService>().InTransientScope();
            Bind<IExperienceService>().To<ExperienceService>().InTransientScope();
        }
    }
}
