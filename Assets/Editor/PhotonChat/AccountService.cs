// ----------------------------------------------------------------------------
// <copyright file="AccountService.cs" company="Exit Games GmbH">
//   Photon Cloud Account Service - Copyright (C) 2012 Exit Games GmbH
// </copyright>
// <summary>
//   Provides methods to register a new user-account for the Photon Cloud and
//   get the resulting appId.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;


public class AccountService
{
    private const string ServiceUrl = "https://service.exitgames.com/AccountExt/AccountServiceExt.aspx";

    //private Action<AccountService> registrationCallback;    // optional (when using async reg)

    public string Message { get; private set; } // msg from server (in case of success, this is the appid)

    protected internal Exception Exception { get; set; } // exceptions in account-server communication

    public string AppId { get; private set; }

    public int ReturnCode { get; private set; } // 0 = OK. anything else is a error with Message

    public enum Origin : byte { ServerWeb = 1, CloudWeb = 2, Pun = 3, Playmaker = 4 };

    /// <summary>
    /// Creates a instance of the Account Service to register Photon Cloud accounts.
    /// </summary>
    public AccountService()
    {
        WebRequest.DefaultWebProxy = null;
        ServicePointManager.ServerCertificateValidationCallback = Validator;
    }

    public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
    {
        return true;    // any certificate is ok in this case
    }

    /// <summary>
    /// Attempts to create a Photon Cloud Account.
    /// Check ReturnCode, Message and AppId to get the result of this attempt.
    /// </summary>
    /// <param name="email">Email of the account.</param>
    /// <param name="origin">Marks which channel created the new account (if it's new).</param>
    public void RegisterByEmail(string email, Origin origin)
    {
//        this.registrationCallback = null;
        this.AppId = string.Empty;
        this.Message = string.Empty;
        this.ReturnCode = -1;

        string result;
        try
        {
            WebRequest req = HttpWebRequest.Create(this.RegistrationUriChat(email, (byte)origin));
            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;

            // now read result
            StreamReader reader = new StreamReader(resp.GetResponseStream());
            result = reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            this.Message = "Failed to connect to Cloud Account Service. Please register via account website.";
            this.Exception = ex;
            return;
        }

        this.ParseResult(result);
    }

    /// <summary>
    /// Creates the service-call Uri, escaping the email for security reasons.
    /// </summary>
    /// <param name="email">Email of the account.</param>
    /// <param name="origin">1 = server-web, 2 = cloud-web, 3 = PUN, 4 = playmaker</param>
    /// <returns>Uri to call.</returns>
    private Uri RegistrationUriChat(string email, byte origin)
    {
        string emailEncoded = Uri.EscapeDataString(email);
        string uriString = string.Format("{0}?email={1}&origin={2}&serviceType=chat", ServiceUrl, emailEncoded, origin);

        return new Uri(uriString);
    }

    /// <summary>
    /// Reads the Json response and applies it to local properties.
    /// </summary>
    /// <param name="result"></param>
    private void ParseResult(string result)
    {
        UnityEngine.Debug.Log("register result: "+ result);

        if (string.IsNullOrEmpty(result))
        {
            this.Message = "Server's response was empty. Please register through account website during this service interruption.";
            return;
        }

        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
        if (values == null)
        {
            this.Message = "Service temporarily unavailable. Please register through account website.";
            return;
        }

        int returnCodeInt = -1;
        string returnCodeString = string.Empty;
        string message;

        values.TryGetValue("ReturnCode", out returnCodeString);
        values.TryGetValue("Message", out message);
        int.TryParse(returnCodeString, out returnCodeInt);

        this.ReturnCode = returnCodeInt;
        if (returnCodeInt == 0)
        {
            // returnCode == 0 means: all ok. message is new AppId
            this.AppId = message;
        }
        else
        {
            // any error gives returnCode != 0
            this.AppId = string.Empty;
            this.Message = message;
        }
    }
}