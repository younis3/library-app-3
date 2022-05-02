using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace LibraryApp3
{
    public class Book
    {
        public int ID { get; }
        public string Title { get; set; }
        public string Author_Fname { get; set; }
        public string Author_Lname { get; set; }
        public string Genre { get; set; }

        public Book(int id, string title, string author_first_name, string author_last_name, string genre = "")
        {
            ID = id;
            Title = title;
            Author_Fname = author_first_name;
            Author_Lname = author_last_name;
            Genre = genre;
        }
        public override bool Equals(object obj)
        {
            bool result = (this.ID == ((Book)obj).ID);
            return result;
        }
    }

    public class PaperBook : Book
    {
        public int Copy_Num { get; set; }

        public PaperBook(int id, string title, string author_first_name, string author_last_name, string genre) : base(id, title, author_first_name, author_last_name, genre)
        {
            Copy_Num = 1;
        }


    }


    public class DigitalBook : Book
    {
        public DigitalBook(int id, string title, string author_first_name, string author_last_name, string genre) : base(id, title, author_first_name, author_last_name, genre)
        {

        }
    }

    public class Subscriber
    {
        public int ID { get; }
        public string Fname { get; set; }
        public string Lname { get; set; }

        public Subscriber(int id, string first_name, string last_name)
        {
            ID = id;
            Fname = first_name;
            Lname = last_name;

        }

        public override bool Equals(object obj)
        {
            bool result = (this.ID == ((Subscriber)obj).ID);
            return result;
        }

    }



    public class Library
    {
        private int Loan_Limit;

        public Library(int loan_limit)
        {
            Loan_Limit = loan_limit;
        }


        public void AddSubscriber(int sub_id, Subscriber subscriber, SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //check if subscriber exists
            cmd.CommandText = $@"SELECT Count(*) FROM subscribers_tbl WHERE Sub_ID = {sub_id}";
            int subCount = 0;
            subCount = Convert.ToInt32(cmd.ExecuteScalar());

            if (subCount > 0)     //subscriber exists
            {
                Console.WriteLine("Subscriber already exists");
            }
            else    //subscriber doesn't exist. Add subscriber to library DB
            {
                cmd.CommandText = $@"INSERT INTO subscribers_tbl(Sub_ID, FirstName, LastName, CurrLoanNum) VALUES
                ({sub_id}, '{subscriber.Fname}','{subscriber.Lname}', 0)";

                cmd.ExecuteNonQuery();
                Console.WriteLine("Subscriber added successfully");
            }
            con.Close();
        }



        public void AddBook(int key, Book book, SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //check if book exists
            cmd.CommandText = $@"SELECT Count(*) FROM books_tbl WHERE Book_ID = {key}";
            int bookCount = 0;
            bookCount = Convert.ToInt32(cmd.ExecuteScalar());

            if (bookCount > 0)     //book exists
            {
                if (book is PaperBook)
                {
                    cmd.CommandText = $@"UPDATE books_tbl SET NumOfCopies = NumOfCopies + 1 WHERE Book_ID = {key}";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Updated number of copies");
                }
                else
                {
                    Console.WriteLine("Book already exists");
                }

            }
            else    //book doesn't exist in the library. Add it
            {
                if (book is PaperBook)
                {
                    cmd.CommandText = $@"INSERT INTO books_tbl(Book_ID, Title, BookType, AuthorFirstName, AuthorLastName, Genre, NumOfCopies) VALUES
                                    ({key}, '{book.Title}', 'paper', '{book.Author_Fname}', '{book.Author_Lname}', '{book.Genre}', {((PaperBook)book).Copy_Num})";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.CommandText = $@"INSERT INTO books_tbl(Book_ID, Title, BookType, AuthorFirstName, AuthorLastName, Genre, NumOfCopies) VALUES
                                    ({key},'{book.Title}', 'digital', '{book.Author_Fname}', '{book.Author_Lname}', '{book.Genre}', -1)";
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Success");
            }
            con.Close();
        }





        public void LoanBook(int inptOpt, string inptBook, int subscriberID, SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //check if subscriber exists
            cmd.CommandText = $@"SELECT Count(*) FROM subscribers_tbl WHERE Sub_ID = {subscriberID}";
            int subCount = 0;
            subCount = Convert.ToInt32(cmd.ExecuteScalar());

            if (subCount == 0)     //subscriber doesn't exist
            {
                Console.WriteLine("Subscriber doesn't exist");
                return;
            }

            int bookID;
            if (inptOpt == 1)   //lookup book by key
            {
                try
                {
                    bookID = int.Parse(inptBook);

                    //check if book exist
                    cmd.CommandText = $@"SELECT Count(*) FROM books_tbl WHERE Book_ID = {bookID}";
                    int bookCount = 0;
                    bookCount = Convert.ToInt32(cmd.ExecuteScalar());

                    if (bookCount == 0)     //book doesn't exist
                    {
                        Console.WriteLine("Book doesn't exist");
                        return;
                    }
                }
                catch (Exception e)    // error in case of user input string instead of int
                {
                    Console.WriteLine("Error! Please type book ID in numbers only!");
                    return;
                }
            }
            else if (inptOpt == 2)      //lookup book by name and loop through all results
            {
                string bookTitle = inptBook;
                cmd.CommandText = $@"SELECT Book_ID, Title, AuthorFirstName, AuthorLastName FROM books_tbl WHERE Title = '{bookTitle}'";
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    Console.WriteLine("No books found matches the same title!");
                    reader.Close();
                    return;
                }

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string title = reader.GetString(1);
                    string fname = reader.GetString(2);
                    string lname = reader.GetString(3);
                    Console.WriteLine("Book ID: " + id + ", Book Info: " + title + ", " + fname + " " + lname);
                }
                reader.Close();
                Console.WriteLine("type Book ID..");
                try
                {
                    bookID = Convert.ToInt32(Console.ReadLine().Trim());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: please type Book ID in numbers only");
                    return;
                }
            }
            else
            {
                return;
            }

            LoanBookHelper(bookID, subscriberID, con);

            con.Close();
        }



        public void LoanBookHelper(int book_id, int sub_id, SqlConnection con)
        {
            SqlCommand cmd_helper = new SqlCommand();
            cmd_helper.Connection = con;

            //check if subscriber already has the book
            cmd_helper.CommandText = $@"SELECT Count(*) FROM loans_tbl WHERE Book_ID = {book_id} AND Sub_ID = {sub_id}";
            int bookCountSub = 0;
            bookCountSub = Convert.ToInt32(cmd_helper.ExecuteScalar());
            if (bookCountSub > 0)     //subscriber already has the book
            {
                Console.WriteLine("Subscriber already has the book");
                return;
            }

            //check subscriber if reached loan limit
            cmd_helper.CommandText = $@"SELECT CurrLoanNum FROM subscribers_tbl WHERE Sub_ID = {sub_id}";
            var reader_helper = cmd_helper.ExecuteReader();
            int loanNum;
            if (reader_helper.Read())
            {
                loanNum = reader_helper.GetInt32(0);
            }
            else
            {
                Console.WriteLine("Error");
                return;
            }
            reader_helper.Close();

            if (loanNum < Loan_Limit)   //only if subscriber didn't reach loan limit allow order
            {
                //check book type
                cmd_helper.CommandText = $@"SELECT BookType, NumOfCopies FROM books_tbl WHERE Book_ID = {book_id}";
                reader_helper = cmd_helper.ExecuteReader();
                string book_type;
                int copiesNum;
                if (reader_helper.Read())
                {
                    book_type = reader_helper.GetString(0);
                    copiesNum = reader_helper.GetInt32(1);
                }
                else
                {
                    Console.WriteLine("Error");
                    return;
                }
                reader_helper.Close();

                if (book_type == "paper")
                {
                    if (copiesNum > 0)
                    {
                        cmd_helper.CommandText = $@"UPDATE books_tbl SET NumOfCopies = NumOfCopies - 1 WHERE Book_ID = {book_id}";
                        cmd_helper.ExecuteNonQuery();
                        cmd_helper.CommandText = $@"UPDATE subscribers_tbl SET CurrLoanNum = CurrLoanNum + 1 WHERE Sub_ID = {sub_id}";
                        cmd_helper.ExecuteNonQuery();
                        Console.WriteLine("Paper Book successfully loaned");

                        //insert into loans table
                        cmd_helper.CommandText = $@"INSERT INTO loans_tbl(Sub_ID, Book_ID) VALUES
                                            ({sub_id}, {book_id})";
                        cmd_helper.ExecuteNonQuery();
                    }
                    else
                    {
                        Console.WriteLine("All copies of the book are already taken");
                    }
                }
                else
                {
                    cmd_helper.CommandText = $@"UPDATE subscribers_tbl SET CurrLoanNum = CurrLoanNum + 1 WHERE Sub_ID = {sub_id}";
                    cmd_helper.ExecuteNonQuery();
                    Console.WriteLine("Digital Book successfully loaned");

                    //insert into loans table
                    cmd_helper.CommandText = $@"INSERT INTO loans_tbl(Sub_ID, Book_ID) VALUES
                                            ({sub_id}, {book_id})";
                    cmd_helper.ExecuteNonQuery();
                }
            }
            else
            {
                Console.WriteLine("Subscriber reached loan limit");
            }
        }





        public void ReturnBook(int sub_id, int book_key, SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //check if subscriber exists
            cmd.CommandText = $@"SELECT Count(*) FROM subscribers_tbl WHERE Sub_ID = {sub_id}";
            int subCount = 0;
            subCount = Convert.ToInt32(cmd.ExecuteScalar());

            if (subCount == 0)     //subscriber doesn't exist
            {
                Console.WriteLine("Subscriber doesn't exist");
                return;
            }

            //check if book exist
            cmd.CommandText = $@"SELECT Count(*) FROM books_tbl WHERE Book_ID = {book_key}";
            int bookCount = 0;
            bookCount = Convert.ToInt32(cmd.ExecuteScalar());

            if (bookCount == 0)     //book doesn't exist
            {
                Console.WriteLine("Book doesn't exist");
                return;
            }


            //check if subscriber has the book
            cmd.CommandText = $@"SELECT Count(*) FROM loans_tbl WHERE Book_ID = {book_key} AND Sub_ID = {sub_id}";
            int bookCountSub = 0;
            bookCountSub = Convert.ToInt32(cmd.ExecuteScalar());
            if (bookCountSub == 0)     //book doesn't exist in subscriber loaned books list
            {
                Console.WriteLine("Book doesn't exist in subscriber loaned books list");
                return;
            }


            //check book type
            cmd.CommandText = $@"SELECT BookType, NumOfCopies FROM books_tbl WHERE Book_ID = {book_key}";
            var reader = cmd.ExecuteReader();
            string book_type;
            int copiesNum;
            if (reader.Read())
            {
                book_type = reader.GetString(0);
                copiesNum = reader.GetInt32(1);
            }
            else
            {
                Console.WriteLine("Error");
                return;
            }
            reader.Close();


            //if all success. Return book
            if (book_type == "paper")
            {
                cmd.CommandText = $@"UPDATE books_tbl SET NumOfCopies = NumOfCopies + 1 WHERE Book_ID = {book_key}";
                cmd.ExecuteNonQuery();
            }
            cmd.CommandText = $@"UPDATE subscribers_tbl SET CurrLoanNum = CurrLoanNum - 1 WHERE Sub_ID = {sub_id}";
            cmd.ExecuteNonQuery();

            //remove from loans table
            cmd.CommandText = $@"DELETE FROM loans_tbl WHERE Book_ID = {book_key} AND Sub_ID = {sub_id}";
            cmd.ExecuteNonQuery();
            Console.WriteLine("Book returned successfully");

            con.Close();
        }




        public void PrintBookInfo(string bookName, string authorFirstName, string authorLastName, SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //get book details
            cmd.CommandText = $@"SELECT BookType, Genre, NumOfCopies FROM books_tbl WHERE Title = '{bookName}' AND AuthorFirstName = '{authorFirstName}'AND AuthorLastName = '{authorLastName}'";
            var reader = cmd.ExecuteReader();
            string book_type;
            string genre;
            int copiesNum;
            if (reader.Read())
            {
                book_type = reader.GetString(0);
                genre = reader.GetString(1);
                copiesNum = reader.GetInt32(2);
            }
            else
            {
                Console.WriteLine("Book was not found!");
                reader.Close();
                return;
            }
            reader.Close();

            //print book details
            if (book_type == "paper")
            {
                Console.WriteLine(bookName + ", paper-book, " + genre + ", number of available copies: " + copiesNum);
            }
            else
            {
                Console.WriteLine(bookName + ", digital-book, " + genre);
            }
            con.Close();
        }




        public void PrintBooksByGenre(string genre, SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            string query = $@"SELECT Book_ID, Title, AuthorFirstName, AuthorLastName FROM books_tbl WHERE Genre = '{genre}'";

            //add results to dataset in order to iterate multiple rows
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = new SqlCommand(query, con);
            DataSet set = new DataSet("genreBooks");
            int rowsNum = adapter.Fill(set, "books_tbl");   //fill dataset and return num of rows to check if it's empty or not

            if (rowsNum == 0)   //no results found in dataset
            {
                Console.WriteLine("No Books were found with this genre!");
                return;
            }

            foreach (DataTable table in set.Tables)
            {
                foreach (DataRow dr in table.Rows)
                {
                    string book_id = dr["Book_ID"].ToString();
                    string title = dr["Title"].ToString();
                    string first_name = dr["AuthorFirstName"].ToString();
                    string last_name = dr["AuthorLastName"].ToString();
                    Console.WriteLine(book_id + ", " + title + ", " + first_name + " " + last_name);
                }
            }
            con.Close();
        }





        public void ShowSubBooks(int subscriber_id, SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //check if subscriber exists
            cmd.CommandText = $@"SELECT Count(*) FROM subscribers_tbl WHERE Sub_ID = {subscriber_id}";
            int subCount = 0;
            subCount = Convert.ToInt32(cmd.ExecuteScalar());

            if (subCount == 0)     //subscriber doesn't exist
            {
                Console.WriteLine("Subscriber doesn't exist");
                con.Close();
                return;
            }

            string query = $@"SELECT books_tbl.Book_ID, books_tbl.Title, books_tbl.AuthorFirstName, books_tbl.AuthorLastName
                            FROM books_tbl, loans_tbl
                            WHERE loans_tbl.Sub_ID = {subscriber_id} AND loans_tbl.Book_ID = books_tbl.Book_ID ";

            //add results to dataset in order to iterate multiple rows
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = new SqlCommand(query, con);
            DataSet set = new DataSet("subBooks");
            int rowsNum = adapter.Fill(set, "books_tbl");   //fill dataset and return num of rows to check if it's empty or not

            if (rowsNum == 0)   //no results found in dataset
            {
                Console.WriteLine("Subscriber doesn't own any books");
                return;
            }

            foreach (DataTable table in set.Tables)
            {
                foreach (DataRow dr in table.Rows)
                {
                    string book_id = dr["Book_ID"].ToString();
                    string title = dr["Title"].ToString();
                    string first_name = dr["AuthorFirstName"].ToString();
                    string last_name = dr["AuthorLastName"].ToString();

                    Console.WriteLine("Book ID: " + book_id + ", Title: " + title + ", Author: " + first_name + " " + last_name);
                }
            }
            con.Close();
        }




        //Create Database Tables
        public bool Create_Tables(SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            int tblCount;


            //books
            //check if table exists
            /*
             * I used this method instead of dropping since I coudn't drop tables which has foreign keys so I decided to check if table exists or not
             * */
            cmd.CommandText = @"SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'books_tbl'";
            tblCount = 0;
            tblCount = Convert.ToInt32(cmd.ExecuteScalar());  //ExecuteScalar is used because select COUNT returns only one result (no need for while(read))

            if (tblCount == 0)     //table doesn't exist, Create it
            {
                cmd.CommandText = @"CREATE TABLE books_tbl(
                                Book_ID INT NOT NULL PRIMARY KEY,
                                Title VARCHAR(255),
	                            BookType VARCHAR(255),
	                            AuthorFirstName VARCHAR(255),
	                            AuthorLastName VARCHAR(255),
	                            Genre VARCHAR(255),
	                            NumOfCopies INT
                                )";
                cmd.ExecuteNonQuery();
                Console.WriteLine("books_tbl created successfully");
            }


            //subscribers
            //check if table exists
            cmd.CommandText = @"SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'subscribers_tbl'";
            tblCount = 0;
            tblCount = Convert.ToInt32(cmd.ExecuteScalar());
            if (tblCount == 0)     //table doesn't exist, Create it
            {
                cmd.CommandText = @"CREATE TABLE subscribers_tbl(
                                Sub_ID INT NOT NULL PRIMARY KEY,
	                            FirstName VARCHAR(255),
	                            LastName VARCHAR(255),
	                            CurrLoanNum INT
                                )";
                cmd.ExecuteNonQuery();
                Console.WriteLine("subscribers_tbl created successfully");
            }


            //loans
            //check if table exists
            cmd.CommandText = @"SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'loans_tbl'";
            tblCount = 0;
            tblCount = Convert.ToInt32(cmd.ExecuteScalar());
            if (tblCount == 0)     //table doesn't exist, Create it
            {
                cmd.CommandText = @"CREATE TABLE loans_tbl(
                                Loan_ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	                            Sub_ID INT FOREIGN KEY REFERENCES subscribers_tbl(Sub_ID),
	                            Book_ID INT FOREIGN KEY REFERENCES books_tbl(Book_ID)
                                )";
                cmd.ExecuteNonQuery();
                Console.WriteLine("loans_tbl created successfully");

            }

            con.Close();

            if (tblCount > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public void InsertInitialData(SqlConnection con)
        {
            if (con != null && con.State == ConnectionState.Closed)   //check if connection is open to prevent errors
            {
                con.Open();
            }
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //books
            cmd.CommandText = @"INSERT INTO books_tbl(Book_ID, Title, BookType, AuthorFirstName, AuthorLastName, Genre, NumOfCopies) VALUES
                (100, 'WW2','paper', 'John', 'Ve', 'History', 1),
                (101, 'Java Lessons','paper', 'Robin', 'Sam', 'Tech', 11),
                (102, 'Machine Learning','digital', 'Sera', 'Voiski', 'Tech', -1), 
                (103, 'Dead Sea','paper', 'Rena', 'Jackson', 'Geography', 5),
                (104, 'Hidden In Mars','paper', 'Rami', 'Shein', 'Sci-Fi', 4)";

            cmd.ExecuteNonQuery();

            //subscribers
            cmd.CommandText = @"INSERT INTO subscribers_tbl(Sub_ID, FirstName, LastName, CurrLoanNum) VALUES
                (204443162, 'Ahmad','Younis', 0),
                (200025111, 'Sena','Kaf', 0),
                (111222333, 'Robi','Beinze', 0),
                (333555111, 'Jerry','Sak', 0),
                (888444012, 'Michael','Fen', 0)";

            cmd.ExecuteNonQuery();
            Console.WriteLine("Data initialized successfully");

            con.Close();
        }



    }


    class Program
    {
        static void Main(string[] args)
        {

            // Connect to DB

            string cs = @"Server=localhost\SQLEXPRESS;Database=library_db_test;Trusted_Connection=True;";

            SqlConnection con = new SqlConnection(cs);

            con.Open();

            string ver_query = "SELECT @@VERSION";

            SqlCommand cmd = new SqlCommand(ver_query, con);

            string version = cmd.ExecuteScalar().ToString();

            Console.WriteLine(version);
            con.Close();

            //--------------------------------------------------------------------

            const int LOAN_LIMIT = 3;    //max num of books a subscriber can loan
            Console.WriteLine("Library Application Started");
            Library Lib = new Library(LOAN_LIMIT);


            //Creating DB tables
            bool isCreatedNew = Lib.Create_Tables(con);     //returns true if tables newly created
            if (isCreatedNew)    //only if created new tables initialize them with some data
            {
                Lib.InsertInitialData(con);
            }

            bool app_over = false;

            while (!app_over)
            {
                Console.WriteLine("------------------------ Main Menu ---------------------");
                Console.WriteLine("------------ Choose activity 1-3. 4 to Exit ------------");
                Console.WriteLine("1: Library Management");
                Console.WriteLine("2: Book Renting");
                Console.WriteLine("3: Print Information");
                Console.WriteLine("4: Exit Library");
                Console.WriteLine("--------------------------------------------------------");

                int option;
                int option_sub_menu;
                try
                {
                    option = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Please Choose activity 1-3. 4 to Exit");
                    continue;
                }

                switch (option)
                {
                    case 1:
                        //Main Menu: Library Management
                        {
                            Console.WriteLine("------------------------ Library Management ---------------------");
                            Console.WriteLine("0: Back to Main Menu");
                            Console.WriteLine("1: Add New Book");
                            Console.WriteLine("2: Add New Subscriber");
                            Console.WriteLine("3: Exit Library");

                            try
                            {
                                option_sub_menu = Convert.ToInt32(Console.ReadLine());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: Please Choose activity. 3 to Exit");
                                continue;
                            }

                            switch (option_sub_menu)
                            {
                                case 0:
                                    //back to main menu
                                    {
                                        break;
                                    }
                                case 1:
                                    //add new book
                                    {
                                        Console.WriteLine("Add new book..");

                                        Book book;

                                        Console.WriteLine("Please enter book type : paper or digital");
                                        string book_type = Console.ReadLine();
                                        if (!(book_type == "paper" || book_type == "digital"))
                                        {
                                            Console.WriteLine("Error!, Book Type must be either 'paper' or 'digital' ");
                                            continue;
                                        }

                                        //enter book key and check validation
                                        Console.WriteLine("Please enter book key (up to 7 numbers)");
                                        string inpt_key = Console.ReadLine();
                                        if (inpt_key.Length > 7)
                                        {
                                            Console.WriteLine("Key can't be more than 7 numbers");
                                            continue;
                                        }
                                        int book_key;
                                        try
                                        {
                                            book_key = int.Parse(inpt_key);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Error: Please use numbers only for book key");
                                            continue;
                                        }

                                        Console.WriteLine("Please enter book title");
                                        string book_title = Console.ReadLine();
                                        Console.WriteLine("Please enter book author first name");
                                        string author_first_name = Console.ReadLine();
                                        Console.WriteLine("Please enter book author last name");
                                        string author_last_name = Console.ReadLine();
                                        Console.WriteLine("Please enter book genre");
                                        string book_genre = Console.ReadLine();

                                        if (book_type == "paper")
                                        {
                                            book = new PaperBook(book_key, book_title, author_first_name, author_last_name, book_genre);
                                        }
                                        else
                                        {
                                            book = new DigitalBook(book_key, book_title, author_first_name, author_last_name, book_genre);
                                        }

                                        Lib.AddBook(book_key, book, con);
                                        break;
                                    }
                                case 2:
                                    //add new subscriber
                                    {
                                        Console.WriteLine("Add new subscriber..");

                                        Subscriber sub;

                                        Console.WriteLine("Input Subsriber ID");
                                        string subID = Console.ReadLine().Trim();
                                        int sub_id;
                                        try
                                        {
                                            sub_id = int.Parse(subID);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Error: Please type only numbers for subscriber ID");
                                            continue;
                                        }
                                        Console.WriteLine("Input Subsriber First Name");
                                        string subFirstName = Console.ReadLine().Trim();
                                        Console.WriteLine("Input Subsriber Last Name");
                                        string subLastName = Console.ReadLine().Trim();
                                        sub = new Subscriber(sub_id, subFirstName, subLastName);
                                        Lib.AddSubscriber(sub_id, sub, con);

                                        break;
                                    }
                                case 3:
                                    //exit
                                    {
                                        Console.WriteLine("Good Bye!");
                                        app_over = true;
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    case 2:
                        //Main Menu: Book Renting
                        {
                            Console.WriteLine("------------------------ Book Renting ---------------------");
                            Console.WriteLine("0: Back to Main Menu");
                            Console.WriteLine("1: Laon a Book");
                            Console.WriteLine("2: Return a Book");
                            Console.WriteLine("3: Exit Library");

                            try
                            {
                                option_sub_menu = Convert.ToInt32(Console.ReadLine());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: Please Choose activity. 3 to Exit");
                                continue;
                            }

                            switch (option_sub_menu)
                            {
                                case 0:
                                    //back to main menu
                                    {
                                        break;
                                    }
                                case 1:
                                    //loan a book
                                    {
                                        Console.WriteLine("Laon Book..");
                                        Console.WriteLine("1. Choose Book by ID    2. Choose Book By Title");
                                        int inptOpt;

                                        try
                                        {
                                            inptOpt = Convert.ToInt32(Console.ReadLine().Trim());
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Error! Please choose 1 or 2 only!");
                                            continue;
                                        }

                                        string inptBook;
                                        if (inptOpt == 1)
                                        {
                                            Console.WriteLine("Type Book ID");
                                            inptBook = Console.ReadLine().Trim();
                                        }
                                        else if (inptOpt == 2)
                                        {
                                            Console.WriteLine("Type Book Title");
                                            inptBook = Console.ReadLine().Trim();
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error! Please choose 1 or 2 only!");
                                            continue;
                                        }

                                        Console.WriteLine("Input Subscriber ID");
                                        string inptSubID = Console.ReadLine().Trim();
                                        int sub_id;
                                        try
                                        {
                                            sub_id = int.Parse(inptSubID);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Error: type Subscriber ID in numbers only");
                                            continue;
                                        }

                                        Lib.LoanBook(inptOpt, inptBook, sub_id, con);

                                        break;
                                    }
                                case 2:
                                    //return a book
                                    {
                                        Console.WriteLine("Return a Book..");

                                        Console.WriteLine("Input Subscriber ID");
                                        string inptSubID = Console.ReadLine().Trim();

                                        Console.WriteLine("Input Book ID");
                                        string BookID = Console.ReadLine().Trim();

                                        int sub_id;
                                        int book_key;
                                        try
                                        {
                                            sub_id = Convert.ToInt32(inptSubID);
                                            book_key = Convert.ToInt32(BookID);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Error: Please use numbers only for book and subscriber IDs");
                                            continue;
                                        }

                                        Lib.ReturnBook(sub_id, book_key, con);

                                        break;
                                    }
                                case 3:
                                    //exit
                                    {
                                        Console.WriteLine("Good Bye!");
                                        app_over = true;
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    case 3:
                        //Main Menu: Print Information
                        {
                            Console.WriteLine("------------------------ Print Information ---------------------");
                            Console.WriteLine("0: Back to Main Menu");
                            Console.WriteLine("1: Print Book Details");
                            Console.WriteLine("2: Print Book by Genre");
                            Console.WriteLine("3: Show Subscriber Books List");
                            Console.WriteLine("4: Exit Library");

                            try
                            {
                                option_sub_menu = Convert.ToInt32(Console.ReadLine());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: Please Choose activity. 4 to Exit");
                                continue;
                            }

                            switch (option_sub_menu)
                            {
                                case 0:
                                    //back to main menu
                                    {
                                        break;
                                    }
                                case 1:
                                    //print Book Details"
                                    {
                                        Console.WriteLine("Print Book Details..");

                                        Console.WriteLine("Input Book Name");
                                        string inptBookName = Console.ReadLine().Trim();
                                        Console.WriteLine("Input Author First Name");
                                        string inptAuthorFirstName = Console.ReadLine().Trim();
                                        Console.WriteLine("Input Author Last Name");
                                        string inptAuthorLastName = Console.ReadLine().Trim();

                                        Lib.PrintBookInfo(inptBookName, inptAuthorFirstName, inptAuthorLastName, con);
                                        break;
                                    }
                                case 2:
                                    //print Book by Genre
                                    {
                                        Console.WriteLine("Print Book by Genre..");

                                        Console.WriteLine("Input Genre");
                                        string inptGenre = Console.ReadLine().Trim();
                                        Lib.PrintBooksByGenre(inptGenre, con);
                                        break;
                                    }
                                case 3:
                                    //show Subscriber Books List
                                    {
                                        Console.WriteLine("Show Subscriber Books List..");

                                        Console.WriteLine("Enter subscriber ID");
                                        string inpt_id = Console.ReadLine().Trim();
                                        int sub_id;
                                        try
                                        {
                                            sub_id = Convert.ToInt32(inpt_id);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Error: Only numbers allowed for subscriber ID");
                                            continue;
                                        }

                                        Lib.ShowSubBooks(sub_id, con);
                                        break;
                                    }
                                case 4:
                                    //exit
                                    {
                                        Console.WriteLine("Good Bye!");
                                        app_over = true;
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    case 4:
                        //Main Menu: Exit
                        {
                            Console.WriteLine("Good Bye!");
                            app_over = true;
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
	
}


