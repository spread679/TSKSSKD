using System;
using System.ServiceProcess;

namespace TSKSSKD
{
    public partial class TSKSerive : ServiceBase
    {
        #region Fields
        private TSKLSKD.SCenter _sCenter;
        #endregion

        public TSKSerive()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _sCenter = new TSKLSKD.SCenter();

                _sCenter.SetUpTasks();
                _sCenter.WriteLogFile("Set up the services.");

                _sCenter.StartTasks();
                _sCenter.WriteLogFile("Services started.");

                _sCenter.StartTimer();
            }
            catch(Exception ex) { _sCenter.WriteLogFile(ex.Message); }
        }

        protected override void OnStop()
        {
            try
            {
                _sCenter.DisposeTasks();
                _sCenter.WriteLogFile("Services stopped.");
            }
            catch (Exception ex) { _sCenter.WriteLogFile(ex.Message); }
        }
    }
}
