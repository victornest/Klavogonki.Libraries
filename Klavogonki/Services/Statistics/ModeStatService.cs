using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class ModeStatService : IModeStatService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IQuickStatService quickStatService;
        private readonly IPeriodStatService periodStatService;
        private readonly IDetailedStatService detailedStatService;

        public ModeStatService(IQuickStatService quickStatService, IPeriodStatService periodStatService, IDetailedStatService detailedStatService)
        {
            this.quickStatService = quickStatService;
            this.periodStatService = periodStatService;
            this.detailedStatService = detailedStatService;
        }

        public async Task<ModeStatList> GetStatsByUsersAndModes(ModeStatSettings input)
        {
            ChangeProgress(new Progress(0));

            bool needPeriod = input.NeedPeriodStat && input.DateFrom != null && input.DateTo != null;
            var modeStatList = new ModeStatList(new StatType { HasPeriod = needPeriod });

            int progressCounter = 0;
            int progressTotal = input.ModeIds.Count * input.UserIds.Count;

            foreach (var modeId in input.ModeIds)
            {
                var mode = new Mode(modeId);
                var list = new ModeStat(mode);
                foreach (var userId in input.UserIds)
                {
                    ChangeProgress(new Progress(progressCounter++, progressTotal));

                    var qs = await quickStatService.GetQuickStat(userId, modeId);
                    if (string.IsNullOrEmpty(mode.Name) && qs.Value != null)
                        mode.SetName(qs.Value.Mode.Name);

                    var s = await detailedStatService.GetDayStat(userId, modeId, DateTime.Today);

                    Stat stat = new Stat(qs.Value) { HasPremium = s.Value?.HasPremium };

                    if (needPeriod)
                    {
                        var daysStat = await periodStatService.GetDaysStat(userId, modeId, input.DateFrom.Value, to: input.DateTo.Value);
                        if (daysStat.IsOpen)
                        {
                            var periodStat = periodStatService.GetPeriodStat(daysStat.Value, from: input.DateFrom.Value, to: input.DateTo.Value, maxSpeed: qs.Value.Record);
                            stat.PeriodStat = periodStat;
                        }
                    }                    

                    list.Add(stat);
                }
                modeStatList.Add(list);
            }
            ChangeProgress(new Progress(progressCounter, progressTotal));
            return modeStatList;
        }
    }
}
