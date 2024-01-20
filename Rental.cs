namespace Projekt_studia2
{
    using System;

    public class Rental
    {
        public int ID { get; set; }
        public string UserLogin { get; set; }
        public int BookID { get; set; }
        public string BookTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public Rental()
        {
        }

        public Rental(int id, string userLogin, int bookId, DateTime startDate, DateTime endDate)
        {
            ID = id;
            UserLogin = userLogin;
            BookID = bookId;
            StartDate = startDate;
            EndDate = endDate;
        }

    }

}
