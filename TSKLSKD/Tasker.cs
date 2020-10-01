using System;
using System.Linq;

namespace TSKLSKD
{
    class Tasker
    {
        #region Fields
        private System.Collections.Generic.Dictionary<string, string> _configurationValues = new System.Collections.Generic.Dictionary<string, string>
        {
            { "min", "" },
            { "hour", "" },
            { "day", "" },
            { "month", "" },
            { "week", "" },
            { "task", "" }
        };
        private double _elapsed;
        private FileCenter _taskFile;
        private System.Diagnostics.Process _process;
        private System.Collections.Generic.List<DayOfWeek> _weekDays;
        private bool _isWeekedTime = false;
        private System.Timers.Timer _timer;
        private bool _inExecution;
        #endregion

        #region Enums
        /*
         * <summary>
         *  The different time used in my tasker file
         *  converted into milleseconds
         * </summary>
         */
        public enum Milliseconds : long
        {
            minute = 60000,
            hour = 3600000,
            day = 86400000,
            month = 2629746000
        }
        /*
         * <summary>
         *  day of the week used into my file
         * </summary>
         */
        public enum WeekDay
        {
            Sun = 0,
            Mon = 1,
            Tue = 2,
            Wed = 3,
            Thu = 4,
            Fri = 5,
            Sat = 6
        }
        #endregion

        #region Properties
        public string FileToExecute { get { return _taskFile.FileName; } }
        public string DaysOfWeekFormat
        {
            get
            {
                WeekDay[] weekDays = (WeekDay[])Enum.GetValues(typeof(WeekDay));
                System.Collections.Generic.List<WeekDay> listWeekDays = new System.Collections.Generic.List<WeekDay>(weekDays);

                return String.Join("-", listWeekDays);
            }
        }
        public bool Started { get; set; } = false;
        #endregion

        #region Constructors
        public Tasker(string theTask)
        {
            this.TaskController(theTask);
            this.PrepareTask();

            _timer = new System.Timers.Timer(_elapsed);
            _inExecution = false;
        }
        #endregion

        #region Private Methods
        /*
         * <summary>
         *  method use to control the input
         *  prepar the timer and file to execute
         *  <parameters>
         *      <param type="string">the task to do with the type
         *          <format>[min]\t[hour]\t[day]\t[month]\t[week]\t[your task]</format>
         *      </param>
         *  </parameters>
         *  <bug>
         *      It's not active the week control
         *  </bug>
         * </summary>
         */
        private void TaskController(string theTask)
        {
            if (String.IsNullOrEmpty(theTask))
                throw new ArgumentNullException("There aren't a schedulated task. Insert a task with this format: [min]\t[hour]\t[day]\t[month]\t[week]\t[your task]. Remember to use tab and not withe space.");

            try
            {
                string[] tasker = new string[6];

                tasker = theTask.Split('\t');

                _configurationValues["min"] = tasker[0];
                _configurationValues["hour"] = tasker[1];
                _configurationValues["day"] = tasker[2];
                _configurationValues["month"] = tasker[3];
                _configurationValues["week"] = tasker[4];
                _configurationValues["task"] = tasker[5];

                this.SetElapse(_configurationValues["min"], _configurationValues["hour"], _configurationValues["day"], _configurationValues["month"], _configurationValues["week"]);
                _taskFile = new FileCenter(_configurationValues["task"]);
            }
            catch (FormatException ex) { throw new FormatException(ex.Message + " - TaskController"); }
            catch (OverflowException ex) { throw new FormatException(ex.Message + " - TaskController"); }
            catch (Exception ex) { throw new Exception(ex.Message + " - TaskController"); }
        }
        /*
         * <summary>
         *  prepare the elapse field
         *  control the input
         *  <parameters>
         *      <param type="string" name="minute">passed the minute value, could be a number or an *</param>
         *      <param type="string" name="hour">passed the hour value, could be a number or an *</param>
         *      <param type="string" name="day">passed the day, could be a number or an *</param>
         *      <param type="string" name="month">passed the month, could be a number or an *</param>
         *      <param type="string" name="week">passed the week, could be a number, a string (Sun-Mon-Fri-Wed-Thu-Fri-Sat; see the WeekDay enum) or an *
         *          <format>
         *              <example1>Mon-Wed-Fri</example1>
         *              <example2>Mon-3-Sat-Sun</example2>
         *              <example3>Mon-Fri-Fri-Sat</example3>
         *      </param>
         *  </parameters>
         * </summary>
         */
        private void SetElapse(string minute, string hour, string day, string month, string week)
        {
            long minuteL, hourL, dayL, monthL;

            try
            {
                if (week != "*")
                {
                    // prepare the elapsed time for the week
                    // save the week day into an array
                    string[] daysOfWeek = week.Split('-');
                    int number = 0;
                    int countOfDaysSetByUser = daysOfWeek.Length;
                    int countOfDaysEnumMatched = daysOfWeek.Where(d => (int.TryParse(d, out number)) ?                  // is integer???
                                                                        Enum.IsDefined(typeof(WeekDay), number) :       // control the integer format
                                                                        Enum.IsDefined(typeof(WeekDay), d)).Count();    // control the string format

                    if (countOfDaysSetByUser != countOfDaysEnumMatched && countOfDaysSetByUser > 0)
                        throw new ArgumentException("Wrong Format. For the week values you can enter a numeric value from 0 to 6 or in string format (" + DaysOfWeekFormat + "), everything separate by '-' - SetElapse");

                    // prepare the day of the week to do the task
                    System.Collections.Generic.List<int> daysOfWeekNumber = daysOfWeek.Select(d => (int)Enum.Parse(typeof(WeekDay), d, false)).ToList();
                    daysOfWeekNumber = this.CleanDuplicateValues(daysOfWeekNumber);
                    _weekDays = daysOfWeekNumber.Select(d => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), d.ToString())).ToList();

                    // prepare the date
                    this.ChangeElapse(minute, hour, day, month);

                    _isWeekedTime = true;
                }
                else
                {
                    minuteL = (minute == "*") ? 0 : Convert.ToInt32(minute) * (long)Milliseconds.minute;
                    hourL = (hour == "*") ? 0 : Convert.ToInt32(hour) * (long)Milliseconds.hour;
                    dayL = (day == "*") ? 0 : Convert.ToInt32(day) * (long)Milliseconds.day;
                    monthL = (month == "*") ? 0 : Convert.ToInt32(month) * (long)Milliseconds.month;

                    _elapsed = (minuteL == 0 && hourL == 0 && dayL == 0 && monthL == 0) ? 60000 : minuteL + hourL + dayL + monthL;
                }
            }
            catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.Message + " - SetElapse"); }
            catch (ArgumentException ex) { throw new ArgumentException(ex.Message + " - SetElapse"); }
            catch (InvalidOperationException ex) { throw new InvalidOperationException(ex.Message + " - SetElapse"); }
            catch (OverflowException ex) { throw new OverflowException(ex.Message + " - SetElapse"); }
            catch (FormatException ex) { throw new FormatException(ex.Message + " - SetElapse"); }
        }
        /*
         * <summary>
         *  calculate the specific time setted by the user
         *  combined when the week is compiled
         *  <parameters>
         *      <param type="string" name="minute">passed the minute value, could be a number or an *</param>
         *      <param type="string" name="hour">passed the hour value, could be a number or an *</param>
         *      <param type="string" name="day">passed the day, could be a number or an *</param>
         *      <param type="string" name="month">passed the month, could be a number or an *</param>
         *  </parameters>
         *  <upgrade>
         *      change the minute from int to double
         *  </upgrade>
         * </summary>
         */
        private void ChangeElapse(string minute, string hour, string day, string month)
        {
            DateTime now = DateTime.Now;
            int minuteI = (minute == "*") ? 0 : Convert.ToInt32(minute);
            int hourI = (hour == "*") ? now.Hour : Convert.ToInt32(hour);
            int dayI = (day == "*") ? now.Day : Convert.ToInt32(day);
            int monthI = (month == "*") ? now.Month : Convert.ToInt32(month);
            DateTime userTime = new DateTime(now.Year, monthI, dayI, hourI, minuteI, 0);

            if (now > userTime)
                userTime = userTime.AddDays(1);

            _elapsed = (userTime - now).TotalMilliseconds;
        }
        /*
         * <summary>
         *  remove duplicated value inside a list
         *  <parameters>
         *      <param type="System.Collections.Generic.List<int>" name="weekList">a list with the day of the week</param>
         *  </parameters>
         * </summary>
         */
        private System.Collections.Generic.List<int> CleanDuplicateValues(System.Collections.Generic.List<int> weekList)
        {
            for (int i = 0; i < weekList.Count; i++)
            {
                for (int j = weekList.Count - 1; j > i; j--)
                {
                    if (weekList[i] == weekList[j])
                        weekList.RemoveAt(j);
                }
            }

            return weekList;
        }
        /*
         * <summary>
         *  prepare the batch or exe file
         * </summary>
         */
        private void PrepareTask()
        {
            try
            {
                _process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        WorkingDirectory = _taskFile.PathFile,
                        FileName = _taskFile.FileName,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    }
                };
            }
            catch (System.ComponentModel.InvalidEnumArgumentException ex) { throw new System.ComponentModel.InvalidEnumArgumentException(ex.Message + " - Constructor"); }
            catch (Exception ex) { throw new Exception(ex.Message + " - Constructor"); }
        }

        /*
         * <summary>
         *  what to do when the timer elapse
         *  call a file to execute
         * </summary>
         */
        private void OnElapsedTime(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_inExecution || (!System.IO.File.Exists(_taskFile.FullFilename))) return;

            try
            {
                _inExecution = true;

                if (_isWeekedTime)
                {
                    DateTime now = DateTime.Now;
                    _timer.Enabled = false;

                    // execute the task
                    if (_weekDays.Where(x => x == now.DayOfWeek).Count() > 0)
                        _process.Start();

                    // change timer
                    this.ChangeElapse(_configurationValues["min"], _configurationValues["hour"], _configurationValues["day"], _configurationValues["month"]);

                    _timer.Interval = _elapsed;
                    _timer.Enabled = true;
                }
                else
                {
                    _process.Start();
                }
                _inExecution = false;
            }
            catch (ObjectDisposedException ex) { throw new ObjectDisposedException(ex.Message + " - OnElapsedTime"); }
            catch (InvalidOperationException ex) { throw new InvalidOperationException(ex.Message + " - OnElapsedTime"); }
            catch (System.ComponentModel.Win32Exception ex) { throw new System.ComponentModel.Win32Exception(ex.Message + " - OnElapsedTime"); }
            catch (PlatformNotSupportedException ex) { throw new PlatformNotSupportedException(ex.Message + " - OnElapsedTime"); }
        }
        #endregion

        #region Public Methods
        /*
         * <summary>
         *  pass some options at the timer and start it
         * </summary>
         */
        public void StartTask()
        {
            try
            {
                _timer.Elapsed += OnElapsedTime;
                _timer.AutoReset = true;
                _timer.Enabled = true;

                this.Started = true;
            }
            catch (ObjectDisposedException ex) { throw new ObjectDisposedException(ex.Message + " - Start Task"); }
            catch (ArgumentException ex) { throw new ArgumentException(ex.Message + " - Start Task"); }
        }

        /*
         * <summary>
         *  stop the timer and the process and dispose them
         * </summary>
         */
        public void DisposeProcess()
        {
            _timer.Enabled = false;
            _timer.Dispose();

            _process.Close();
            _process.Dispose();

            _inExecution = false;
            this.Started = false;
        }
        #endregion
    }
}
