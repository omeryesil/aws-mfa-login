using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static System.Environment;

namespace AwsUtility.MfaLogin
{
    /// <summary>
    /// 
    /// </summary>
    public class MfaLogin
    {
        string AwsPath = Path.Combine(
            Environment.GetFolderPath(SpecialFolder.UserProfile, SpecialFolderOption.DoNotVerify),
            ".aws");

        string AwsConfigPath
        {
            get
            {
                return Path.Combine(AwsPath, "config");
            }
        }

        string AwsCredentialsPath
        {
            get
            {
                return Path.Combine(AwsPath, "credentials");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MfaLogin()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="region"></param>
        /// <param name="serialnumber"></param>
        /// <param name="tokencode"></param>
        public string Run(string profile, string region, string serialnumber, string tokencode)
        {
            //unset environment variabes
            try
            {
                if (OsDetector.IsWindows())
                {
                    ExecuteCommand($"set AWS_ACCESS_KEY_ID=");
                    ExecuteCommand($"set AWS_SECRET_ACCESS_KEY=");
                    ExecuteCommand($"set AWS_SESSION_TOKEN=");
                    ExecuteCommand($"set AWS_PROFILE=");

                    //if case there is an issue to remove the environment varialbe with the code above
                    ExecuteCommand("REG delete HKCU\\Environment /F /V AWS_ACCESS_KEY_ID");
                    ExecuteCommand("REG delete HKCU\\Environment /F /V AWS_SECRET_ACCESS_KEY");
                    ExecuteCommand("REG delete HKCU\\Environment /F /V AWS_SESSION_TOKEN");
                    ExecuteCommand("REG delete HKCU\\Environment /F /V AWS_PROFILE");
                }
                else
                {
                    ExecuteCommand($"export AWS_ACCESS_KEY_ID=\"\"");
                    ExecuteCommand($"export AWS_SECRET_ACCESS_KEY=\"\"");
                    ExecuteCommand($"export AWS_SESSION_TOKEN=\"\"");
                    ExecuteCommand($"export AWS_PROFILE=\"\"");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed while deleting environment variables, but that is OK");
            }

            string result = ExecuteCommand($"aws sts get-session-token --profile {profile} --serial-number {serialnumber} --token-code {tokencode}");

            AwsStsGetTokenReturn credentials = JsonConvert.DeserializeObject<AwsStsGetTokenReturn>(result);

            //unset environment variabes
            if (OsDetector.IsWindows())
            {
                //set environment variables for AWS CLI
                ExecuteCommand($"setx AWS_ACCESS_KEY_ID {credentials.Credentials.AccessKeyId}");
                ExecuteCommand($"setx AWS_SECRET_ACCESS_KEY {credentials.Credentials.SecretAccessKey}");
                ExecuteCommand($"setx AWS_SESSION_TOKEN {credentials.Credentials.SessionToken}");

                //set environment variable for terraform
                //if (profile != "default")
                ExecuteCommand($"setx AWS_PROFILE {profile}");
            }
            else
            {
                //set environment variables for AWS CLI
                ExecuteCommand($"export AWS_ACCESS_KEY_ID=\"{credentials.Credentials.AccessKeyId}\"");
                ExecuteCommand($"export AWS_SECRET_ACCESS_KEY=\"{credentials.Credentials.SecretAccessKey}\"");
                ExecuteCommand($"export AWS_SESSION_TOKEN=\"{credentials.Credentials.SessionToken}\"");

                //set environment variable for terraform
                if (profile != "default")
                    ExecuteCommand($"export AWS_PROFILE=\"{profile}\"");
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public string GetMfaSerial(string profile)
        {
            return GetConfigValue(profile, "mfa_serial");
        }

        public string GetRegion(string profile)
        {
            return GetConfigValue(profile, "region");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfigValue(string profile, string key)
        {
            var keyValue = "";

            using (var file = new System.IO.StreamReader(AwsConfigPath))
            {
                string currentProfile = "";
                string line = "";

                while ((line = file.ReadLine()) != null)
                {
                    if (line.Trim() == "[default]")
                        currentProfile = "default";
                    else if (line.Contains("[profile")) //[profile profileName]
                    {
                        currentProfile = line.Replace(" ", "").Replace("[profile", "").Replace("]", "");
                    }

                    if (profile != currentProfile)
                        continue;

                    if (line.Contains(key))
                    {
                        keyValue = line.Replace(" ", "").Replace($"{key}=", "");
                        break;
                    }
                }
            }

            return keyValue;

        }

        private string ExecuteCommand(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");

            ProcessStartInfo procStartInfo = OsDetector.IsWindows() ?
                                        new ProcessStartInfo("cmd", "/c " + escapedArgs) :
                                        new ProcessStartInfo("/bin/bash", $"-c \"{escapedArgs}\"");

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;

            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;

            // wrap IDisposable into using (in order to release hProcess) 
            using (Process process = new Process())
            {
                process.StartInfo = procStartInfo;
                process.Start();

                // Add this: wait until process does its work
                process.WaitForExit();

                // and only then read the result
                string result = process.StandardOutput.ReadToEnd();
                if (String.IsNullOrEmpty(result))
                {
                    throw new Exception(process.StandardError.ReadToEnd());
                }

                return result;
            }

        }
    }


    public class Credentials
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class AwsStsGetTokenReturn
    {
        public Credentials Credentials { get; set; }
    }
}
