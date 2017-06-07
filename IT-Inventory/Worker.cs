using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace IT_Inventory
{
    public class Worker
    {
        private string _log;
        private readonly Timer _30Timer;
        public Worker()
        {
            _30Timer = new Timer
            {
                Interval = 1800000
            };
            _30Timer.Elapsed += On30MinTimer;
        }

        public async void StartJobsAsync()
        {
            _30Timer?.Start();
            _log = "IT Inventory worker started!";
            await _log.WriteToLogAsync();

            //run tasks on startup
            await Task.Run(() => StaticData.RefreshUsers());
            await Task.Run(() => StaticData.RefreshComputers());
            await Task.Run(() => StaticData.SendUrgentItemsMail());
        }

        public void StopJobs()
        {
            _30Timer?.Stop();
        }

        private async void On30MinTimer(object sender, ElapsedEventArgs e)
        {
            _log = "30 minute job started!";
            await _log.WriteToLogAsync();

            await Task.Run(() => StaticData.RefreshUsers());
            await Task.Run(() => StaticData.RefreshComputers());
            await Task.Run(() => StaticData.SendUrgentItemsMail());
        }
    }
}