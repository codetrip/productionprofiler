
//using System;
//using System.Collections.Specialized;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;
//using Microsoft.VisualStudio.WebHost;
//using NUnit.Framework;

//namespace ProductionProfiler.Tests.Integration
//{
//    [TestFixture]
//    public class WebTests
//    {
//        private Server _server;
//        private const int Port = 8085;
//        private const string WebServerVDir = "/";
//        private readonly string _tempPath = AppDomain.CurrentDomain.BaseDirectory;
//        private readonly string _tempBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
//        private string _webServerUrl; //built in Setup
//        private readonly StringCollection _extractedFilesToCleanup = new StringCollection();

//        [TestFixtureSetUp]
//        public void Setup()
//        {
//            //Extract the web.config and test cases (case sensitive!)
//            ExtractResource("web.config", _tempPath);
//            ExtractResource("test1.aspx", _tempPath);
//            ExtractResource("test2.aspx", _tempPath);

//            //NOTE: Cassini is going to load itself AGAIN into another AppDomain,
//            // and will be getting it's Assembliesfrom the BIN, including another copy of itself!
//            // Therefore we need to do this step FIRST because I've removed Cassini from the GAC

//            //Copy our assemblies down into the web server's BIN folder
//            Directory.CreateDirectory(_tempBinPath);
//            foreach (string file in Directory.GetFiles(_tempPath, "*.dll"))
//            {
//                string newFile = Path.Combine(_tempBinPath, Path.GetFileName(file));
//                if (File.Exists(newFile)) { File.Delete(newFile); }
//                File.Copy(file, newFile);
//            }

//            //Start the internal Web Server
//            _server = new Server(Port, WebServerVDir, _tempPath);
//            _webServerUrl = String.Format("http://localhost:{0}{1}", Port, WebServerVDir);
//            _server.Start();

//            //Let everyone know
//            Debug.WriteLine(String.Format("Web Server started on port {0} with VDir {1} in physical directory {2}", Port, WebServerVDir, _tempPath));
//        }

//        private string ExtractResource(string filename, string directory)
//        {
//            Assembly a = Assembly.GetExecutingAssembly();
//            string filePath;
//            using (Stream stream = a.GetManifestResourceStream("ProductionProfiler.Tests." + filename))
//            {
//                filePath = Path.Combine(directory, filename);
//                using (StreamWriter outfile = File.CreateText(filePath))
//                {
//                    using (StreamReader infile = new StreamReader(stream))
//                    {
//                        outfile.Write(infile.ReadToEnd());
//                    }
//                }
//            }
//            _extractedFilesToCleanup.Add(filePath);
//            return filePath;
//        }

//        [Test]
//        public void Init()
//        {
            
//        }
//    }
//}
