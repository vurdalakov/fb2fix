namespace Vurdalakov
{
    using System;
    using System.IO;
    using System.Reflection;

    public class TemporaryDirectory : IDisposable
    {
        private Boolean dontDelete = false;

        #region IDisposable

        ~TemporaryDirectory()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                // free managed resources
            }

            // free native resources if there are any.

            if (this.DirectoryInfo != null)
            {
                if (!this.dontDelete)
                {
                    try
                    {
                        this.DirectoryInfo.Delete(true);
                        this.DirectoryInfo = null;
                    }
                    catch { }
                }
            }
        }

        #endregion

        public DirectoryInfo DirectoryInfo { get; private set; }

        public TemporaryDirectory(Boolean dontDelete = false) : this(null, null, dontDelete)
        {
        }

        public TemporaryDirectory(String prefix, String extension = null, Boolean dontDelete = false)
        {
            this.dontDelete = dontDelete;

            CreateDirectory(prefix, extension);
        }

        public void Rename(String oldFileName, String newFileName)
        {
            oldFileName = this.CreateFullName(oldFileName);
            newFileName = this.CreateFullName(newFileName);

            if (File.Exists(oldFileName))
            {
                File.Move(oldFileName, newFileName);
            }
            else if (Directory.Exists(oldFileName))
            {
                Directory.Move(oldFileName, newFileName);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public void Delete(String fileName)
        {
            fileName = this.CreateFullName(fileName);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            else if (Directory.Exists(fileName))
            {
                Directory.Delete(fileName, true);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public String CreateFullName(String fileName)
        {
            return Path.Combine(this.DirectoryInfo.FullName, fileName);
        }

        private void CreateDirectory(String prefix = null, String extension = null)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                prefix = Assembly.GetExecutingAssembly().GetModules()[0].Name;
            }

            if (String.IsNullOrEmpty(extension))
            {
                extension = "tmp";
            }

            var mask = prefix + ".{0:X08}";
            mask = Path.ChangeExtension(mask, extension);
            mask = Path.Combine(Path.GetTempPath(), mask);

            var random = new Random();

            var directoryName = "";
            do
            {
                directoryName = String.Format(mask, random.Next(0x10000000, 0x7FFFFFFF));
            }
            while (Directory.Exists(directoryName) || File.Exists(directoryName));

            this.DirectoryInfo = Directory.CreateDirectory(directoryName);
        }
    }
}
