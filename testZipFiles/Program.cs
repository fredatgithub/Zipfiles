using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

namespace testZipFiles
{
  public class Program
  {
    private static void Main()
    {
      Action<string> display = Console.WriteLine;
      //var dateMin = DateTime.MinValue;
      //var dateMax = DateTime.MaxValue;
      //display($"minimum date is {dateMin}");
      //display($"maximum date is {dateMax}");

      var invalidPathChars = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
      var yesterday = DateTime.Now.AddDays(-1).ToShortDateString();
      display($"yesterday was {yesterday}");
      display("list of dates:");
      string testPath = $"{Path.GetTempPath()}\\UnitTests";
      foreach (string dateFound in GetListOfDatesFromfileNames(testPath).OrderBy(int.Parse))
      {
        display(dateFound);
      }

      foreach (string dateFound in GetListOfDatesFromfileNames(testPath))
      {
        ZipLogFiles(dateFound, testPath, 10);
      }

      DeleteHimZipLogFiles(@"C:\directory\directory.Logs");
      int zipDeletionExpireness = Properties.Settings.Default.HIMZipLogFileDeletionFrequencyInDays;
      display($"log files have been zipped and zip files older than {zipDeletionExpireness} day{Plural(zipDeletionExpireness)} have been deleted");

      display(string.Empty);
      display("press any key to exit:");
      Console.ReadKey();
    }

    private static string Plural(int number)
    {
      return number > 1 ? "s" : string.Empty;
    }

    public static List<string> GetListOfDatesFromfileNames(string directoryPath)
    {
      List<string> result = new List<string>();
      if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
      {
        return result;
      }

      List<string> allFiles = new List<string>();
      List<string> datedFiles = new List<string>();
      DirectoryInfo directory = new DirectoryInfo(directoryPath);
      allFiles = directory.GetFiles("*").Select(f => f.Name).ToList();
      Regex rgx = new Regex(@"\d{8}$");
      foreach (string file in allFiles)
      {
        Match match = rgx.Match(file);
        if (!string.IsNullOrEmpty(match.ToString()))
        {
          datedFiles.Add(file.Substring(file.Length - 8));
        }
      }

      allFiles = null;
      result = datedFiles.Distinct().ToList();
      datedFiles = null;
      return result;
    }

    public static void ZipLogFiles(string dateToZip, string filePath, int numberOfDays, int zipLevel = 9)
    {
      DateTime tmpDateToZip;
      string yesterday = string.Empty;
      if (string.IsNullOrEmpty(dateToZip) || dateToZip.Length != 8 || DateTime.TryParse(dateToZip, out tmpDateToZip))
      {
        DateTime yesterdayDateTime = DateTime.Today.AddDays(-1);
        yesterday = $"{yesterdayDateTime.Year}{yesterdayDateTime.Month}{yesterdayDateTime.Day}";
      }
      else
      {
        yesterday = dateToZip;
      }

      string yesterdayDateFormat = $"*.log{yesterday}";
      string finalZipFileName = $"logs_{yesterday}.zip";
      DirectoryInfo directory = new DirectoryInfo(filePath);
      List<string> allYesterdayfiles = new List<string>();
      try
      {
        allYesterdayfiles = directory.GetFiles(yesterdayDateFormat).Select(f => f.Name).ToList();
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception);
        throw;
      }

      if (allYesterdayfiles.Count != 0)
      {
        ZipFiles(filePath, allYesterdayfiles, finalZipFileName);
      }
    }

    /// <summary>Zip all files in the same directory and delete them afterwards.</summary>
    /// <param name="folderPath">The path where to zip files.</param>
    /// <param name="listOfFiles">The list of all the files to be zipped.</param>
    /// <param name="zipFileName">The name of the zip file.</param>
    /// <param name="zipLevel">
    /// The level of compression, from 0 to 9, 0 is the lowest compression and 9 is the highest compression, 9 by default.
    /// </param>
    public static void ZipFiles(string folderPath, IEnumerable<string> listOfFiles, string zipFileName, int zipLevel = 9)
    {
      ZipOutputStream zipStream = null;
      try
      {
        if (File.Exists(Path.Combine(folderPath, zipFileName)))
        {
          zipStream = new ZipOutputStream(File.OpenWrite(Path.Combine(folderPath, zipFileName))); // open existing zip file
        }
        else
        {
          zipStream = new ZipOutputStream(File.Create(Path.Combine(folderPath, zipFileName))); // create new zip file
        }

        using (zipStream)
        {
          if (zipLevel < 0 || zipLevel > 9)
          {
            zipLevel = 9; // 9 is the maximum level of compression, we take it by default (level from 0 to 9)
          }

          zipStream.SetLevel(zipLevel);
          foreach (string fileName in listOfFiles)
          {
            ZipEntry zipEntry = new ZipEntry(fileName);
            zipStream.PutNextEntry(zipEntry);
            using (FileStream fileStream = File.OpenRead(Path.Combine(folderPath, fileName)))
            {
              byte[] buffer = new byte[fileStream.Length];
              fileStream.Read(buffer, 0, buffer.Length);
              zipStream.Write(buffer, 0, buffer.Length);
            }
          }
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine($"Exception found: {exception.Message}");
        Console.WriteLine("press a key to continue to next exception");
        Console.ReadKey();
      }
      finally
      {
        if (zipStream != null)
        {
          zipStream.Finish();
          zipStream.Close();
          zipStream.Dispose();
        }
      }

      //deleting only log files which have been zipped successfully
      try
      {
        IEnumerable<string> allFileNameFromZipfile = GetAllFileNameFromZipFile(Path.Combine(folderPath, zipFileName));
        if (allFileNameFromZipfile.Count() != 0)
        {
          foreach (string file in listOfFiles)
          {
            if (allFileNameFromZipfile.Contains(file))
            {
              if (File.Exists(Path.Combine(folderPath, file)))
              {
                File.Delete(Path.Combine(folderPath, file));
              }
            }
          }
        }
      }
      catch (Exception exception)
      {
        // ignored here in helper class
        Console.WriteLine(exception.Message);
        Console.WriteLine(string.Empty);
      }
    }

    public static IEnumerable<string> GetAllFileNameFromZipFile(string zipFileName)
    {
      List<string> result = new List<string>();
      ZipFile zipFile = null;
      try
      {
        using (FileStream fileStream = File.OpenRead(zipFileName))
        {
          zipFile = new ZipFile(fileStream);
          result.AddRange(from ZipEntry zipEntry in zipFile select zipEntry.Name);
        }
      }
      finally
      {
        if (zipFile != null)
        {
          zipFile.IsStreamOwner = true; // close and also shut the underlying stream
          zipFile.Close(); // Ensure we release resource
        }
      }

      return result;
    }

    public static void DeleteHimZipLogFiles(string filePath)
    {
      try
      {
        string filterPattern = "logs_*.zip";
        string[] files = Directory.GetFiles(filePath, filterPattern);
        foreach (var file in files)
        {
          // Get file date from zip file 
          // example: file = C:\HIM\Him.Logs\logs_20171103.zip
          if (file.Length < 12)
          {
            continue;
          }

          string fileDate = file.Substring(file.Length - 12, 8);
          int yearFileName;
          if (!int.TryParse(fileDate.Substring(0, 4), out yearFileName))
          {
            continue;
          }

          int monthFileName;
          if (!int.TryParse(fileDate.Substring(4, 2), out monthFileName))
          {
            continue;
          }

          int dayFileName;
          if (!int.TryParse(fileDate.Substring(6, 2), out dayFileName))
          {
            continue;
          }

          DateTime fileNameDate;
          if (!DateTime.TryParse($"{yearFileName}-{monthFileName}-{dayFileName}", out fileNameDate))
          {
            continue;
          }

          TimeSpan diffDate = DateTime.Now - fileNameDate;
          if (diffDate.Days > Properties.Settings.Default.HIMZipLogFileDeletionFrequencyInDays)
          {
            try
            {
              if (!IsFileLocked(new FileInfo(file)))
              {
                File.Delete(file);
              }
            }
            catch (Exception exception)
            {
              Console.WriteLine($"The deletion of the file {file} failed in the directory {filePath}. The exception is {exception.Message}");
              Console.WriteLine(MethodBase.GetCurrentMethod());
            }
          }
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine($"Error while trying to get all zip files in the directory {filePath}. The exception is {exception.Message}");
        Console.WriteLine(MethodBase.GetCurrentMethod());
      }
    }

    public static bool FileIsDeletable(string fileName, int numberOfDays)
    {
      bool result = false;
      if (fileName.Length < 9)
      {
        return false;
      }

      string fileDate = fileName.Substring(fileName.Length - 8, 8);
      int yearFileName;
      if (!int.TryParse(fileDate.Substring(0, 4), out yearFileName))
      {
        return false;
      }

      int monthFileName;
      if (!int.TryParse(fileDate.Substring(4, 2), out monthFileName))
      {
        return false;
      }

      int dayFileName;
      if (!int.TryParse(fileDate.Substring(6, 2), out dayFileName))
      {
        return false;
      }

      DateTime fileNameDate;
      if (!DateTime.TryParse($"{yearFileName}-{monthFileName}-{dayFileName}", out fileNameDate))
      {
        return false;
      }

      TimeSpan diffDate = DateTime.Now - fileNameDate;
      if (diffDate.Days > Properties.Settings.Default.HIMZipLogFileDeletionFrequencyInDays)
      {
        return true;
      }

      return result;
    }

    public static bool IsFileLocked(FileInfo fileName)
    {
      FileStream stream = null;

      try
      {
        stream = fileName.Open(FileMode.Open, FileAccess.Read, FileShare.None);
      }
      catch (IOException)
      {
        //the file is unavailable because it is
        //still being written to or being processed by another thread
        return true;
      }
      finally
      {
        stream?.Close();
        stream?.Dispose();
      }

      //file is not locked
      return false;
    }
  }
}
