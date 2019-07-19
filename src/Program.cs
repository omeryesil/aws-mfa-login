using System;

namespace AwsUtility.MfaLogin
{
    class Program
    {

        static void Main(string region, string profile = "default", string serialnumber = "", string tokencode = "")
        {
            MfaLogin mfaLogin = new MfaLogin();

            Console.WriteLine("Aws MFA Login");

            WriteHelp();
            Console.WriteLine("-----------");
            try
            {
                // if (OsDetector.IsLinux())
                // {
                //     throw new NotImplementedException("Linux not supported");
                // }

                if (String.IsNullOrEmpty(serialnumber))
                {
                    serialnumber = mfaLogin.GetMfaSerial(profile);
                    if (String.IsNullOrEmpty(serialnumber))
                        throw new Exception("serialnumber must be provided through command line (--serialnumber) or in ~/.aws/config file with mfa_serial value");
                }

                if (String.IsNullOrEmpty(region))
                {
                    region = mfaLogin.GetRegion(profile);
                    if (String.IsNullOrEmpty(region))
                        throw new Exception("region must be provided through command line (--region) or in ~/.aws/config file with region value");
                }

                if (String.IsNullOrEmpty(tokencode))
                {
                    Console.Write("tokencode: ");
                    tokencode = Console.ReadLine();
                }

                var result = mfaLogin.Run(profile, region, serialnumber, tokencode);

                Console.WriteLine(result);

                Console.WriteLine("SUCCESS");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        private static void WriteHelp()
        {
            Console.WriteLine("--profile : default 'default'");
            Console.WriteLine("--region : default 'us-east-2'");
            Console.WriteLine("--serialnumber : MFA devide serial number");
            Console.WriteLine("--tokencode : MFA token code");
            Console.WriteLine("Sample: awsutility --serialnumber arn:aws:iam::123123123:mfa/email@emaildomain.com --token-code 123123 ");
        }

    }
}
