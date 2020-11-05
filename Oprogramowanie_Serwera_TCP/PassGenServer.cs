using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Oprogramowanie_Serwera_TCP
{
    public class PassGenServer : TcpSoftware
    {
        #region Fields
        //private  command; // used to receive proper input to the program
        private string input;
        private string prompt = "$";
        private byte[] buffer = null;
        private object obj = new object();

        public delegate void TransmissionDataDelegate(TcpClient tcpClient);

        #endregion

        public PassGenServer(IPAddress IP, int port) : base(IP, port)
        {

        }

        public override void Start()
        {
            StartListening();

            //transmission starts within the accept function

            AcceptClient();   
        }

        /// <summary>
        /// Funkcja obsługująca połączenie z klientem
        /// </summary>
        /// <param name="Stream"></param>
        private void BeginDataTransmission(TcpClient tcpClient)
        {
            NetworkStream Stream = tcpClient.GetStream();
            Stream.ReadTimeout = 60000;
            PasswordGenerator generator = new PasswordGenerator();
            while (tcpClient.Connected)
            {
                /// <summary>
                /// Wyswietlenie komunikatu startowego i przejscie do oprogramowania
                /// </summary>
                try
                {
                    Send(Stream, buffer, "Password generator server by Dawid Bronszkiewicz.\r\nTo start please enter the command \"genpass [how many passwords] [how many characters]\".\r\nEnter \"exit\" to close the connection.\r\n\r\n");
                    UI(Stream, buffer);
                }
                /// <summary>
                /// Wyłapanie wyjątku powoduje wyświetlenie komunikatu o błędzie i zamknięcie połączenia
                /// </summary>
                catch (IOException)
                {
                    //exception.Message;
                    Send(Stream, buffer, "Closing connection.\r\n\r\n");
                    tcpClient.Close();
                }
            }
        }

        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();

                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);

                //callback style

                transmissionDelegate.BeginInvoke(tcpClient, TransmissionCallback, tcpClient);

                // async result style

                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);

                ////operacje......

                //while (!result.IsCompleted) ;

                ////sprzątanie
            }
        }

        private void TransmissionCallback(IAsyncResult ar)

        {
            TcpClient tcpClient = ar.AsyncState as TcpClient;

            // sprzątanie
            tcpClient.Close();
        }

        /// <summary>
        /// This function sends a message to client
        /// </summary>
        protected override void Send(NetworkStream stream, byte[] buffer, string message)
        {
            buffer = Encoding.ASCII.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// This function receives a message from client
        /// </summary>
        protected override void Receive(NetworkStream stream, byte[] buffer)
        {
            string asciiString = ""; // used to convert from byte[] to string
            char[] charToTrim = { '\0', '\r', '\n' }; // chars we don't want to be in buffer
            buffer = new byte[1024];
            do
            {
                stream.Read(buffer, 0, 1024);
                asciiString = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                input = asciiString.TrimEnd(charToTrim);
            } while (input == "");
            return;
        }

        /// <summary>
        /// This is main user interface
        /// </summary>
        public void UI(NetworkStream stream, byte[] buffer)
        {
            Send(stream, buffer, prompt);
            Receive(stream, buffer);

            switch (input)
            {
                case "genpass":
                    lock (obj)
                    {
                        GenPass(stream, buffer);
                    }
                    break;
                case "exit":
                    throw new IOException("Close connection");
                case "":
                    break;
                default:
                    Send(stream, buffer, "\n\nNo such command.\r\nTo start please enter the command \"genpass [how many passwords] [how many characters]\"\r\nEnter \"exit\" to close the connection\r\n\r\n");
                    break;
            }
        }

        /// <summary>
        /// This is function to execute "genpass" command
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        public void GenPass(NetworkStream stream, byte[] buffer)
        {
            Send(stream, buffer, "How many passwords? ");
            Receive(stream, buffer);
        }
        /*
        void create_command()
        {
            command.clear();

            command.emplace_back();

            bool texty = 0;

            while (!input.empty())
            {
                if (texty)
                {
                    command.back().push_back(input[0]);
                    input.erase(input.begin());
                    if (input.front() == '\"')
                    {
                        input.erase(input.begin());
                        if (texty)
                            texty = 0;
                        else
                            texty = 1;
                        continue;
                    }
                }
                else
                {
                    if (input.front() == ' ')
                    {
                        while (!input.empty() && input.front() == ' ')
                            input.erase(input.begin());
                        command.emplace_back();
                    }
                    if (input.empty())
                    {
                        command.pop_back();
                        break;
                    }
                    if (input.front() == '\"')
                    {
                        input.erase(input.begin());
                        if (texty)
                            texty = 0;
                        else
                            texty = 1;
                        continue;
                    }
                    command.back().push_back(input[0]);
                    input.erase(input.begin());
                }
            }
        }*/
    }
}
