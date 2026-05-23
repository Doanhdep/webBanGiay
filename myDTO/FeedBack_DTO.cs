using MyStore.Models;

namespace MyStore.myDTO
{
    public class FeedBack_DTO
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = null!;
        public string fullnamerely { get; set; } = null!;
        public string noterely { get; set; } = null!;
        public string filenameuser { get; set; } = null!;
        public string filenameuserrely { get; set; } = null!;
        public int? ParentId { get; set; }
        public string Note { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }
        public int? star { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string>? filenames { get; set; } = new List<string>();
        public int UserId { get; set; }
        public int ProductPriceId { get; set; }
        public string ProductName { get; set; } = null!;

    }
}
