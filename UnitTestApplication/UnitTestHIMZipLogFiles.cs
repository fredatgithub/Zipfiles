using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zipFunctions = testZipFiles.Program;
namespace UnitTestApplication
{
    [TestClass]
    public class UnitTestHIMZipLogFiles
    {
        #region FileIsDeletable

        [TestMethod]
        public void TestMethod_FileIsDeletable_length_smaller_than_8_characters()
        {
            const string source = "log2017";
            const bool expected = false;
            bool result = zipFunctions.FileIsDeletable(source);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestMethod_FileIsDeletable_wrong_year()
        {
            const string source = "log_201711";
            const bool expected = false;
            bool result = zipFunctions.FileIsDeletable(source);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestMethod_FileIsDeletable_wrong_month()
        {
            const string source = "log_2017mm20";
            const bool expected = false;
            bool result = zipFunctions.FileIsDeletable(source);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestMethod_FileIsDeletable_wrong_day()
        {
            const string source = "log_201711dd";
            const bool expected = false;
            bool result = zipFunctions.FileIsDeletable(source);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestMethod_FileIsDeletable_wrong_date()
        {
            const string source = "log_20170231";
            const bool expected = false;
            bool result = zipFunctions.FileIsDeletable(source);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestMethod_FileIsDeletable_correct_date_but_not_deletable()
        {
            const string source = "log_99991231";
            const bool expected = false;
            bool result = zipFunctions.FileIsDeletable(source);
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
            bool result = zipFunctions.FileIsDeletable(source);
            Assert.AreEqual(result, expected);
        }

        #endregion FileIsDeletable
        #region GetListOfDatesFromfileNames

        [TestMethod]
        public void TestMethod_GetListOfDatesFromfileNames_correct()
        {
            string workingPath = $"{Path.GetTempPath()}\\HimUnitTests" ;
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

            List<string> result = zipFunctions.GetListOfDatesFromfileNames(workingPath);
            expected.Sort();
            result.Sort();
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(expected[i], result[i]);
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
                List<string> allFiles = directory.GetFiles("*").Select(f=>f.Name).ToList();
                foreach (string file in allFiles)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
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
            string workingPath = $"{Path.GetTempPath()}\\HimUnitTests";
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

            List<string> result = zipFunctions.GetListOfDatesFromfileNames(workingPath);
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
            string workingPath = $"{Path.GetTempPath()}\\HimUnitTests";
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

            List<string> result = zipFunctions.GetListOfDatesFromfileNames(workingPath);
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

        #endregion GetListOfDatesFromfileNames
        #region ZipLogFiles

        #endregion ZipLogFiles
    }
}