using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Web;
using System.Text;
using HtmlAgilityPack;

namespace LearningMono
{
	class MainClass
	{	
		public static void ParseIt()
		{
			// This parses the idfpr website for license information.
			HtmlDocument doc = new HtmlDocument();
 
			doc.Load("/home/chris/Temp/a_results.htm");

			var xpathStr = "/html/body//td[@class='tmpl_menuover'] | /html/body//td[@class='tmpl_tabbackgroundcolor']";
			
			var tds = doc.DocumentNode.SelectNodes(xpathStr);
			
			List<string> outStr = new List<string>();
			
			foreach(var node in tds)
 			{	
				outStr.Add(node.InnerText);
				
				if(outStr.Count >= 8)
				{
					foreach(var dataStr in outStr)
					{
						System.Console.Write(dataStr + " ");
					}
					
					System.Console.WriteLine();
					
					outStr.Clear();
				}
 			}

			doc.Save("/home/chris/Temp/anilao_search_post.html");
		}
		
		public static string GetSessionKey()
		{
			// Host: www.idfpr.com
			// User-Agent: Mozilla/5.0 (X11; U; Linux x86_64; en-US; rv:1.9.2.13) Gecko/20101206 Ubuntu/10.10 (maverick) Firefox/3.6.13
			// Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
			// Accept-Language: en-us,en;q=0.5
			// Accept-Encoding: gzip,deflate
			// Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7
			// Keep-Alive: 115
			// Connection: keep-alive
			
			HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create("http://www.idfpr.com");
			
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
				if(c.Name == sessionCodeKey)
				{
					sessionCode = c.Value;
				}
			}

			if(sessionCode == null) throw new Exception();
			
			return sessionCode;
		}
		
		public static void SearchForNurses(string sessionKey, string seachString)
		{
			// POST /dpr/licenselookup/results.asp HTTP/1.1
			// Host: www.idfpr.com
			// User-Agent: Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.13) Gecko/20101203 Firefox/3.6.13 (.NET CLR 3.5.30729)
			// Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
			// Accept-Language: en-us,en;q=0.5
			// Accept-Encoding: gzip,deflate
			// Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7
			// Keep-Alive: 115
			// Connection: keep-alive
			// Referer: http://www.idfpr.com/dpr/licenselookup/default.asp
			// Cookie: ASPSESSIONIDCABRCRTC=EEJNDGGBAMHANPFBMKKPOBGK
			// Content-Type: application/x-www-form-urlencoded
			// Content-Length: 88
			
			// TYPE=NAME&pro_cde=0041&lnme=Anilao&checkbox=on&finit=&rowcount=&submit1.x=32&submit1.y=4
			
			HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create("http://www.idfpr.com/dpr/licenselookup/results.asp");
			
			string postData = "TYPE=NAME&pro_cde=0041&lnme=Anilao&checkbox=on&finit=&rowcount=5000&submit1.x=43&submit1.y=8";
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			
			myReq.Method = "POST";
			myReq.ContentType = "Mozilla/5.0 (X11; U; Linux x86_64; en-US; rv:1.9.2.13) Gecko/20101206 Ubuntu/10.10 (maverick) Firefox/3.6.13";
			myReq.Headers.Add("Accept-Language", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			myReq.Headers.Add("Accept-Encoding", "gzip,deflate");
			myReq.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
			myReq.Headers.Add("Keep-Alive", "115");
			myReq.KeepAlive = true;
			myReq.Referer = "http://www.idfpr.com/dpr/licenselookup/default.asp";
			myReq.CookieContainer = new CookieContainer();
			Cookie c = new Cookie("ASPSESSIONIDCABRCRTC", sessionKey, "/", "http://www.idfpr.com");
			myReq.CookieContainer.Add(c);
			myReq.ContentType = "application/x-www-form-urlencoded";
			myReq.ContentLength = byteArray.Length;
			
			Stream dataStream = myReq.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();
			
			WebResponse response = myReq.GetResponse();
			
			Console.WriteLine(((HttpWebResponse)response).StatusDescription);
			
			dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
		}
		
		public static void Main(string[] args)
		{		
			SearchForNurses(GetSessionKey(), "anilao");
		}
	}
}