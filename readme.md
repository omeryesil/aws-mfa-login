# Aws Utility Mfa Login

A utility to login to AWS with MFA.

**NOTE** : Currently only Windows OS supported. 

- After the successful run, the following environment variables get set:
```shell
AWS_ACCESS_KEY_ID=<Access-Key-as-in-Previous-Output>
AWS_SECRET_ACCESS_KEY=<Secret-Access-Key-as-in-Previous-Output>
AWS_SESSION_TOKEN=<Session-Token-as-in-Previous-Output>
AWS_PROFILE=<profile>
```

AWS_PROFILE is used for Terraform

## Usage 

### Parameters 

- '--profile' : Profile name. Default is 'default'
- '--region' : Aws Region. If not provided, then the value will be taken from ~/.aws/config file
- '--serialnumber' : MFA Device serial number. If not provided, then it will be taken from ~/.aws/config (mfa_serial). 
- '--tokencode' : MFA Token code. 

### With all parameters

```shell
dotnet awsmfalogin.dll --profile osram --region us-east-2 --serialnumber arn:aws:iam::172564481277:mfa/o.yesil-ext@osram.com --tokencode 12312
```

### With Profile 

Based on the provided profile name, region and serialnumber values will be taken from ~/.aws/config file

```shell
dotnet awsmfalogin.dll --profile osram --tokencode 12312
```
