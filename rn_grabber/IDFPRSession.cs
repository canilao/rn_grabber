using System;
using System.Net;

namespace RNGrabber
{
   class IDFPRSession
   {
      private string theKey;

      private string GetSessionKey()
      {
         // Host: www.idfpr.com
         // User-Agent: Mozilla/5.0 (X11; U; Linux x86_64; en-US; rv:1.9.2.13) Gecko/20101206 Ubuntu/10.10 (maverick) Firefox/3.6.13
         // Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
         // Accept-Language: en-us,en;q=0.5
         // Accept-Encoding: gzip,deflate
         // Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7
         // Keep-Alive: 115
         // Connection: keep-alive

         HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.idfpr.com");

         myReq.UserAgent = "Mozilla/5.0 (X11; U; Linux x86_64; en-US; rv:1.9.2.13) Gecko/20101206 Ubuntu/10.10 (maverick) Firefox/3.6.13";
         myReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
         myReq.Headers.Add("Accept-Language", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
         myReq.Headers.Add("Accept-Encoding", "gzip,deflate");
         myReq.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
         myReq.Headers.Add("Keep-Alive", "115");
         myReq.KeepAlive = true;

         CookieContainer cookieJar = new CookieContainer();
         myReq.CookieContainer = cookieJar;

         myReq.GetResponse();

         // This is the code cookie key that idfpr.com sends the session code with.
         var sessionCodeKey = "ASPSESSIONIDCCQSCRCA";
         string sessionCode = null;

         // Find our cookie.
         foreach (Cookie c in cookieJar.GetCookies(myReq.RequestUri))
         {
            if (c.Name == sessionCodeKey)
            {
               sessionCode = c.Value;
            }
         }

         if (sessionCode == null) throw new Exception("Did not locate sessionCode");

         return sessionCode;
      }

      public IDFPRSession() { theKey = GetSessionKey(); }

      public string sessionKey { get { return theKey; } }
   }

}
