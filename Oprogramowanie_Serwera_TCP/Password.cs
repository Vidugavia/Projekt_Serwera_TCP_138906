namespace Oprogramowanie_Serwera_TCP
{
    public class Password
    {
        public string password { get; set; }
        public int liczbaznakow { get; set; }

        public Password(string password)
        {
            this.password = password;
            this.liczbaznakow = password.Length;
        }
    }
}