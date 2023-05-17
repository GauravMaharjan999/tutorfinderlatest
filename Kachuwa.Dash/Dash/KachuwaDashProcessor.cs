using System;
using System.Diagnostics;
using System.IO;
using Kachuwa.Dash.Services;

namespace Kachuwa.Dash
{
    public class KachuwaDashProcessor
    {
        internal static string Dash(DASHTool dashTool, string @params, Action<int> onStart = null)
        {
            return Execute(dashTool, @params, onStart);
        }

        internal static string Dash(DASHTool dashTool, string @params, long vlogId= 0)
        {
            return Execute(dashTool, @params, null, vlogId);
        }
        public static string Execute(DASHTool dashTool, string @params, Action<int> onStart = null,long vlogId=0)
        {
            Process p = null; var encodingService = new EncodingService();
            try
            {
                encodingService.LogToFile(@params);
                using (p = new Process())
                {
                    var workdir = Path.GetDirectoryName(Config.Instance.Bento4Path[dashTool]);

                    if (string.IsNullOrWhiteSpace(workdir))
                        throw new ApplicationException("work directory is null");

                    var exePath = Config.Instance.Bento4Path[dashTool];

                    // System.Diagnostics.Process.Start(exePath, @params);
                    bool useBatch = dashTool == DASHTool.Mp4Dash;

                    var info = new ProcessStartInfo(exePath);
                    if (useBatch)
                    {
                        info.Arguments = @params;
                        info.CreateNoWindow = true;
                        info.RedirectStandardError = true;
                        info.RedirectStandardOutput = true;
                        info.UseShellExecute = false; //allow batch script file
                        info.WorkingDirectory = workdir;
                    }
                    else
                    {
                        info.Arguments = @params;
                        info.CreateNoWindow = true;
                        info.RedirectStandardError = true;
                        info.RedirectStandardOutput = true;
                        info.UseShellExecute = false;
                        info.WorkingDirectory = workdir;
                    }

                    p.StartInfo = info;
                    p.Start();
                    if(vlogId>0)
                    encodingService.LogEncodingStart(vlogId, p.Id);
                    if (null != onStart)
                    {
                        onStart.Invoke(p.Id);
                    }

                    string output = string.Empty;
                    string error = string.Empty;
                    if (useBatch)
                    {
                        output = p.StandardOutput.ReadToEnd();
                        error = p.StandardError.ReadToEnd();

                    }
                    else
                    {
                        while (!p.StandardError.EndOfStream)
                        {
                            output = p.StandardError.ReadLine();
                        }

                        error = output;
                    }
                    // *** Read the streams ***


                    if (p.HasExited)
                    {
                        int exitCode = p.ExitCode;
                        if (exitCode != 0)
                        {
                            
                            throw new Exception(error);
                        }
                    }
                    var message = "";
                    message = output;
                    //handing runtime error disk space or other
                    if (message != null && message.Contains("ERROR:"))
                    {
                        throw new Exception(error);
                    }
                    if (vlogId > 0)
                    {
                        encodingService.LogEncodingEnd(vlogId, message, error);
                      
                    }

                    if (dashTool == DASHTool.Mp4HLS||dashTool == DASHTool.Mp4Dash || dashTool == DASHTool.Mp4DashWithHLS)
                    {
                        encodingService.DeleteConvertedFiles(vlogId);
                        encodingService.MarkAsDashReady(vlogId, true);
                    }

                    return message;
                }
            }
            catch (Exception ex)
            {
                if (vlogId > 0)
                {
                    encodingService.LogEncodingError(vlogId, ex.Message);
                    encodingService.CheckPreviousJobsAndRequee(vlogId);
                }

                if (null != p)
                {
                    p.Close();
                    p.Dispose();
                }

                throw ex;
            }
            finally
            {
                if (null != p)
                {
                    p.Close();
                    p.Dispose();
                }
            }

        }
    }
}