namespace RylozeWebSecureLoginApp.API.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string? ProductDescription { get; set; }


        public int ProductCategoryId { get; set; }
        public Category ProductCategory { get; set; } = default!;
    }
}
