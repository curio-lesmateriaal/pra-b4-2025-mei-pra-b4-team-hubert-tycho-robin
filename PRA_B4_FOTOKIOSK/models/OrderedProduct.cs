namespace PRA_B4_FOTOKIOSK.models
{
    public class OrderedProduct
    {
        public int PhotoId { get; set; }
        public string ProductName { get; set; }
        public int Amount { get; set; }
        public double TotalPrice { get; set; }

        public OrderedProduct(int photoId, string productName, int amount, double totalPrice)
        {
            PhotoId = photoId;
            ProductName = productName;
            Amount = amount;
            TotalPrice = totalPrice;
        }
    }
}
