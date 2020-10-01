using System;
using System.Linq;

namespace TSKLSKD
{
    public class SCenter
    {
        #region Fileds
        private static readonly string _settingsPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _tskPathSettings = System.IO.Path.Combine(_settingsPath, "TSK");
        private static readonly string _logPathSettings = System.IO.Path.Combine(_settingsPath, "LOG");
        private readonly char _commentChar = '#';
        private System.Collections.Generic.List<FileCenter> _allFilesInTskPath;
        private System.Collections.Generic.List<Tasker> _allTheTaskers;
        private System.Collections.Generic.List<string> _filesToExecute;
        private System.Collections.Generic.List<string> _filesName;
        private FileCenter _fileLog;
        private System.Timers.Timer _timer;
        #endregion

        #region Properties
        public string SettingsPath { get { return _settingsPath; } }
        public string TskPath { get { return _tskPathSettings; } }
        public string LogPath { get { return _logPathSettings; } }
        public string ExtensionTsk { get; } = ".tsk.txt";
        public string ExtensioLog { get; } = ".tsk.log";
        #endregion

        #region Constructors
        public SCenter()
        {
            try
            {
                DateTime now = DateTime.Now;
                string fileLog = "sCenter_" + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + this.ExtensioLog;
                string completePathFileLog = System.IO.Path.Combine(_logPathSettings, fileLog);

                // prepare the setting folders
                FileCenter.CheckPath(_settingsPath);
                FileCenter.CheckPath(_tskPathSettings);
                FileCenter.CheckPath(_logPathSettings);

                _allFilesInTskPath = new System.Collections.Generic.List<FileCenter>();
                _allTheTaskers = new System.Collections.Generic.List<Tasker>();
                _filesToExecute = new System.Collections.Generic.List<string>();
                _filesName = new System.Collections.Generic.List<string>();
                _fileLog = new FileCenter(completePathFileLog);
            }
            catch (System.IO.DirectoryNotFoundException ex) { throw new System.IO.IOException(ex.ToString() + " - Constructor SCenter"); }
            catch (System.IO.PathTooLongException ex) { throw new System.IO.PathTooLongException(ex.ToString() + " - Constructor SCenter"); }
            catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.ToString() + " - Constructor SCenter"); }
            catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.ToString() + " - Constructor SCenter"); }
            catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.ToString() + " - Constructor SCenter"); }
            catch (NotSupportedException ex) { throw new NotSupportedException(ex.ToString() + " - Constructor SCenter"); }
        }
        #endregion

        #region Public Methods
        /*
         * <summary>
         *  used to control all the task files
         *  read all their lines
         *  and set up the tasks
         * </summary>
         */
        public void SetUpTasks()
        {
            string[] tskFiles = System.IO.Directory.GetFiles(_tskPathSettings, ("*" + ExtensionTsk));
            System.Collections.Generic.List<string> tasks;

            this.AddNewFiles(tskFiles);

            tasks = this.GetAllTasks();

            this.AddNewTasks(tasks);

            tasks.Clear();
        }
        /*
         * <summary>
         *  start all the services
         * </summary>
         */
        public void StartTasks()
        {
            try
            {
                foreach (var service in _allTheTaskers.Where(x => !x.Started))
                    service.StartTask();
            }
            catch (Exception ex) { throw new Exception(ex.ToString() + " - StartTasks"); }
        }
        /*
         * <summary>
         *  dispose all the process
         * </summary>
         */
        public void DisposeTasks()
        {
            foreach (var service in _allTheTaskers.Where(x => x.Started))
                service.DisposeProcess();
        }
        /*
         * <summary>
         *  prepare log file
         *  <parameters>
         *      <param type="string" name="logLine">the message to write into the log file</param>
         *  </parameters>
         * </summary>
         */
        public void WriteLogFile(string logLine)
        {
            try
            {
                DateTime now = DateTime.Now;

                _fileLog.WriteLine("[" + now.ToString() + "] - " + logLine);
                _fileLog.WriteLine("\n");
            }
            catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.ToString() + " - WriteLogFile"); }
            catch (ArgumentException ex) { throw new ArgumentException(ex.ToString() + " - WriteLogFile"); }
            catch (Exception ex) { throw new Exception(ex.ToString() + " - WriteLogFile"); }
        }
        /*
         * <summary>
         *  set the timer and start it
         *  the default value of the timer is every minute (60000)
         * </summary>
         */
        public void StartTimer()
        {
            _timer = new System.Timers.Timer(60000);
            _timer.Elapsed += this.ControlTasks;
            _timer.Enabled = true;
        }
        #endregion

        #region Private Method
        /*
         * <summary>
         *  read inside the task files and get the time and the file to execute
         *  <parameters>
         *      <param type="FileCenter" name="fileToRead">the file to read</param>
         *  </parameters>
         * </summary>
         */
        private System.Collections.Generic.List<string> GetTaskFromAFile(FileCenter fileToRead)
        {
            System.Text.StringBuilder text = fileToRead.ReadFile();
            System.Collections.Generic.List<string> tasks = new System.Collections.Generic.List<string>();

            // read all the line and ignore the comment (#)
            foreach (var line in text.ToString().Split('\n'))
            {
                if (line.IndexOf(_commentChar) >= 0 || String.IsNullOrWhiteSpace(line))
                    continue;
                tasks.Add(line);
            }

            text.Clear();

            return tasks;
        }
        /*
         * <summary>
         *  prepare a list with all the tasks to execute
         * </summary>
         */
        private System.Collections.Generic.List<string> GetAllTasks()
        {
            System.Collections.Generic.List<string> tasks = new System.Collections.Generic.List<string>();

            foreach (var file in _allFilesInTskPath)
            {
                foreach (var line in this.GetTaskFromAFile(file))
                    tasks.Add(line);
            }

            return tasks;
        }
        /*
         * <summary>
         *  return all the new files not already readed
         *  <parameters>
         *      <param type="string[]" name="files">all the new files readed in TSK folder</param>
         *  </parameters>
         * </summary>
         */
        private void AddNewFiles(string[] files)
        {
            foreach (var file in files)
            {
                if (_filesName.Where(x => x == file).Count() == 0)
                {
                    _allFilesInTskPath.Add(new FileCenter(file));

                    _filesName.Add(file);
                }
            }
        }
        /*
         * <summary>
         *  get the newest tasks to execute
         *  <parameters>
         *      <param type="System.Collections.Generic.List<string>" name="tasks">all the tasks to add</param>
         *  </parameters>
         * </summary>
         */
        private void AddNewTasks(System.Collections.Generic.List<string> tasks)
        {
            foreach (var task in tasks)
            {
                if (_filesToExecute.Where(x => x == task).Count() == 0)
                {
                    _allTheTaskers.Add(new Tasker(task));

                    _filesToExecute.Add(task);
                    this.WriteLogFile("New task: " + task);
                }
            }
        }
        /*
         * <summary>
         *  return all the files to remove
         *  <parameters>
         *      <param type="string[]" name="files">all the new files readed in TSK folder</param>
         *  </parameters>
         * </summary>
         */
        private void RemoveFiles(string[] files)
        {
            for (int i = _filesName.Count - 1; i >= 0; --i)
            {
                string file = _filesName[i];

                if (files.Where(x => x == file).Count() == 0)
                {
                    _allFilesInTskPath.RemoveAt(i);
                    _filesName.RemoveAt(i);
                    this.WriteLogFile("Removed file: " + file);
                }
            }
        }
        /*
         * <summary>
         *  remove the tasks removed from the files
         *  <parameters>
         *      <param tyep="System.Collections.Generic.List<string>" name="tasks">all the tasks read into the files</param>
         *  </parameters>
         * </summary>
         */
        private void RemoveTask(System.Collections.Generic.List<string> tasks)
        {
            for (int i = _filesToExecute.Count - 1; i >= 0; --i)
            {
                string task = _filesToExecute[i];

                if (tasks.Where(x => x == task).Count() == 0)
                {
                    _allTheTaskers[i].DisposeProcess();

                    _filesToExecute.RemoveAt(i);
                    _allTheTaskers.RemoveAt(i);
                    this.WriteLogFile("Removed " + task);
                }
            }
        }
        /*
         * <summary>
         *  control if the user sets new files or new tasks
         * </summary>
         */
        private void ControlTasks(object sender, System.Timers.ElapsedEventArgs e)
        {
            string[] tskFiles = System.IO.Directory.GetFiles(_tskPathSettings, ("*" + this.ExtensionTsk));
            System.Collections.Generic.List<string> tasks;

            this.RemoveFiles(tskFiles);
            this.AddNewFiles(tskFiles);

            tasks = this.GetAllTasks();

            this.RemoveTask(tasks);
            this.AddNewTasks(tasks);
            this.StartTasks();

            tasks.Clear();
        }
        #endregion
    }
}