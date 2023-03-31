namespace Buoi6_TrenLop.Models
{
    public class Cart
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public int Quatity { get; set; }
        public decimal Money
        {
            get
            {
                return Quatity * Price;
            }
        }


    }
}