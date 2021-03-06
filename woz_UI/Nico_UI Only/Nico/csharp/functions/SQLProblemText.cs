﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Nico.csharp.functions
{
    public class SQLProblemText
    {
        public int ProblemID { get; set; }

        // Returns the problem text and headers for a table
        public static List<string> ReadProbText(int problemid)
        {
            List<string> problemText = new List<string>();

            string queryString = "Select * From NicoDB.dbo.Problem_Text Where NicoDB.dbo.Problem_Text.ProblemID = @ProblemID";
            string constr = ConfigurationManager.ConnectionStrings["NicoDB"].ConnectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand cmd = new SqlCommand(queryString, con);
                    con.Open();
                    cmd.Parameters.AddWithValue("@ProblemID", problemid);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        problemText = ReadSingleRow((IDataRecord)reader);
                    }

                    // Call Close when done reading.
                    reader.Close();
                }
            }
            catch (Exception error)
            {
                SQLLog.InsertLog(DateTime.Now, error.Message, error.ToString(), "SQLProblemText ReadProbText", 0, "nlubold");
            }

            return problemText;
        }

        private static List<string> ReadSingleRow(IDataRecord record)
        {
            List<string> problemText = new List<string>();
            for (int i = 1; i < record.FieldCount; i++)
            {
                problemText.Add(record[i].ToString());
            }
            return problemText;
        }
    }
}