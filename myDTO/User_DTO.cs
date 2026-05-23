namespace MyStore.myDTO
{
    public class User_DTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Filename { get; set; } // Avatar/Profile Image
        public DateTime? CreatedAt { get; set; }
    }
}
