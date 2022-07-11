using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using WinSCP;
using System.Configuration;
 

namespace BE.Utilities.SFTP
{
    
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Setup session options
                string hostName = ConfigurationManager.AppSettings["Domain"].ToString();
                string userName = ConfigurationManager.AppSettings["User"].ToString();
                string password = ConfigurationManager.AppSettings["Password"].ToString();
                string fingerprint = ConfigurationManager.AppSettings["Fingerprint"].ToString();
                
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = hostName,
                    UserName = userName,
                    Password = password,
                    SshHostKeyFingerprint = fingerprint
                };

                
                using (Session session = new Session())
                {
                    FTPLog.logger.Info("Starting SFTP transmisson...");
                    FTPLog.logger.Info("Opening SFTP Session...");

                    // Connect
                    session.Open(sessionOptions);

                    FTPLog.logger.Info("Uploading file(s)...");
                    
                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.FilePermissions = null; // This is default
                    transferOptions.PreserveTimestamp = false;

                    TransferOperationResult transferResult;
                    string UploadFromLocation = ConfigurationManager.AppSettings["LocationUploadFrom"].ToString();
                    string UploadToLocation = ConfigurationManager.AppSettings["LocationUploadTo"].ToString();
                    transferResult = session.PutFiles(@UploadFromLocation, UploadToLocation, false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Log results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        FTPLog.logger.Info(String.Format("Upload of {0} succeeded", transfer.FileName));
                    }
                }
                FTPLog.logger.Info("Finished uploading file(s)");
            }
            catch (Exception e)
            {
                FTPLog.logger.Error(String.Format("Error: {0}", e.ToString()));
            }
            WaitConsole();
        }

        private static void WaitConsole()
        {
            Console.WriteLine("");
            Console.WriteLine("...done...");
            Console.ReadKey();
        }
    }


    public class FTPLog
    {
        public static readonly ILog logger = 
              LogManager.GetLogger(typeof(FTPLog));
    
        static FTPLog()
        {
            XmlConfigurator.Configure();
        }
    }
}
