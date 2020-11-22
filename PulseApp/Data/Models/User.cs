namespace PulseApp.Data
{
    public class User : BaseModel<int>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool CanLogin { get; set; }
    }
}