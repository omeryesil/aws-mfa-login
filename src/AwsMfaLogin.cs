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
                System.Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", null);
                System.Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", null);
                System.Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", null);
                System.Environment.SetEnvironmentVariable("AWS_PROFILE", null);

                /*
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
                */
            }
            catch (Exception)
            {
                Console.WriteLine("Failed while deleting environment variables, but that is OK");
            }

            Console.WriteLine($"Log : aws sts get-session-token --profile {profile} --serial-number {serialnumber} --token-code {tokencode}");
            //string result = ExecuteCommand($"aws sts get-session-token --profile {profile} --serial-number {serialnumber} --token-code {tokencode}");

            string result = ProcessExecuter.ExecuteCommand($"aws sts get-session-token --profile {profile} --serial-number {serialnumber} --token-code {tokencode}");

            AwsStsGetTokenReturn credentials = JsonConvert.DeserializeObject<AwsStsGetTokenReturn>(result);

            Console.WriteLine("Log : Setting environment variables: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN, AWS_PROFILE");
            
            System.Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", credentials.Credentials.AccessKeyId, EnvironmentVariableTarget.User);
            System.Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", credentials.Credentials.SecretAccessKey, EnvironmentVariableTarget.User);
            System.Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", credentials.Credentials.SessionToken, EnvironmentVariableTarget.User);
            System.Environment.SetEnvironmentVariable("AWS_PROFILE", profile, EnvironmentVariableTarget.User);

            //set environment variabes
            /*
            if (OsDetector.IsWindows())
            {                
                //set environment variables for AWS CLI
                ExecuteCommand($"setx AWS_ACCESS_KEY_ID {credentials.Credentials.AccessKeyId}");
                ExecuteCommand($"setx AWS_SECRET_ACCESS_KEY {credentials.Credentials.SecretAccessKey}");
                ExecuteCommand($"setx AWS_SESSION_TOKEN {credentials.Credentials.SessionToken}");
                
                //set environment variable for terraform
                //if (profile != "default")
                ExecuteCommand($"setx AWS_PROFILE {profile}");
                
            } */
            if (!OsDetector.IsWindows())
            {
                System.Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", credentials.Credentials.AccessKeyId);
                System.Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", credentials.Credentials.SecretAccessKey);
                System.Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", credentials.Credentials.SessionToken);
                System.Environment.SetEnvironmentVariable("AWS_PROFILE", profile);

                ////set environment variables for AWS CLI
                //ProcessExecuter.ExecuteCommand($"export AWS_ACCESS_KEY_ID={credentials.Credentials.AccessKeyId}");
                //ProcessExecuter.ExecuteCommand($"export AWS_SECRET_ACCESS_KEY={credentials.Credentials.SecretAccessKey}");
                //ProcessExecuter.ExecuteCommand($"export AWS_SESSION_TOKEN={credentials.Credentials.SessionToken}");

                ////set environment variable for terraform
                //ProcessExecuter.ExecuteCommand($"export AWS_PROFILE=\"{profile}\"");

                Console.WriteLine("Dotnetcore has an issue to update environment variables. If they are not set, run the following commands manually:") ;
                Console.WriteLine($"export AWS_ACCESS_KEY_ID=\"{credentials.Credentials.AccessKeyId}\"" + 
                   $" && export AWS_SECRET_ACCESS_KEY=\"{credentials.Credentials.SecretAccessKey}\"" + 
                   $" && export AWS_SESSION_TOKEN=\"{credentials.Credentials.SessionToken}\"" + 
                   $" && export AWS_PROFILE=\"{profile}\"\n");
                Console.WriteLine($"_________________________________________________________________");



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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
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
