using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using HtmlAgilityPack;

namespace LearningMono
{
	class MainClass
	{	
		public static void ParseIt()
		{
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
			
			var sessionCodeKey = "ASPSESSIONIDCCQSCRCA";
			string sessionCode = null;
			
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
		
		public static void Main(string[] args)
		{
			System.Console.WriteLine(GetSessionKey());
		}
	}
}