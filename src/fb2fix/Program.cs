namespace Vurdalakov
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Ionic.Zip;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("royallib <filename.fb2.zip> | <mask*.fb2.zip>");
                Environment.Exit(1);
            }

            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(args[0]));
            var files = directoryInfo.GetFiles(Path.GetFileName(args[0]));

            foreach (var file in files)
            {
                Process(file.FullName);
            }
        }

        private static void Process(String zipFileName)
        {
            Console.WriteLine("\nProcessing '{0}'", zipFileName);

            var encoding = Encoding.GetEncoding(866);

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var tempFileName = "";

                var readOptions = new ReadOptions();
                readOptions.Encoding = encoding;

                using (var zipFile = ZipFile.Read(zipFileName, readOptions))
                {
                    foreach (var zipEntry in zipFile)
                    {
                        if (Path.GetExtension(zipEntry.FileName).Equals(".fb2", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tempFileName = zipEntry.FileName;

                            Console.WriteLine("Processing '{0}'", tempFileName);
                            zipEntry.Extract(temporaryDirectory.DirectoryInfo.FullName, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }

                if (String.IsNullOrEmpty(tempFileName))
                {
                    Console.WriteLine("FB2 file not found");
                    Environment.Exit(2);
                }

                var fileName = Path.ChangeExtension(tempFileName, null);
                fileName = fileName.Replace("royallib.com", "");
                fileName = fileName.Replace(" - ", "");
                fileName = fileName.Trim() + ".fb2";

                temporaryDirectory.Rename(tempFileName, fileName);
                Console.WriteLine("Renamed to '{0}'", fileName);

                var fb2FileName = temporaryDirectory.CreateFullName(fileName);

                var fb2 = File.ReadAllText(fb2FileName);

                var windows1251regrex = @" encoding\s*=\s*""windows-1251""";
                if (Regex.IsMatch(fb2, windows1251regrex))
                {
                    fb2 = File.ReadAllText(fb2FileName, Encoding.GetEncoding(1251));
                    fb2 = Regex.Replace(fb2, windows1251regrex, @" encoding=""utf-8""");
                }

                fb2 = Regex.Replace(fb2, "<myheader>.*</myheader>", "");
                fb2 = Regex.Replace(fb2, "<myfooter>.*</myfooter>", "");

                File.WriteAllText(fb2FileName, fb2);

                var fb2zipFileName = Path.Combine(Path.GetDirectoryName(zipFileName), fileName) + ".zip";

                if (File.Exists(fb2zipFileName))
                {
                    File.Delete(fb2zipFileName);
                }

                using (var zipFile = new ZipFile(encoding))
                {
                    zipFile.AddFile(fb2FileName, "");
                    zipFile.Save(fb2zipFileName);
                }

                Console.WriteLine("Packed  to '{0}'", fb2zipFileName);

                if (!zipFileName.Equals(fb2zipFileName))
                {
                    File.Delete(zipFileName);
                }
            }
        }
    }
}
