using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public class CurrentUser
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }

        public CurrentUser(string username, int userid)
        {
            Username = username;
            UserId = userid;
        }

        
       
    }
}
