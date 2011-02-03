using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace RNGrabber
{
   class RNDataRow
   {
      private MySqlConnection theConnection;

      private RNDataRow(MySqlConnection connection) { theConnection = connection; }

      private void ParseDataList(List<String> list) { }

      private string IDFPR_License_Name { get; set; }

      private string IDFPR_DBA_AKA { get; set; }

      private string IDFPR_RN_License_Number { get; set; }

      private string IDFPR_License_Status { get; set; }

      private string IDFPR_City { get; set; }

      private string IDFPR_State { get; set; }

      private DateTime IDFPR_Original_Issue_Date { get; set; }

      private DateTime IDFPR_Expiration_Date { get; set; }

      private bool IDFPR_Ever_Disciplined { get; set; }

      static public RNDataRow Create(MySqlConnection connection, List<String> dataList)
      {
         var obj = new RNDataRow(connection);
         obj.ParseDataList(dataList);

         return obj;
      }

      public void InsertIntoDataBase()
      {
         // This needs to check to see if the data exists, if not then its ok to add, otherwise throw an exception.
         /*
         MySqlCommand command = connection.CreateCommand();

         command.CommandText = "INSERT INTO Registered_Nurses(IDFPR_License_Name,IDFPR_DBA_AKA,IDFPR_RN_License_Number,IDFPR_License_Status,IDFPR_City_State,IDFPR_Original_Issue_Date,IDFPR_Expiration_Date,IDFPR_Ever_Disciplined) VALUES(";

         command.CommandText = command.CommandText.Remove(command.CommandText.LastIndexOf(','), 1);
         command.CommandText += ")";

         // Sometimes they put an space for empty license numbers, usually for Deceased.
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
            LogEvent(e.Message);
            LogEvent(command.CommandText);
         }
          */
      }

      public bool IsUnique() { return true; }
   }
}
