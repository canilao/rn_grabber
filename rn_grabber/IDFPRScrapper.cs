using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;

namespace RNGrabber
{
   class IDFPRScrapper
   {
      private IDFPRSession theSession = new IDFPRSession();

      private void FindAllSearchLinks(out List<String> links, HtmlDocument htmlDoc)
      {
         links = new List<String>();

         // This parses the idfpr website for license information.
         foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
         {
            foreach (var attrib in link.Attributes)
            {
               if (!links.Contains(attrib.Value) && attrib.Value.Contains("results.asp?page="))
               {
                  links.Add(attrib.Value);
               }
            }
         }
      }

      private void ExecuteInitialSearch(out HtmlDocument htmlDoc, uint maxRowsPerPage, string searchString)
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

         HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.idfpr.com/dpr/licenselookup/results.asp");

         string postData = "TYPE=NAME&pro_cde=0041&lnme=" + searchString + "&checkbox=on&finit=&rowcount=" + maxRowsPerPage + "&submit1.x=43&submit1.y=8";
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
         Cookie c = new Cookie("ASPSESSIONIDCABRCRTC", theSession.sessionKey, "/", "idfpr.com");
         myReq.CookieContainer.Add(c);
         myReq.ContentType = "application/x-www-form-urlencoded";
         myReq.ContentLength = byteArray.Length;

         Stream dataStream = myReq.GetRequestStream();
         dataStream.Write(byteArray, 0, byteArray.Length);
         dataStream.Close();

         WebResponse response = myReq.GetResponse();

         dataStream = response.GetResponseStream();
         StreamReader reader = new StreamReader(dataStream);

         htmlDoc = new HtmlDocument();
         htmlDoc.Load(reader);
      }

      public void FollowLink(out HtmlDocument htmlDoc, string linkData)
      {
         HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://www.idfpr.com/dpr/licenselookup/" + linkData);

         myReq.UserAgent = "Mozilla/5.0 (X11; U; Linux x86_64; en-US; rv:1.9.2.13) Gecko/20101206 Ubuntu/10.10 (maverick) Firefox/3.6.13";
         myReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
         myReq.Headers.Add("Accept-Language", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
         myReq.Headers.Add("Accept-Encoding", "gzip,deflate");
         myReq.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
         myReq.Referer = "http://www.idfpr.com/dpr/licenselookup/results.asp";
         myReq.Headers.Add("Keep-Alive", "115");
         myReq.KeepAlive = true;
         myReq.CookieContainer = new CookieContainer();
         Cookie c = new Cookie("ASPSESSIONIDCABRCRTC", theSession.sessionKey, "/", "http://www.idfpr.com");
         myReq.CookieContainer.Add(c);

         var response = myReq.GetResponse();

         var dataStream = response.GetResponseStream();
         StreamReader reader = new StreamReader(dataStream);

         htmlDoc = new HtmlDocument();
         htmlDoc.Load(reader);
      }

      private string SanitizeText(string original)
      {
         // The DBA / AKA has a &nbsp in it, remove it if it exists.
         var fixedText = original.Replace("&nbsp", "");

         // Check to see if it is a button, if the nurse was disciplined, they use a button with a Y on it.
         if (original.Contains("disc.asp")) fixedText = "Y";

         // Check to see if the IDFPR_Ever_Disciplined == </form>
         if (original.Contains("</form>")) fixedText = "Y";

         // Sanitize the string a little more.
         return fixedText.Replace("'", "''");
      }

      private void ExtractAllNurses(MySqlConnection connection, HtmlDocument htmlDoc)
      {
         // This parses the idfpr website for license information.

         var xpathStr = "/html/body//td[@class='tmpl_menuover'] | /html/body//td[@class='tmpl_tabbackgroundcolor']";

         var tds = htmlDoc.DocumentNode.SelectNodes(xpathStr);

         if (tds == null) throw new Exception("No Data Found");

         List<String> outStr = new List<String>();

         foreach (var node in tds)
         {
            // Save the data to our list of strings.
            outStr.Add(SanitizeText(node.InnerText));

            // If we have 8 pieces of data, that means that we have a complete row.
            if (outStr.Count >= 8)
            {
               // Create a RNDataRow.
               var dataRow = RNDataRow.Create(connection, outStr);

               // Clear out the output string.
               outStr.Clear();

               // Insert into the database.
               try
               {
                  if (dataRow.IsUnique())
                  {
                     dataRow.InsertIntoDataBase();
                  }
               }
               catch (Exception e)
               {
                  // Something happened when we tried to insert the data.
                  Logger.Log(e.Message);
               }
            }
         }
      }

      public void ExecuteScrape(List<String> searchList, MySqlConnection connection)
      {
         const uint rowsPerPage = 1000;

         List<String> links;
         HtmlDocument htmlDoc;

         foreach (var searchTxt in searchList)
         {
            // Execute initial search. 
            ExecuteInitialSearch(out htmlDoc, rowsPerPage, searchTxt);

            // Extract all links from the initial search.
            FindAllSearchLinks(out links, htmlDoc);

            try
            {
               // Extract nurses and insert them into database.
               ExtractAllNurses(connection, htmlDoc);
            }
            catch(Exception e)
            {
               // Something happened when we tried to insert the data.
               Logger.Log(e.Message);
            }

            // For each of the links we find, we need to follow and extract.
            foreach (string link in links)
            {
               FollowLink(out htmlDoc, link);

               try
               {
                  // Need to extract nurses from the link we just visited.
                  ExtractAllNurses(connection, htmlDoc);
               }
               catch(Exception e)
               {
                  // Something happened when we tried to insert the data.
                  Logger.Log(e.Message);
               }
            }
         }
      }
   }
}
