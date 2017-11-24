using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zipFunctions = testZipFiles.Program;
//using zipFunctions = Him.Tools.HimZip;
//using functionsFromAdmin = Him.Admin.InstallAndUpdateManager.TaskRunner;

namespace UnitTestApplication
{
  [TestClass]
  public class UnitTestZipLogFiles
  {
    #region FileIsDeletable

    [TestMethod]
    public void TestMethod_FileIsDeletable_length_smaller_than_8_characters()
    {
      const string source = "log2017";
      const bool expected = false;
      bool result = zipFunctions.FileIsDeletable(source, 10);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_FileIsDeletable_wrong_year()
    {
      const string source = "log_201711";
      const bool expected = false;
      bool result = zipFunctions.FileIsDeletable(source, 10);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_FileIsDeletable_wrong_month()
    {
      const string source = "log_2017mm20";
      const bool expected = false;
      bool result = zipFunctions.FileIsDeletable(source, 10);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_FileIsDeletable_wrong_day()
    {
      const string source = "log_201711dd";
      const bool expected = false;
      bool result = zipFunctions.FileIsDeletable(source, 10);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_FileIsDeletable_wrong_date()
    {
      const string source = "log_20170231";
      const bool expected = false;
      bool result = zipFunctions.FileIsDeletable(source, 10);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_FileIsDeletable_correct_date_but_not_deletable()
    {
      const string source = "log_99991231";
      const bool expected = false;
      bool result = zipFunctions.FileIsDeletable(source, 10);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_FileIsDeletable_correct_date_but_deletable()
    {
      DateTime yesterday = DateTime.Now.AddDays(-1000);
      string month = yesterday.Month.ToString().Length == 1 ? "0" + yesterday.Month : yesterday.Month.ToString();
      string day = yesterday.Day.ToString().Length == 1 ? "0" + yesterday.Day : yesterday.Day.ToString();
      string source = $"log_{yesterday.Year}{month}{day}";
      const bool expected = true;
      bool result = zipFunctions.FileIsDeletable(source, 10);
      Assert.AreEqual(result, expected);
    }

    #endregion FileIsDeletable
    #region GetListOfDatesFromfileNames

    [TestMethod]
    public void TestMethod_GetListOfDatesFromfileNames_correct()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      List<string> expected = new List<string> { "99991231", "20171121", "20160101", "20150102", "20141121" };
      //clean up temp directory before running unit tests
      foreach (string file in expected)
      {
        if (File.Exists(Path.Combine(workingPath, $"log_{file}")))
        {
          File.Delete(Path.Combine(workingPath, $"log_{file}"));
        }
      }

      List<string> source2 = new List<string> { "log_99991231", "log_20171121", "log_20160101", "log_20150102", "log_20141121" };
      foreach (string file in source2)
      {
        if (!File.Exists(file))
        {
          using (File.Create(Path.Combine(workingPath, file)))
          {
            //just create a new file
          }
        }
      }

      IEnumerable<string> result1 = zipFunctions.GetListOfDatesFromfileNames(workingPath);
      List<string> result2 = result1.ToList();
      expected.Sort();
      result2.Sort();
      for (int i = 0; i < result2.Count; i++)
      {
        Assert.AreEqual(expected[i], result2[i]);
      }

      //clean-up
      foreach (string file in source2)
      {
        if (File.Exists(Path.Combine(workingPath, file)))
        {
          File.Delete(Path.Combine(workingPath, file));
        }
      }
    }

    private void PrepareDirectoryTest(string directoryPath)
    {
      //delete all files in HimUnitTest directory if it exists or
      // create a brand new HimUnitTest directory 
      if (Directory.Exists(directoryPath))
      {
        // delete all files
        DirectoryInfo directory = new DirectoryInfo(directoryPath);
        List<string> allFiles = directory.GetFiles("*").Select(f => f.Name).ToList();
        foreach (string file in allFiles)
        {
          if (File.Exists(Path.Combine(directoryPath, file)))
          {
            File.Delete(Path.Combine(directoryPath, file));
          }
        }
      }
      else
      {
        Directory.CreateDirectory(directoryPath);
      }
    }

    [TestMethod]
    public void TestMethod_GetListOfDatesFromfileNames_not_correct_because_result_count_is_different()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      List<string> expected = new List<string> { "99991231", "20171121", "20160101", "20150102", "20141121" };
      foreach (string file in expected)
      {
        if (File.Exists(Path.Combine(workingPath, $"log_{file}")))
        {
          File.Delete(Path.Combine(workingPath, $"log_{file}"));
        }
      }

      List<string> source2 = new List<string> { "log_99991231", "log_20160101", "log_20150102", "log_20141121" };
      foreach (string file in source2)
      {
        if (!File.Exists(Path.Combine(workingPath, file)))
        {
          using (File.Create(Path.Combine(workingPath, file)))
          {
            //just create a new file
          }
        }
      }

      IEnumerable<string> result1 = zipFunctions.GetListOfDatesFromfileNames(workingPath);
      List<string> result = result1.ToList();
      expected.Sort();
      result.Sort();
      bool finalResult = true;
      if (expected.Count == result.Count)
      {
        for (int i = 0; i < result.Count; i++)
        {
          if (expected[i] != result[i])
          {
            finalResult = false;
            break;
          }
        }
      }
      else
      {
        finalResult = false;
      }

      Assert.IsFalse(finalResult);

      //clean-up
      foreach (string file in source2)
      {
        if (File.Exists(file))
        {
          File.Delete(Path.Combine(workingPath, file));
        }
      }
    }

    [TestMethod]
    public void TestMethod_GetListOfDatesFromfileNames_not_correct_because_items_are_different()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      List<string> expected = new List<string> { "99991231", "20171121", "20160101", "20150102", "20141121" };
      //Clean up temp directory before running unit tests
      foreach (string file in expected)
      {
        if (File.Exists(Path.Combine(workingPath, $"log_{file}")))
        {
          File.Delete(Path.Combine(workingPath, $"log_{file}"));
        }
      }

      List<string> source2 = new List<string> { "log_99991231", "log_20160101", "log_20150102", "log_20141121" };
      foreach (string file in source2)
      {
        if (!File.Exists(Path.Combine(workingPath, file)))
        {
          using (File.Create(Path.Combine(workingPath, file)))
          {
            //just create a new file
          }
        }
      }

      IEnumerable<string> result1 = zipFunctions.GetListOfDatesFromfileNames(workingPath);
      List<string> result = result1.ToList();
      expected.Sort();
      result.Sort();
      bool finalResult = true;
      if (expected.Count == result.Count)
      {
        for (int i = 0; i < result.Count; i++)
        {
          if (expected[i] != result[i])
          {
            finalResult = false;
            break;
          }
        }
      }
      else
      {
        finalResult = false;
      }

      Assert.IsFalse(finalResult);

      //clean-up
      foreach (string file in source2)
      {
        if (File.Exists(file))
        {
          File.Delete(Path.Combine(workingPath, file));
        }
      }
    }

    #region IsFileLocked
    [TestMethod]
    public void TestMethod_IsFileLocked_returns_false()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const bool expected = false;
      const string source = "log_99991231";
      using (File.Create(Path.Combine(workingPath, source)))
      {
        //just create a new file
      }

      bool result = zipFunctions.IsFileLocked(new FileInfo(Path.Combine(workingPath, source)));
      Assert.AreEqual(expected, result);

      //clean-up
      if (File.Exists(Path.Combine(workingPath, source)))
      {
        File.Delete(Path.Combine(workingPath, source));
      }
    }

    [TestMethod]
    public void TestMethod_IsFileLocked_returns_true()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const bool expected = true;
      const string source = "log_99991231";
      StreamWriter sw = new StreamWriter(Path.Combine(workingPath, source));
      sw.WriteLine("test");
      bool result = zipFunctions.IsFileLocked(new FileInfo(Path.Combine(workingPath, source)));
      sw.Close();
      Assert.AreEqual(expected, result);

      //clean-up
      if (File.Exists(Path.Combine(workingPath, source)))
      {
        File.Delete(Path.Combine(workingPath, source));
      }
    }
    #endregion
    #endregion GetListOfDatesFromfileNames

    #region ZipLogFiles
    [TestMethod]
    public void TestMethod_ZipLogFiles_zip_and_delete_1_file()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const bool expected = true;
      const string source = "test.log99991231";
      const int source2 = 10;
      using (File.Create(Path.Combine(workingPath, source)))
      {
        //just create a new file
      }

      // Source file should have been deleted
      zipFunctions.ZipLogFiles("99991231", workingPath, source2);
      bool result = !File.Exists(Path.Combine(workingPath, source));
      Assert.AreEqual(expected, result);

      // Zip file should have been created
      string ExpectedZipfile = "logs_99991231.zip";
      result = File.Exists(Path.Combine(workingPath, ExpectedZipfile));
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_ZipLogFiles_zip_and_delete_1_file_failed()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const bool expected = false;
      const string source = "test.log99991231";
      const int source2 = 10;
      using (File.Create(Path.Combine(workingPath, source)))
      {
        //just create a new file
      }

      // Source file should have been deleted
      zipFunctions.ZipLogFiles("20171124", workingPath, source2);
      bool result = !File.Exists(Path.Combine(workingPath, source));
      Assert.AreEqual(expected, result);

      // Zip file should have been created
      string ExpectedZipfile = "logs_99991231.zip";
      result = File.Exists(Path.Combine(workingPath, ExpectedZipfile));
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_ZipLogFiles_zip_and_delete_several_files_ok()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const bool expected = true;
      List<string> source = new List<string> { "test1.log99991231", "test2.log99991231", "test3.log99991231", "test4.log99991231" };
      const int source2 = 10;
      foreach (string file in source)
      {
        using (File.Create(Path.Combine(workingPath, file)))
        {
          //just create a new file
        }
      }

      // All the source files should have been deleted
      zipFunctions.ZipLogFiles("99991231", workingPath, source2);
      foreach (string file in source)
      {
        bool FileResult = !File.Exists(Path.Combine(workingPath, file));
        Assert.AreEqual(expected, FileResult);
      }

      // One zip file should have been created
      string ExpectedZipfile = "logs_99991231.zip";
      bool result = File.Exists(Path.Combine(workingPath, ExpectedZipfile));
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_ZipLogFiles_zip_and_delete_several_files_failed()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const bool expected = false;
      List<string> source = new List<string> { "test1.log99991231", "test2.log99991231", "test3.log99991231", "test4.log99991231" };
      const int source2 = 10;
      foreach (string file in source)
      {
        using (File.Create(Path.Combine(workingPath, file)))
        {
          //just create a new file
        }
      }

      // All the source files should have been deleted
      zipFunctions.ZipLogFiles("20171124", workingPath, source2);
      foreach (string file in source)
      {
        bool FileResult = !File.Exists(Path.Combine(workingPath, file));
        Assert.AreEqual(expected, FileResult);
      }

      // One zip file should have been created
      string ExpectedZipfile = "logs_99991231.zip";
      bool result = File.Exists(Path.Combine(workingPath, ExpectedZipfile));
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_ZipLogFiles_zip_and_delete_several_files_ok_despite_zip_level_equal_to_99()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const bool expected = true;
      List<string> source = new List<string> { "test1.log99991231", "test2.log99991231", "test3.log99991231", "test4.log99991231" };
      const int source2 = 10;
      const int source3 = 99;
      foreach (string file in source)
      {
        using (File.Create(Path.Combine(workingPath, file)))
        {
          //just create a new file
        }
      }

      // All the source files should have been deleted
      zipFunctions.ZipLogFiles("99991231", workingPath, source2, source3);
      foreach (string file in source)
      {
        bool FileResult = !File.Exists(Path.Combine(workingPath, file));
        Assert.AreEqual(expected, FileResult);
      }

      // One zip file should have been created
      string ExpectedZipfile = "logs_99991231.zip";
      bool result = File.Exists(Path.Combine(workingPath, ExpectedZipfile));
      Assert.AreEqual(expected, result);
    }
    #endregion ZipLogFiles

    #region GetAllFileNameFromZipFile

    [TestMethod]
    public void TestMethod_GetAllFileNameFromZipFile_with_1_file()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      const string expected = "test.log99991231";
      const string source = "test.log99991231";
      const int source2 = 10;
      using (File.Create(Path.Combine(workingPath, source)))
      {
        //just create a new file
      }

      zipFunctions.ZipLogFiles("99991231", workingPath, source2);
      string ExpectedZipfile = "logs_99991231.zip";
      List<string> result = zipFunctions.GetAllFileNameFromZipFile(Path.Combine(workingPath, ExpectedZipfile)).ToList();
      Assert.IsTrue(result.Count == 1);
      Assert.AreEqual(expected, result[0]);
    }

    [TestMethod]
    public void TestMethod_GetAllFileNameFromZipFile_with_several_files_ok()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      List<string> expected = new List<string> { "test1.log99991231", "test2.log99991231", "test3.log99991231", "test4.log99991231" };
      List<string> source = new List<string> { "test1.log99991231", "test2.log99991231", "test3.log99991231", "test4.log99991231" };
      const int source2 = 10;
      foreach (string file in source)
      {
        using (File.Create(Path.Combine(workingPath, file)))
        {
          //just create a new file
        }
      }

      zipFunctions.ZipLogFiles("99991231", workingPath, source2);
      string ExpectedZipfile = "logs_99991231.zip";
      List<string> result = zipFunctions.GetAllFileNameFromZipFile(Path.Combine(workingPath, ExpectedZipfile)).ToList();
      Assert.IsTrue(result.Count == 4);
      expected.Sort();
      source.Sort();
      for (int i = 0; i < expected.Count; i++)
      {
        Assert.AreEqual(expected[i], result[i]);
      }
    }

    [TestMethod]
    public void TestMethod_GetAllFileNameFromZipFile_with_several_files_failed()
    {
      string workingPath = $"{Path.GetTempPath()}\\UnitTests";
      PrepareDirectoryTest(workingPath);
      List<string> expected = new List<string> { "test1.log99991231", "test2.log99991231", "test3.log99991231", "test4.log99991231" };
      List<string> source = new List<string> { "test2.log99991231", "test3.log99991231", "test4.log99991231" };
      const int source2 = 10;
      foreach (string file in source)
      {
        using (File.Create(Path.Combine(workingPath, file)))
        {
          //just create a new file
        }
      }

      zipFunctions.ZipLogFiles("99991231", workingPath, source2);
      string ExpectedZipfile = "logs_99991231.zip";
      List<string> result = zipFunctions.GetAllFileNameFromZipFile(Path.Combine(workingPath, ExpectedZipfile)).ToList();
      Assert.IsFalse(result.Count == 4);
      expected.Sort();
      source.Sort();
      bool finalResult = true;
      for (int i = 0; i < source.Count; i++)
      {
        if (expected[i] != result[i])
        {
          finalResult = false;
        }
      }

      Assert.IsFalse(finalResult);
    }

    #endregion GetAllFileNameFromZipFile

  }
}