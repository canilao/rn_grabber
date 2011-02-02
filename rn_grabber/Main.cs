using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Web;
using System.Text;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;

namespace RNGrabber
{
	class MainClass
	{	
		public static MySqlConnection connection = new MySqlConnection("SERVER=localhost;" + "DATABASE=IDFPR_RN_Records;" + "UID=root;" + "PASSWORD=ceejay1;");
		
		public static string theKey = GetSessionKey();
		
		public static void ParseIt(string htmlData)
		{
			// This parses the idfpr website for license information.
			HtmlDocument doc = new HtmlDocument();
 
			var textReader = new StringReader(htmlData);
			doc.Load(textReader);

			var xpathStr = "/html/body//td[@class='tmpl_menuover'] | /html/body//td[@class='tmpl_tabbackgroundcolor']";
			
			var tds = doc.DocumentNode.SelectNodes(xpathStr);
			
			if(tds == null) throw new Exception("No Data Found");
			
			List<string> outStr = new List<string>();
			
			foreach(var node in tds)
 			{	
				// The DBA / AKA has a &nbsp in it, remove it if it exists.
				var fixedText = node.InnerText.Replace("&nbsp", "");
				
				// Check to see if it is a button, if the nurse was disciplined, they use a button with a Y on it.
				if(node.InnerText.Contains("disc.asp")) fixedText = "Y";
				
				// Check to see if the IDFPR_Ever_Disciplined == </form>
				if(node.InnerText.Contains("</form>")) fixedText = "Y";
				
				outStr.Add(fixedText);
				
				if(outStr.Count >= 8)
				{				
					MySqlCommand command = connection.CreateCommand();
			
					command.CommandText = "INSERT INTO Registered_Nurses(IDFPR_License_Name,IDFPR_DBA_AKA,IDFPR_RN_License_Number,IDFPR_License_Status,IDFPR_City_State,IDFPR_Original_Issue_Date,IDFPR_Expiration_Date,IDFPR_Ever_Disciplined) VALUES(";
					
					foreach(var dataStr in outStr)
					{
						// Sanitize apostrophes.
						var clean = dataStr.Replace("'", "''");
						
						command.CommandText += "'" + clean + "'" + ",";
					}
					
					command.CommandText = command.CommandText.Remove(command.CommandText.LastIndexOf(','), 1);
					command.CommandText += ")";
					
					// Sometimes they put an space for empty license numbers.
					if(outStr[2] == " ")
					{
						command.CommandText = command.CommandText.Replace("Registered_Nurses", "RN_Empty_License");
					}
					
					try
					{
						command.ExecuteNonQuery();
					}
					catch(Exception e)
					{
						Console.WriteLine(e.Message);
						Console.WriteLine(command.CommandText);
					}
					
					outStr.Clear();
				}
 			}
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
		
		public static string SearchForNurses(int maxRows, string sessionKey, string searchString)
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
			
			string postData = "TYPE=NAME&pro_cde=0041&lnme=" + searchString + "&checkbox=on&finit=&rowcount=" + maxRows + "&submit1.x=43&submit1.y=8";
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
			
			dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            
			return reader.ReadToEnd();
		}
		
		public static List<String> FindAllPageLinks(string htmlData)
		{
			// This parses the idfpr website for license information.
			HtmlDocument doc = new HtmlDocument();
 
			var textReader = new StringReader(htmlData);
			doc.Load(textReader);
			
			var retList = new List<String>();
			
			foreach(HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
			{
				foreach(var attrib in link.Attributes)
				{	
					if(!retList.Contains(attrib.Value) && attrib.Value.Contains("results.asp?page="))
					{
						retList.Add(attrib.Value);
					}
				}
			}
			
			return retList;
		}
		
		public static string FollowALink(string uri, string sessionKey)
		{
			HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create("http://www.idfpr.com/dpr/licenselookup/" + uri);
			
			myReq.UserAgent = "Mozilla/5.0 (X11; U; Linux x86_64; en-US; rv:1.9.2.13) Gecko/20101206 Ubuntu/10.10 (maverick) Firefox/3.6.13";
			myReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			myReq.Headers.Add("Accept-Language", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			myReq.Headers.Add("Accept-Encoding", "gzip,deflate");
			myReq.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
			myReq.Referer = "http://www.idfpr.com/dpr/licenselookup/results.asp";
			myReq.Headers.Add("Keep-Alive", "115");
			myReq.KeepAlive = true;
			myReq.CookieContainer = new CookieContainer();
			Cookie c = new Cookie("ASPSESSIONIDCABRCRTC", sessionKey, "/", "http://www.idfpr.com");
			myReq.CookieContainer.Add(c);
			
			var response = myReq.GetResponse();
							
			var dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            
			return reader.ReadToEnd();
		}

			/*
			MySqlDataReader Reader;
			
			Reader = command.ExecuteReader();
			
			while (Reader.Read())
			{
				string thisrow = "";
				for (int i= 0;i<Reader.FieldCount;i++)
						thisrow+=Reader.GetValue(i).ToString() + ",";
				listBox1.Items.Add(thisrow);
			}
			*/
		
		public static void GrabIt(string search)
		{				
			Console.WriteLine("Attempting to get data from server...");
			
			var htmlData = SearchForNurses(1000, theKey, search);
			
			Console.WriteLine("Attempting parse all of the links...");
			
			var linkList = FindAllPageLinks(htmlData);
			
			Console.WriteLine("Data from server retrieved, attempting to parse " + htmlData.Length.ToString() + " characters...");
			
			foreach(var parsedLink in linkList) Console.WriteLine(parsedLink);
			
			Console.WriteLine("Parsing html data...");
			
			ParseIt(htmlData);
			
			foreach(string link in linkList)
			{
				Console.WriteLine("Following link " + link + "...");
				
				var htmlDataFromLink = FollowALink(link, theKey);
				
				Console.WriteLine("Data from server retrieved, attempting to parse " + htmlDataFromLink.Length.ToString() + " characters...");
				
				ParseIt(htmlDataFromLink);
			}
		}
		
		public static void Main(string[] args)
		{
			connection.Open();
			
			GrabIt("a");
			GrabIt("b");
			GrabIt("c");
			GrabIt("d");
			GrabIt("e");
			GrabIt("f");
			GrabIt("g");
			GrabIt("h");
			GrabIt("i");
			GrabIt("j");
			GrabIt("k");
			GrabIt("l");
			GrabIt("m");
			GrabIt("n");
			GrabIt("o");
			GrabIt("p");
			GrabIt("q");
			GrabIt("r");
			GrabIt("s");
			GrabIt("t");
			GrabIt("u");
			GrabIt("v");
			GrabIt("w");
			GrabIt("x");
			GrabIt("y");
			GrabIt("z");
			
			connection.Close();
		}
	}
}