﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityServer.Web.Authentication.External;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading;
using System.Configuration;

namespace OktaMFA_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //claims = null;
            IAdapterPresentation result = null;
            //string pin = proofData.Properties["pin"].ToString();
            string tenantName = "marcjordan";
            string userName = "joel@oktademo1.com";
            //string baseUrl = "https://" + tenantName + ".oktapreview.com/api/v1/";
            //string authToken = "SSWS 009RUU8EeUvD-EpOEH1qHL0OZwmCTJK71kzFjsQufr";
            string userID = "";

            string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            System.Configuration.ExeConfigurationFileMap fileMap = new System.Configuration.ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = windir + "\\ADFS\\OktaMFA-ADFS.dll.config";
            Console.WriteLine(windir);
            Console.ReadLine();
            System.Configuration.Configuration cfg =
            System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
            string oktaTenant = cfg.AppSettings.Settings["Tenant"].Value;
            string authToken = cfg.AppSettings.Settings["apiKey"].Value;
            //string oktaTenant = "https://hanson-family.okta.com";
            //string authToken = "SSWS 00iqSGFD-x_oNoXPiznNodqZ0_wpfxbgUGd0A9Jjos";

            //string upn = identityClaim.Value;
            //string tenantName = "marcjordan";
            string baseUrl = oktaTenant + "/api/v1/";
            string pinSuccess = "no";
            string verifyResult = "false";
            
              

            HttpWebRequest upnRequest = (HttpWebRequest)WebRequest.Create(baseUrl + "users/" + userName);
            upnRequest.Headers.Add("Authorization", authToken);
            upnRequest.Method = "GET";
            upnRequest.ContentType = "application/json";
            var upnResponse = (HttpWebResponse)upnRequest.GetResponse();
            var idReader = new StreamReader(upnResponse.GetResponseStream());
            var id = idReader.ReadToEnd();

            RootObject userProfile = JsonConvert.DeserializeObject<RootObject>(id);

            userID = userProfile.id.ToString();

            HttpWebRequest factorRequest = (HttpWebRequest)WebRequest.Create(baseUrl + "users/" + userID + "/factors");
            factorRequest.Headers.Add("Authorization", authToken);
            factorRequest.Method = "GET";
            factorRequest.ContentType = "application/json";
            factorRequest.Accept = "application/json";
            var factorResponse = (HttpWebResponse)factorRequest.GetResponse();
            var factorReader = new StreamReader(factorResponse.GetResponseStream());
            var factorList = factorReader.ReadToEnd();

            RootObject[] factors = JsonConvert.DeserializeObject<RootObject[]>(factorList);
            string factorID = "";
            foreach (RootObject factor in factors)
            {

                if (factor.factorType == "sms")
                {
                    Thread.Sleep(1000);
                    factorID = factor.id;
                    HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(baseUrl + "users/" + userID + "/factors/" + factorID + "/verify");
                    httprequest.Headers.Add("Authorization", authToken);
                    httprequest.Method = "POST";
                    httprequest.ContentType = "application/json";
                    var httpResponse = (HttpWebResponse)httprequest.GetResponse();
                    Console.WriteLine("Enter Pin");
                    string pin = Console.ReadLine();

                    HttpWebRequest verifyRequest = (HttpWebRequest)WebRequest.Create(baseUrl + "users/" + userID + "/factors/" + factorID + "/verify");
                    verifyRequest.Headers.Add("Authorization", authToken);
                    verifyRequest.Method = "POST";
                    verifyRequest.ContentType = "application/json";


                    otpCode otpCode = new otpCode
                    { passCode = pin };
                    string otpString = JsonConvert.SerializeObject(otpCode);
                    using (var streamWriter = new StreamWriter(verifyRequest.GetRequestStream()))
                    {

                        streamWriter.Write(otpString);
                    }

                    try
                    {
                        var verifyResponse = (HttpWebResponse)verifyRequest.GetResponse();

                        //var factorReader2 = new StreamReader();
                        //var factorList = factorReader.ReadToEnd();

                        if (verifyResponse.StatusCode.ToString() == "OK" && pin != "")
                        {            
                            pinSuccess = "yes";
                            Claim claim = new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                            //claims = new Claim[] { claim };
                            //return result;

                            var verifyReader = new StreamReader(verifyResponse.GetResponseStream());
                            var verifyList = factorReader.ReadToEnd();
                            Console.WriteLine("yes");

                        }

                    }
                    catch (WebException we)
                    {
                        var failResponse = we.Response as HttpWebResponse;
                        if (failResponse == null)
                            throw;
                        //result = new AdapterPresentation("Authentication failed.", proofData.Properties["upn"].ToString(), false);
                    }
                    Console.ReadLine();
                }


            }

            //if (pinSuccess == "yes")
            //{
            //    Claim claim = new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
            //    claims = new Claim[] { claim };
            //    return result;
            //}
            //else
            //{
            //    result = new AdapterPresentation("Authentication failed.", proofData.Properties["upn"].ToString(), false);
            //}
            //return result;
        }

    }

        public class otpCode
        {
            public string passCode { get; set; }


        }

        public class userProfile
        {
            public string id { get; set; }
            public string status { get; set; }
            public string lastLogin { get; set; }
            public string lastUpdated { get; set; }
            public string passwordChanged { get; set; }
            public string created { get; set; }

        }

        public class Key
        {
            public string kty { get; set; }
            public string use { get; set; }
            public string kid { get; set; }
            public List<string> x5c { get; set; }
        }

        public class Profile
        {
            public string credentialId { get; set; }
            public string phoneNumber { get; set; }
            public string deviceType { get; set; }
            public List<Key> keys { get; set; }
            public string name { get; set; }
            public string platform { get; set; }
            public string version { get; set; }
            public string email { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string login { get; set; }
            public object mobilePhone { get; set; }
            public object secondEmail { get; set; }
        }

        public class Hints
        {
            public List<string> allow { get; set; }
        }

        public class Verify
        {
            public string href { get; set; }
            public Hints hints { get; set; }
        }

        public class Hints2
        {
            public List<string> allow { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
            public Hints2 hints { get; set; }
        }

        public class Hints3
        {
            public List<string> allow { get; set; }
        }

        public class User
        {
            public string href { get; set; }
            public Hints3 hints { get; set; }
        }

        public class Links
        {
            public Verify verify { get; set; }
            public Self self { get; set; }
            public User user { get; set; }
            public ResetPassword resetPassword { get; set; }
            public ResetFactors resetFactors { get; set; }
            public ExpirePassword expirePassword { get; set; }
            public ForgotPassword forgotPassword { get; set; }
            public ChangeRecoveryQuestion changeRecoveryQuestion { get; set; }
            public Deactivate deactivate { get; set; }
            public ChangePassword changePassword { get; set; }
            public Poll poll { get; set; }

        }

        public class RootObject
        {
            public string id { get; set; }
            public string factorType { get; set; }
            public string provider { get; set; }
            public string status { get; set; }
            public string created { get; set; }
            public string lastUpdated { get; set; }
            public Profile profile { get; set; }
            public Links _links { get; set; }
            public object activated { get; set; }
            public string statusChanged { get; set; }
            public string lastLogin { get; set; }
            public string passwordChanged { get; set; }
            public Credentials credentials { get; set; }
            public string factorResult { get; set; }
            public string expiresAt { get; set; }
        }



        public class Password
        {
        }

        public class RecoveryQuestion
        {
            public string question { get; set; }
        }

        public class Provider
        {
            public string type { get; set; }
            public string name { get; set; }
        }

        public class Credentials
        {
            public Password password { get; set; }
            public RecoveryQuestion recovery_question { get; set; }
            public Provider provider { get; set; }
        }

        public class ResetPassword
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class ResetFactors
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class ExpirePassword
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class ForgotPassword
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class ChangeRecoveryQuestion
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class Deactivate
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class ChangePassword
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class Cancel
        {
            public string href { get; set; }
            public Hints2 hints { get; set; }
        }

        public class Poll
        {
            public string href { get; set; }
            public Hints hints { get; set; }
        }
    }




