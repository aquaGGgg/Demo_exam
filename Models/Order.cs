using System.ComponentModel.DataAnnotations;

namespace Test_Demo_Ex.Models
{
    public class Order
    {
        public Order(int num, string name, string num_tel, string wishes, string address, string apartmentNumber, DateTime checkInDate, DateTime checkOutDate, string additionalWishes, string admin)
        {
            Num = num;
            Name = name;
            Num_tel = num_tel;
            Wishes = wishes;
            Address = address;
            ApartmentNumber = apartmentNumber;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            AdditionalWishes = additionalWishes;
            Admin = admin;
        }

        [Key]
        public int Num { get; set; }

        [Required]
        public string Name { get; set; }

        public string Num_tel { get; set; }
        public string Wishes { get; set; }
        public string Address { get; set; }
        public string ApartmentNumber { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public string AdditionalWishes { get; set; }
        public string Admin { get; set; }

        public double TimeInDay() => (CheckOutDate - CheckInDate).TotalDays;


    }
}
