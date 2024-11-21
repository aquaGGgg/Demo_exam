namespace Test_Demo_Ex.Models
{
    class Order
    {
        int num;
        string name;
        string num_tel;
        string wishes;
        string address;
        string apartmentNumber;
        DateTime checkInDate;
        DateTime checkOutDate;
        string additionalWishes;
        string admin;

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

        public int Num { get => num; set => num = value; }
        public string Name { get => name; set => name = value; }
        public string Num_tel { get => num_tel; set => num_tel = value; }
        public string Wishes { get => wishes; set => wishes = value; }
        public string Address { get => address; set => address = value; }
        public string ApartmentNumber { get => apartmentNumber; set => apartmentNumber = value; }
        public DateTime CheckInDate { get => checkInDate; set => checkInDate = value; }
        public DateTime CheckOutDate { get => checkOutDate; set => checkOutDate = value; }
        public string AdditionalWishes { get => additionalWishes; set => additionalWishes = value; }
        public string Admin { get => admin; set => admin = value; }

        public double TimeInDay() => (CheckOutDate - CheckInDate).TotalDays;

    }
}
