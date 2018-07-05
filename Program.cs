using SharpSvn;
using SharpSvn.Security;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;

namespace MySvnClient
{
    /// <summary>
    /// https://sharpsvn.open.collab.net/servlets/ProjectProcess?pageID=3794   官网
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var url = "仓库地址";
            var number = GetLatestRevision(url);
            var log= GetCommitLog(url, new SvnRevision(75034), new SvnRevision(number));
            File.AppendAllText(@"C:\Users\Administrator\Desktop\08commitLog.txt", log);
            File.AppendAllText(@"C:\Users\Administrator\Desktop\08commitLog.txt", "\r\n最新版本号：" + number);
            //SendEmail(log);
            Console.WriteLine("\r\n最新版本号：" + number);

        }




        /// <summary>
        /// 获取最新的版本号
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static long GetLatestRevision(string url)
        {
            using (SvnClient client = GetSvnClient())
            {
                SvnInfoEventArgs svnInfo;
                if (client.GetInfo(new SvnUriTarget(url), out svnInfo))
                {
                    return svnInfo.LastChangeRevision;
                }

                return 0;
            }
        }

        /// <summary>
        /// 获取提交日志记录
        /// </summary>
        /// <param name="url"></param>
        /// <param name="startRevision"></param>
        /// <param name="endRevision"></param>
        public static string GetCommitLog(string url, SvnRevision startRevision, SvnRevision endRevision)
        {
            using (SvnClient client = GetSvnClient())
            {
                Collection<SvnLogEventArgs> logs;
                var totalLog = new StringBuilder();
                if (client.GetLog(new Uri(url), new SvnLogArgs(new SvnRevisionRange(startRevision, endRevision)), out logs))
                {
                    
                    //后续操作，可以获取作者，版本号，提交时间，提交的message和提交文件列表等信息
                    foreach (var log in logs.OrderByDescending(x => x.Time))
                    {
                        var stringBuilder = new StringBuilder();
                        foreach (var item in log.ChangedPaths.Distinct())
                        {
                            var operateMes = item.Action + "  " + item.RepositoryPath.ToString().Replace("branch/BiHu.BaoXian.ArtificialSubmit/BiHu.BaoXian.ArtificialSubmit.RB-08/", "");
                            stringBuilder.AppendLine(operateMes);
                        }
                        totalLog.AppendLine(string.Format("{0}  {1}  {2} {3}\r\n{4}",
                            log.Author,
                            log.Revision.ToString(),
                            log.Time,
                            log.LogMessage,
                            stringBuilder
                            ));
                    }
                }
                return totalLog.ToString();
            }
        }



        /// <summary>
        /// 导出指定版本的文件到本地
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="revision"></param>
        /// <param name="exportPath"></param>
        /// <returns></returns>
        public static bool ExportFile(string url, string filePath, long revision, string exportPath)
        {
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }


            exportPath = Path.Combine(exportPath, Path.GetFileName(filePath));

            using (SvnClient client = GetSvnClient())
            {
                return client.Export(new SvnUriTarget(Path.Combine(url, filePath), revision), exportPath);
            }
        }



        /// <summary>
        /// 获取svn客户端
        /// </summary>
        /// <returns></returns>
        private static SvnClient GetSvnClient()
        {
            SvnClient client = new SvnClient();
            client.Authentication.Clear();
            client.Authentication.UserNamePasswordHandlers += Authentication_UserNamePasswordHandlers;
            client.Authentication.SslServerTrustHandlers += Authentication_SslServerTrustHandlers;
            return client;
        }

        private static void Authentication_UserNamePasswordHandlers(object sender, SvnUserNamePasswordEventArgs e)
        {
            //登录SVN的用户名和密码
            e.UserName = "svn账户名";
            e.Password = "svn账户密码";
        }


        //SSL证书有问题的，如果要忽略的话可以在这里忽略
        private static void Authentication_SslServerTrustHandlers(object sender, SvnSslServerTrustEventArgs e)
        {
            e.AcceptedFailures = e.Failures;
            e.Save = true;
        }
    }
}
