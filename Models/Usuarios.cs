// "namespace" is a label that groups related code together, like a folder.
// Other files write "using CadastroUsuarios.Models;" at the top to get access
// to everything inside this namespace (here: the Usuario class).
namespace CadastroUsuarios.Models
{
    // A "class" is a blueprint. "Usuario" describes what ONE user looks like.
    // Every user we create from it has its own Id, Name, Age and Address.
    // "public" means other files are allowed to use this class.
    public class Usuario
    {
        // These are "properties": pieces of information stored inside each user.
        //   int    = a whole number (like 35)
        //   string = text (like "Jon Doe")
        //   { get; set; } = the value can be read (get) and changed (set) from outside.
        //
        // Id is the unique number the database gives to each user.
        // "= string.Empty" means: start with blank text instead of "nothing" (null).
        // It only exists to stop C# from showing yellow warnings.
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Address { get; set; } = string.Empty;

        // This is a "constructor": the code that runs when we write "new Usuario()".
        // It has the same name as the class and no return type.
        // Ours is empty on purpose: it creates a user with nothing filled in.
        // Program.cs creates the user first and fills in the details afterwards,
        // once the person types them.
        public Usuario()
        {
        }

        // ---------------------------------------------------------------------
        // ADD: asks the questions and saves each answer inside THIS user.
        //
        // This is a "method": a named block of code that runs when something
        // calls its name. "void" means it does its job but doesn't hand
        // any value back.
        // ---------------------------------------------------------------------
        public void ReadUserData()
        {
            // "while (true)" repeats the block below again and again.
            // It only stops when the code reaches "break".
            // We use it to keep asking until the name is valid.
            while (true)
            {
                // Console.Write prints text and stays on the same line.
                // (Console.WriteLine prints text and then moves to the next line.)
                Console.Write("Insert the User's Name: ");

                // Console.ReadLine() returns what the person typed.
                // "?? string.Empty" means: if nothing came back (null), use blank text instead.
                // Trim() removes accidental spaces at the start and end.
                Name = (Console.ReadLine() ?? string.Empty).Trim();

                // "!=" means "is different from". So: if the name is NOT blank...
                if (Name != string.Empty)
                {
                    break; // ...the name is valid, so leave the loop.
                }
                Console.WriteLine("The name cannot be empty.");
            }

            // Calls the helper method at the bottom of this file.
            // It returns a whole number, which we store in Age.
            Age = ReadAge("Insert the User's Age: ");

            Console.Write("Insert the User's Address: ");
            Address = (Console.ReadLine() ?? string.Empty).Trim();
        }

        // ---------------------------------------------------------------------
        // EDIT: shows the current value of each field and lets the person
        // either type a new value or press Enter to keep the current one.
        // ---------------------------------------------------------------------
        public void EditUserData()
        {
            Console.WriteLine("Press Enter on any field to keep its current value.");

            // --- Name ---
            // A "$" before the quotes lets us place values inside the text using { }.
            // Example: if Name is "Ana", this prints:  Name [Ana]:
            Console.Write($"Name [{Name}]: ");
            string newName = (Console.ReadLine() ?? string.Empty).Trim();

            // "==" compares two things. "=" stores a value. They are different!
            // If the person typed something, replace the name.
            // If they just pressed Enter, newName is blank and nothing changes.
            if (newName != string.Empty)
            {
                Name = newName;
            }

            // --- Age ---
            while (true)
            {
                Console.Write($"Age [{Age}]: ");
                string ageText = (Console.ReadLine() ?? string.Empty).Trim();

                if (ageText == string.Empty)
                {
                    break; // Enter pressed: keep the current age and leave the loop.
                }

                // Typed answers are always text, so we must convert the text to a number.
                // int.TryParse tries the conversion and answers true (worked) or false (failed).
                // If it worked, the number is placed in the new variable "newAge"
                // (that is what "out int newAge" does).
                // "&&" means AND: both things must be true.
                if (int.TryParse(ageText, out int newAge) && newAge >= 0)
                {
                    Age = newAge;
                    break;
                }
                Console.WriteLine("Please type a whole number, or press Enter to keep the current age.");
            }

            // --- Address ---
            Console.Write($"Address [{Address}]: ");
            string newAddress = (Console.ReadLine() ?? string.Empty).Trim();
            if (newAddress != string.Empty)
            {
                Address = newAddress;
            }
        }

        // ---------------------------------------------------------------------
        // DISPLAY: only SHOWS what the user already has. It doesn't ask anything.
        // ---------------------------------------------------------------------
        public void DisplayUser()
        {
            Console.WriteLine($"Id: {Id} - Name: {Name} - Age: {Age} - Address: {Address}");
        }

        // ---------------------------------------------------------------------
        // HELPER: keeps asking until the person types a valid age.
        //
        // "private" = only code inside this class can use it.
        // "int" before the method name = this method hands back a whole number.
        // "return" is the word that hands the value back and ends the method.
        // The text in parentheses ("string question") is an input the method receives.
        // ---------------------------------------------------------------------
        private int ReadAge(string question)
        {
            while (true)
            {
                Console.Write(question);
                string text = Console.ReadLine() ?? string.Empty;

                // Same TryParse idea as above: convert the text without crashing.
                if (int.TryParse(text, out int age) && age >= 0)
                {
                    return age; // Valid number: hand it back and finish.
                }
                Console.WriteLine("Please type a whole number (0 or higher).");
            }
        }
    }
}
