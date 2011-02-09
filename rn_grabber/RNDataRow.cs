using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace RNGrabber
{
   class RNDataRow
   {
      private MySqlConnection theConnection;

      private RNDataRow(MySqlConnection connection) { theConnection = connection; }

      private void ParseDataList(List<String> list) 
      {
         int i = 0;

         foreach (var dataElement in list)
         {
            switch (i++)
            {
               case 0:
                  IDFPR_License_Name = dataElement;
                  break;
               case 1:
                  IDFPR_DBA_AKA = dataElement;
                  break;
               case 2:
                  IDFPR_RN_License_Number = dataElement;
                  break;
               case 3:
                  IDFPR_License_Status = dataElement;
                  break;
               case 4:
                  char[] delimiters = { ',' };
                  var elements = dataElement.Split(delimiters);

                  if (elements.Length >= 2)
                  {
                     IDFPR_City = elements[0].Trim();
                     IDFPR_State = elements[1].Trim();
                  }

                  break;
               case 5:
                  try
                  {
                     IDFPR_Original_Issue_Date = DateTime.Parse(dataElement);
                  }
                  catch
                  {
                     IDFPR_Original_Issue_Date = new DateTime(0);
                  }
                  break;
               case 6:
                  try
                  {
                     IDFPR_Expiration_Date = DateTime.Parse(dataElement);
                  }
                  catch
                  {
                     IDFPR_Expiration_Date = new DateTime(0);
                  }
                  break;
               case 7:
                  IDFPR_Ever_Disciplined = dataElement == "Y" ? true : false;
                  break;
            }
         }
      }

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
         // Create the command object.
         MySqlCommand command = theConnection.CreateCommand();

         // If the RN is DECEASED check for the data in a different table.
         if(IDFPR_License_Status == "DECEASED")
         {           
            command.CommandText = "INSERT INTO RN_Empty_License(IDFPR_License_Name,IDFPR_DBA_AKA,IDFPR_RN_License_Number,IDFPR_License_Status,IDFPR_City,IDFPR_State,IDFPR_Original_Issue_Date,IDFPR_Expiration_Date,IDFPR_Ever_Disciplined) VALUES(";
         }
         else
         {
            command.CommandText = "INSERT INTO Registered_Nurses(IDFPR_License_Name,IDFPR_DBA_AKA,IDFPR_RN_License_Number,IDFPR_License_Status,IDFPR_City,IDFPR_State,IDFPR_Original_Issue_Date,IDFPR_Expiration_Date,IDFPR_Ever_Disciplined) VALUES(";
         }

         var outStr = new List<string>();
            
         outStr.Add(IDFPR_License_Name);
         outStr.Add(IDFPR_DBA_AKA);
         outStr.Add(IDFPR_RN_License_Number);
         outStr.Add(IDFPR_License_Status);
         outStr.Add(IDFPR_City);
         outStr.Add(IDFPR_State);
         outStr.Add(IDFPR_Original_Issue_Date.ToString("yyyy-MM-dd"));
         outStr.Add(IDFPR_Expiration_Date.ToString("yyyy-MM-dd"));
         outStr.Add(IDFPR_Ever_Disciplined ? "1" : "0");
 
         // Build the rest of the SQL statement.
         foreach(var dataStr in outStr)
         {
            // Sanitize apostrophes.
            var clean = dataStr.Replace("'", "''");
         
            command.CommandText += "'" + clean + "'" + ",";
         }
            
         command.CommandText = command.CommandText.Remove(command.CommandText.LastIndexOf(','), 1);
         command.CommandText += ")";

         try
         {
            command.ExecuteNonQuery();
         }
         catch(Exception e)
         {
            Logger.Log(e.Message);
            Logger.Log(command.CommandText);
         }
      }

      public bool IsUnique() 
      {
         // This needs to check to see if the data exists, if not then its ok to add, otherwise throw an exception.
         MySqlCommand command = theConnection.CreateCommand();

         // If the RN is DECEASED check for the data in a different table.
         if(IDFPR_License_Status == "DECEASED")
         {           
            command.CommandText = "SELECT COUNT( * ) AS NumberFound FROM RN_Empty_License WHERE IDFPR_License_Name='" + IDFPR_License_Name + "'";  
         }
         else
         {
            command.CommandText = "SELECT COUNT( * ) AS NumberFound FROM Registered_Nurses WHERE IDFPR_RN_License_Number='" + IDFPR_RN_License_Number + "'";
         }

         MySqlDataReader reader = null;
         
         try
         {
             reader = command.ExecuteReader();
         }
         catch(Exception e)
         {
            Logger.Log(e.Message);
            Logger.Log(command.CommandText);
         }
            
         int theCount = 0;
         
         while(reader.Read()) 
         {
            theCount = int.Parse(reader["NumberFound"].ToString());
         }  
            
         // Make sure the reader is closed.
         reader.Close();
            
         return theCount == 0; 
      }
   }
}
