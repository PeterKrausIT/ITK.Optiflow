// ***********************************************************************
// Assembly         : UnitTest.Adapter.FTP
// Author           : Nils Kraus
// Created          : 03-09-2020
// ***********************************************************************
// <summary>UnitTest.Adaptor.FTP.cs
// FTPAdapterTest.cs
// Unit tests for the GetFTP and the SendFTP adapter
// </summary>
// ***********************************************************************
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Activities;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The UnitTestFTPAdapter namespace.
/// </summary>
namespace UnitTestFTPAdapter
{
    /// <summary>
    /// This is the class for FTPAdapter unit testing.
    /// </summary>
    [TestClass]
    public class FTPAdapterTest
    {
        /// <summary>
        /// TestMethodFTPAdapter
        /// First writes a testfile to the ftp server,
        /// then downloads the file and changes it into .BAK,
        /// then checks if number and content of testfile is correct.
        /// </summary>
        [TestMethod]
        public void TestMethodFTPAdapter()
        {
            OF.Adapter.GetFTP.GetFTP gftp = new OF.Adapter.GetFTP.GetFTP();
            OF.Adapter.SendFTP.SendFTP sftp = new OF.Adapter.SendFTP.SendFTP();


            string filename1 = "Test." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".1.TST";
            string filename2 = "Test." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".2.TST";
           
            var inputSend = new Dictionary<string, object>
            {
                { "FTPServer", "ftp://preislistendruck.de" },
                { "Username", "OptiFlow" },
                { "Password", "flow!1Opti" },
                { "Message", "This is just a test file." },
                { "Filename", filename1 },
                {"Subdir", "" }
            };

            var inputSend2 = new Dictionary<string, object>
            {
                { "FTPServer", "ftp://preislistendruck.de" },
                { "Username", "OptiFlow" },
                { "Password", "flow!1Opti" },
                { "Message", "This is just a test file." },
                { "Filename", filename2 },
                {"Subdir", "" }
            };

            var outputSend = WorkflowInvoker.Invoke<int>(sftp, inputSend);
            var outputSend2 = WorkflowInvoker.Invoke<int>(sftp, inputSend2);

            var inputGet = new Dictionary<string, object>
            {
                { "FTPServer", "ftp://Preislistendruck.de" },
                { "Username", "OptiFlow" },
                { "Password", "flow!1Opti" },
                { "MsgExtension", "TST" },
                { "RenameWhenDone", "BAK" }
            };

            IDictionary<string, object> outputGet = new Dictionary<string, object>
            {
                { "Message", "" }
            };

            var outputs = WorkflowInvoker.Invoke<int>(gftp, inputGet, out outputGet, new TimeSpan(0, 0, 10));

            Assert.AreEqual(outputGet.ContainsKey("Message"), true);
            Assert.AreEqual(outputGet["Message"], "This is just a test file.");
            Assert.AreEqual(outputs, 2);

            var inputGet2 = new Dictionary<string, object>
            {
                { "FTPServer", "ftp://Preislistendruck.de" },
                { "Username", "OptiFlow" },
                { "Password", "flow!1Opti" },
                { "MsgExtension", "TST" },
                { "RenameWhenDone", "BAK" }
            };
            IDictionary<string, object> outputGet2 = new Dictionary<string, object>
            {
                { "Message", "" }
            };

            var outputs2 = WorkflowInvoker.Invoke<int>(gftp, inputGet2, out outputGet2, new TimeSpan(0, 0, 10));

            Assert.AreEqual(outputGet2.ContainsKey("Message"), true);
            Assert.AreEqual(outputGet2["Message"], "This is just a test file.");
            Assert.AreEqual(outputs2, 1);


        }

        /// <summary>
        /// TestMethodFTPAdapterErr
        /// First writes a testfile to the ftp server but with non working filename
        /// Checks if error is noticed
        /// </summary>
        [TestMethod]
        public void TestMethodFTPAdapterErr()
        {
            OF.Adapter.GetFTP.GetFTP gftp = new OF.Adapter.GetFTP.GetFTP();
            OF.Adapter.SendFTP.SendFTP sftp = new OF.Adapter.SendFTP.SendFTP();


            string filename1 = "Test*." + DateTime.Now.ToString("yyyy-MM-dd:HH:mm:ss") + ".3.TST.?";

            var inputSend = new Dictionary<string, object>
            {
                { "FTPServer", "ftp://Preislistendruck" },
                { "Username", "OptiFlow" },
                { "Password", "flow!1Opti" },
                { "Message", "This is just a test file." },
                { "Filename", filename1 },
                {"Subdir", "" }
            };

            // TODO: implement this test...
            try
            {
                var outputSend = WorkflowInvoker.Invoke<int>(sftp, inputSend);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                // Check if the exception is of the expected type or contains the expected message
                Assert.IsTrue(ex is System.Net.WebException || ex.InnerException is System.Net.WebException, "Unexpected exception type: " + ex.GetType().ToString());
            }
        }

    }
}
