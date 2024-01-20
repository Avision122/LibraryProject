namespace Projekt_studia2
{
    public class Book
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public int AvailableCopies { get; set; }


        public Book()
        {
        }

        public Book(int id, string title, string author, int year, int availableCopies)
        {
            ID = id;
            Title = title;
            Author = author;
            Year = year;
            AvailableCopies = availableCopies;

        }
    }
}
