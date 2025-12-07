namespace RylozeWebSecureLoginApp.API.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public string? CategoryDescription { get; set; }
    }
}
