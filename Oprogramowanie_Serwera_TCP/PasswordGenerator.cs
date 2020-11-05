using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Oprogramowanie_Serwera_TCP
{
    class PasswordGenerator
    {
        /// <summary>
        /// This is main program to generate passwords
        /// </summary>
        public Password GeneratePassword(int size)
        {
            Random rnd = new Random();
            Password passwd;            
            passwd = new Password("");
            for (int j = 0; j < size; j++)
            {
                passwd.password.Append((char)rnd.Next(33, 126));
            }
            return passwd;
        }        
    }
}
