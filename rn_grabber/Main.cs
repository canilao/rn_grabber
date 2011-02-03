using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace RNGrabber
{
   class Program
   {
      static void Main(string[] args)
      {
         // Make an SQL connection.
         // var connection = new MySqlConnection("SERVER=localhost;" + "DATABASE=IDFPR_RN_Records;" + "UID=root;" + "PASSWORD=ceejay1;");
         MySqlConnection connection = null;

         // connection.Open();

         IDFPRScrapper scrapper;

         var searchList = new List<String>() 
         { 
            "a", "b", "c", "d", "e", "f", "g",
            "h", "i", "j", "k", "l", "m", "n",
            "o", "p", "q", "r", "s", "t", "u",
            "v", "w", "x", "y", "z" 
         };

         try
         {
            scrapper = new IDFPRScrapper();
            scrapper.ExecuteScrape(searchList, connection);
         }
         catch(Exception e)
         {
            Logger.Log(e.Message);
         }

         // connection.Close();
      }
   }
}
