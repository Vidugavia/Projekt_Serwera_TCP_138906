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
        private object obj = new object();
        /// <summary>
        /// This is main program to generate passwords
        /// </summary>
        public string GeneratePassword(int size)
        {
            string passwd = "";
            lock (obj)
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());                
                for (int j = 0; j < size; j++)
                {
                    passwd += (char)rnd.Next(33, 126);
                }
            }
            return passwd;
        }        
    }
}
