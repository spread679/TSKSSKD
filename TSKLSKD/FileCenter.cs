using System;
using System.Linq;

namespace TSKLSKD
{
    class FileCenter
    {
        #region Fields
        private System.IO.FileInfo _fileName;
        private System.IO.DirectoryInfo _pathFile;
        private string _extension;
        private System.Collections.Generic.List<string> _extensionUsedToWrite;
        #endregion

        #region Constructors
        public FileCenter(string filename)
        {
            string path = System.IO.Path.GetDirectoryName(filename);

            try
            {
                FileCenter.CheckPath(path);
                FileCenter.CheckFile(filename);

                _pathFile = new System.IO.DirectoryInfo(path);
                _fileName = new System.IO.FileInfo(filename);

                _extension = _fileName.Extension;
                _extensionUsedToWrite = new System.Collections.Generic.List<string> { ".txt", ".log" };
            }
            catch (System.IO.DirectoryNotFoundException ex) { throw new System.IO.IOException(ex.Message + " - FileCenter"); }
            catch (System.IO.PathTooLongException ex) { throw new System.IO.PathTooLongException(ex.Message + " - FileCenter"); }
            catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.Message + " - FileCenter"); }
            catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.Message + " - FileCenter"); }
            catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.Message + " - FileCenter"); }
            catch (ArgumentException ex) { throw new ArgumentException(ex.Message + " - FileCenter"); }
            catch (NotSupportedException ex) { throw new NotSupportedException(ex.Message + " - FileCenter"); }
            catch (System.Security.SecurityException ex) { throw new System.Security.SecurityException(ex.Message + " - FileCenter"); }
        }
        #endregion

        #region Properties
        public string FileName { get { return _fileName.Name; } }
        public string PathFile { get { return _pathFile.FullName; } }
        public string FullFilename { get { return _fileName.FullName; } }
        #endregion

        #region Public Methods
        /*
         * <summary>
         *  write into a file
         *  <param type="string">the text to insert or append</param>
         *  <param type="append">true if you need to append otherwise false</param>
         * </summary>
         */
        public void WriteLine(string text, bool append = true)
        {
            if (_extensionUsedToWrite.Where(x => x == _extension).Count() > 0)
            {
                try
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(_fileName.FullName, append))
                        writer.WriteLine(text);
                }
                catch (System.IO.DirectoryNotFoundException ex) { throw new System.IO.IOException(ex.Message + " - WriteLine"); }
                catch (System.IO.PathTooLongException ex) { throw new System.IO.PathTooLongException(ex.Message + " - WriteLine"); }
                catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.Message + " - WriteLine"); }
                catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.Message + " - WriteLine"); }
                catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.Message + " - WriteLine"); }
                catch (ArgumentException ex) { throw new ArgumentException(ex.Message + " - WriteLine"); }
                catch (ObjectDisposedException ex) { throw new ObjectDisposedException(ex.Message + " - WriteLine"); }
            }
            else
            {
                throw new Exception("Extension not allowed.");
            }
        }
        /*
         * <summary>
         *  write into a file
         *  <parameters>
         *      <param type="string" name="text">the text to insert or append</param>
         *      <param type="bool" name="append">true if you need to append otherwise false</param>
         *  </parameters>
         * </summary>
         */
        public System.Text.StringBuilder ReadFile()
        {
            if (_extensionUsedToWrite.Where(x => x == _extension).Count() > 0)
            {
                System.Text.StringBuilder lines = new System.Text.StringBuilder();
                string line = "";

                try
                {
                    using (System.IO.FileStream fs = _fileName.OpenRead())
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(fs))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            lines.Append(line);
                            lines.Append("\n");
                        }
                    }

                    return lines;
                }
                catch (System.IO.DirectoryNotFoundException ex) { throw new System.IO.IOException(ex.Message + " - ReadFile"); }
                catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.Message + " - ReadFile"); }
                catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.Message + " - ReadFile"); }
                catch (ArgumentOutOfRangeException ex) { throw new ArgumentOutOfRangeException(ex.Message + " - ReadFile"); }
                catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.Message + " - ReadFile"); }
                catch (ArgumentException ex) { throw new ArgumentException(ex.Message + " - ReadFile"); }
                catch (OutOfMemoryException ex) { throw new OutOfMemoryException(ex.Message + " - ReadFile"); }
            }
            else
            {
                throw new Exception("Extension not allowed.");
            }
        }
        /*
         * <summary>
         *  remove the file
         * </summary>
         */
        public void DeleteFile()
        {
            try
            {
                _fileName.Delete();
            }
            catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.Message + " - DeleteFile"); }
            catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.Message + " - DeleteFile"); }
            catch (System.Security.SecurityException ex) { throw new System.Security.SecurityException(ex.Message + " - DeleteFile"); }
        }
        /*
         * <summary>
         *  remove the file
         * </summary>
         */
        public void DeletePath()
        {
            try
            {
                _pathFile.Delete();
            }
            catch (System.IO.DirectoryNotFoundException ex) { throw new System.IO.DirectoryNotFoundException(ex.Message + " - DeletePath"); }
            catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.Message + " - DeletePath"); }
            catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.Message + " - DeletePath"); }
            catch (System.Security.SecurityException ex) { throw new System.Security.SecurityException(ex.Message + " - DeletePath"); }
        }
        #endregion

        #region Public Static Methods
        /*
         * <summary>
         *  control if the folders exist otherwise it will generate them.
         *  <parameters>
         *      <param type="string" name="path">the path</param>
         *  </parameters>
         * </summary>
         */
        public static void CheckPath(string path)
        {
            try
            {
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
            }
            catch (System.IO.DirectoryNotFoundException ex) { throw new System.IO.IOException(ex.Message + " - CheckPath"); }
            catch (System.IO.PathTooLongException ex) { throw new System.IO.PathTooLongException(ex.Message + " - CheckPath"); }
            catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.Message + " - CheckPath"); }
            catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.Message + " - CheckPath"); }
            catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.Message + " - CheckPath"); }
            catch (NotSupportedException ex) { throw new NotSupportedException(ex.Message + " - CheckPath"); }
        }
        /*
         * <summary>
         *  control if the file exists otherwise it will generate it.
         *  <parameters>
         *      <param type="string" name="filename">the filename</param>
         *  </parameters>
         * </summary>
         */
        public static void CheckFile(string filename)
        {
            try
            {
                if (!System.IO.File.Exists(filename))
                {
                    var tmpFile = System.IO.File.Create(filename);
                    tmpFile.Close();
                }
            }
            catch (System.IO.DirectoryNotFoundException ex) { throw new System.IO.IOException(ex.Message + " - CheckFile"); }
            catch (System.IO.PathTooLongException ex) { throw new System.IO.PathTooLongException(ex.Message + " - CheckFile"); }
            catch (System.IO.IOException ex) { throw new System.IO.IOException(ex.Message + " - CheckFile"); }
            catch (UnauthorizedAccessException ex) { throw new UnauthorizedAccessException(ex.Message + " - CheckFile"); }
            catch (ArgumentNullException ex) { throw new ArgumentNullException(ex.Message + " - CheckFile"); }
            catch (NotSupportedException ex) { throw new NotSupportedException(ex.Message + " - CheckFile"); }
        }
        #endregion


    }
}
