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
        private List<string> command = new List<string>(); // used to receive proper input to the program
        private string input;
        private string prompt = "$";
        private byte[] buffer = null;

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
            create_command();

            switch (command[0])
            {
                case "genpass":
                    PasswordGenerator generator = new PasswordGenerator();
                    for (int i = 1; i <= Int32.Parse(command[1]); i++)
                        Send(stream, buffer, "Password #" + i + ": " + generator.GeneratePassword(Int32.Parse(command[2])) + "\r\n");
                    break;
                case "exit":
                    throw new IOException("Close connection");
                case "":
                    break;
                default:
                    Send(stream, buffer, "\r\nNo such command.\r\nTo start please enter the command \"genpass [how many passwords] [how many characters]\"\r\nEnter \"exit\" to close the connection\r\n\r\n");
                    break;
            }
        }

        private void create_command()
        {
            if(command != null && command.Count() != 0 )
                command.Clear();

            bool texty = false;
            command.Add("");

            while (input.Length != 0)
            {
                if (texty)
                {
                    command[command.Count - 1] += input[0];
                    input = input.Remove(0,1);
                    if (input[0] == '\"')
                    {
                        input.Remove(0, 1);
                        if (texty)
                            texty = false;
                        else
                            texty = true;
                        continue;
                    }
                }
                else
                {
                    if (input[0] == ' ')
                    {
                        while (input.Length != 0 && input[0] == ' ')
                            input = input.Remove(0, 1);
                        command.Add("");
                    }
                    if (input[0] == '\"')
                    {
                        input = input.Remove(0, 1);
                        if (texty)
                            texty = false;
                        else
                            texty = true;
                        continue;
                    }
                    command[command.Count-1] += input[0];
                    input = input.Remove(0, 1);
                }
            }
        }
    }
}
