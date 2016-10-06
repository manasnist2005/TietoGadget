using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TietoGadget.Models;

namespace TietoGadget.Helpers
{
    public static class DBHelper
    {
        
        public static bool CheckUser(string username, string password, out User usr)
        {

            usr = new User();
            SqlConnection con = new SqlConnection();
            con.ConnectionString = ConfigurationManager.ConnectionStrings["Tridion_CustomDBString"].ToString();
            bool exists = false;
            try
            {
                con.Open();
                // create a command to check if the username exists
                using (SqlCommand cmdget = new SqlCommand("select count(*) from [UserDetails] where UserName = @UserName and Password = @Password", con))
                {
                    cmdget.Parameters.AddWithValue("UserName", username);
                    cmdget.Parameters.AddWithValue("Password", password);
                    exists = (int)cmdget.ExecuteScalar() > 0;
                    
                }
                if (exists)
                {
                    string selectstatement = "select Username,Name,Gender,City,MaritalStatus,BirthDate from [UserDetails]  where UserName = @UserName";              
                    //string selectstatement = "SELECT * FROM [UserDetails]";
                    SqlCommand cmdselect = new SqlCommand(selectstatement, con);
                    cmdselect.Parameters.AddWithValue("UserName", username);
                    //cmdselect.Parameters.AddWithValue("Password", password);
                    cmdselect.CommandType = CommandType.Text;
                    
                    SqlDataAdapter da = null;
                    DataTable table = new DataTable();
                    using (da = new SqlDataAdapter(cmdselect))
                    {
                        da.Fill(table);
                    }

                
                    if (table.Rows.Count>0)
                    {
                        usr.Username = table.Rows[0][0].ToString();
                        usr.Name = table.Rows[0][1].ToString();
                        usr.Gender = table.Rows[0][2].ToString();
                        usr.City = table.Rows[0][3].ToString();
                        usr.Maritalstatus = table.Rows[0][4].ToString();
                        usr.Birthday = Convert.ToDateTime(table.Rows[0][5].ToString());
                    } 
                   
                                    
                }
                con.Close();
                return exists;  
            }
            catch (Exception e)
            {
                con.Close();
                return false;
            }
           
        }
        public static bool UsernameExists(string ussername)
        {
            return false;
        }

        public static string SaveUserDetails(User usr)
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = ConfigurationManager.ConnectionStrings["Tridion_CustomDBString"].ToString();
            bool exists = false;
            try
            {
                con.Open();
                // create a command to check if the username exists
                using (SqlCommand cmdget = new SqlCommand("select count(*) from [UserDetails] where UserName = @UserName", con))
                {
                    cmdget.Parameters.AddWithValue("UserName", usr.Username);
                    exists = (int)cmdget.ExecuteScalar() > 0;
                }
                if (!exists)
                {
                    string insertstatement = "insert into [UserDetails] (Username,Password,Name,Email,Gender,City,MaritalStatus,BirthDate) values " +
                    "('" + usr.Username + "','" + usr.Password + "','" + usr.Name + "','" + usr.Email + "','" + usr.Gender + "','" + usr.City + "','" + usr.Maritalstatus + "','" + usr.Birthday + "')";

                    SqlCommand cmdinsert = new SqlCommand(insertstatement, con);
                    cmdinsert.CommandType = CommandType.Text;
                    cmdinsert.ExecuteNonQuery();
                    con.Close();
                    return "User created";
                }
                else
                {
                    return "Username already taken";
                }
               
            }
            catch (Exception e)
            {
                con.Close();
                return "Error while registration";
            }
           
        }

    }
}