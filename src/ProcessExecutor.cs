using System;
using System.Text;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace AwsUtility.MfaLogin {
    
    ///
    ///
    public class ProcessExecuter
    {
        // Define static variables shared by class methods.
        private static StreamWriter _streamError =null;
        private static StringBuilder _processOutput = null;
        private static bool _errorRedirect = false;
        private static bool _errorsWritten = false;

        public static string ExecuteCommand(string command)
        {

            _streamError =null;
            _processOutput = null;

            var escapedArgs = command.Replace("\"", "\\\"");

            ProcessStartInfo procStartInfo = OsDetector.IsWindows() ?
                                        new ProcessStartInfo("cmd", "/c " + escapedArgs) :
                                        new ProcessStartInfo("/bin/bash", $"-c \"{escapedArgs}\"");

            // Check if errors should be redirected to a file.
            _errorsWritten = false;

            Process process;
            process = new Process();
            process.StartInfo = procStartInfo;

            // Set UseShellExecute to false for redirection.
            process.StartInfo.UseShellExecute = false;

            // Redirect the standard output of the net command.  
            // This stream is read asynchronously using an event handler.
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += new DataReceivedEventHandler(NetOutputDataHandler);
            _processOutput = new StringBuilder();
   
            // Do not redirect the error output.
            process.StartInfo.RedirectStandardError = false;

            // Start the process.
            process.Start();
            process.BeginOutputReadLine();

            if (_errorRedirect)
            {
                process.BeginErrorReadLine();
            }

            process.WaitForExit();

            if (_streamError != null)
                _streamError.Close();
            else 
            {
                // Set _errorsWritten to false if the stream is not
                // open.   Either there are no errors, or the error
                // file could not be opened.
                _errorsWritten = false;
            }

            if (_processOutput.Length > 0)
            {
                // If the process wrote more than just
                // white space, write the output to the console.
                Console.WriteLine("\nOutput: \n{0}\n", _processOutput);
            }

            process.Close();
            return _processOutput.ToString();
        }

        private static void NetOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the net view command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // Add the text to the collected output.
                _processOutput.Append(Environment.NewLine + "  " + outLine.Data);
            }
        }

    }
}