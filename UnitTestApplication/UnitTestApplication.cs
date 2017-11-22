using Microsoft.VisualStudio.TestTools.UnitTesting;
using testZipFiles;
namespace UnitTestApplication
{
  [TestClass]
  public class UnitTestApplication
  {
    [TestMethod]
    public void TestMethodDeletable_file_name_is_ok()
    {
      PrivateType privateTypeObject = new PrivateType(typeof(Program));
      const string methodName = "Deletable";
      const string source = "logs_20171103";
      const bool expected = true;
      object obj = privateTypeObject.InvokeStatic(methodName, source);
      Assert.AreEqual(expected, (bool)obj);
    }

    [TestMethod]
    public void TestMethodDeletable_file_name_is_ok2()
    {
      PrivateType privateTypeObject = new PrivateType(typeof(Program));
      const string methodName = "Deletable";
      const string source = "messages.log20171103";
      const bool expected = true;
      object obj = privateTypeObject.InvokeStatic(methodName, source);
      Assert.AreEqual(expected, (bool)obj);
    }
    
    [TestMethod]
    public void TestMethodDeletable_file_name_is_ko_txt()
    {
      PrivateType privateTypeObject = new PrivateType(typeof(Program));
      const string methodName = "Deletable";
      const string source = "messageslog20171103_test.txt";
      const bool expected = false;
      object obj = privateTypeObject.InvokeStatic(methodName, source);
      Assert.AreEqual(expected, (bool)obj);
    }

    [TestMethod]
    public void TestMethodDeletable_file_name_is_ko_zip()
    {
      PrivateType privateTypeObject = new PrivateType(typeof(Program));
      const string methodName = "Deletable";
      const string source = "messageslog20171103.zip";
      const bool expected = false;
      object obj = privateTypeObject.InvokeStatic(methodName, source);
      Assert.AreEqual(expected, (bool)obj);
    }
  }
}