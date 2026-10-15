// "using" lines at the top IMPORT code so this file can use it:
//   CadastroUsuarios.Models  -> our Usuario class
//   CadastroUsuarios.Data    -> our UsuariosDb class (the one that talks to the database)
//   Microsoft.Data.SqlClient -> needed here only to recognize database errors (SqlException)
using CadastroUsuarios.Models;
using CadastroUsuarios.Data;
using Microsoft.Data.SqlClient;

// This class holds the main program: the menu and the screens the person interacts with.
class Program
{
    static UsuariosDb db = new UsuariosDb();
    static void Main()
    {
        bool running = true; //controls the main loop. When it becomes false, the program ends.

        while (running)
        {
            ShowMenu();

            string choice = (Console.ReadLine() ?? string.Empty).Trim();

            try 
            {
                switch (choice) //DEPENDING ON WHAT THE PERSON TYPED, SWITCH CALLS THE METHOD CHOSEN
                {
                    case "1":
                        AddUser();      // Runs the method named AddUser (written further down).
                        break;
                    case "2":
                        EditUser();
                        break;
                    case "3":
                        ListUsers();
                        break;
                    case "4":
                        DeleteUser();
                        break;
                    case "0":
                        running = false; // The loop will stop the next time it checks "running".
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose a number from the menu.");
                        break;
                }
            }
            catch (SqlException ex)
            {

                Console.WriteLine($"Database error: {ex.Message}");
            }
        }

        Console.WriteLine("Goodbye!");
    }

    // ---------------------------------------------------------------------
    // MENU: only prints the options. It doesn't read anything or decide anything.
    // ---------------------------------------------------------------------
    static void ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("========== USER REGISTRY ==========");
        Console.WriteLine("1 - Add a new user");
        Console.WriteLine("2 - Edit a user");
        Console.WriteLine("3 - List users");
        Console.WriteLine("4 - Delete a user");
        Console.WriteLine("0 - Exit");

        // Console.Write stays on the same line, so the person types right after the text.
        Console.Write("Choose an option: ");
    }

    // ---------------------------------------------------------------------
    // 1 - ADD
    // ---------------------------------------------------------------------
    static void AddUser()
    {
        // "new Usuario()" creates a fresh, empty user.
        Usuario novoUser = new Usuario();

        novoUser.ReadUserData();   // Asks the questions and stores the answers in this user.
        db.Adicionar(novoUser);    // Saves the user to SQL Server (and fills in novoUser.Id).

        Console.WriteLine("User saved:");
        novoUser.DisplayUser();
    }

    // ---------------------------------------------------------------------
    // 2 - EDIT
    // ---------------------------------------------------------------------
    static void EditUser()
    {
        Usuario? user = FindUser(); // "?" means: the variable may be null (nothing) if no user was found or chosen.

        // "== null" checks if the variable is empty.
        if (user == null)
        {
            return; // "return" ends this method right here. FindUser already explained what went wrong.
        }

        Console.WriteLine("Current data:");
        user.DisplayUser();
        Console.WriteLine();// BLANK LINE FOR SEPARATION.

        // Changes the values inside the user object. The database is NOT changed yet.
        user.EditUserData();

        // NOW the changes are written to the database.
        // Atualizar answers true if a row was updated, false if the user no longer exists.
        bool updated = db.Atualizar(user);

        if (updated)
        {
            Console.WriteLine("User updated:");
            user.DisplayUser();
        }
        else
        {
            Console.WriteLine("Update failed: the user no longer exists.");
        }
    }

    // ---------------------------------------------------------------------
    // 3 - LIST
    // ---------------------------------------------------------------------
    static void ListUsers()
    {
        // The person decides how many lines to show (minimum 1).
        int amount = ReadInt("How many users do you want to see? ", 1);

        // "List<Usuario>" is a list that holds only Usuario objects.
        // The part inside < > says what type of item the list contains.
        List<Usuario> users = db.Listar(amount);

        // .Count is how many items are in the list.
        if (users.Count == 0)
        {
            Console.WriteLine("There are no users registered yet.");
            return;
        }

        Console.WriteLine();

        // "foreach" means: "for each user in the list, run this block once".
        // Inside the block, the variable "user" is the current one.
        foreach (Usuario user in users)
        {
            user.DisplayUser();
        }
        Console.WriteLine($"Showing {users.Count} user(s).");
    }

    // ---------------------------------------------------------------------
    // 4 - DELETE
    // ---------------------------------------------------------------------
    static void DeleteUser()
    {
        Usuario? user = FindUser();
        if (user == null)
        {
            return;
        }

        Console.WriteLine("You are about to delete this user:");
        user.DisplayUser();

        // Deleting can't be undone, so we ask for confirmation first.
        Console.Write("Type YES to confirm: ");
        string confirmation = (Console.ReadLine() ?? string.Empty).Trim();

        // .Equals compares two texts. "StringComparison.OrdinalIgnoreCase" means
        // capital letters don't matter, so "yes", "YES" and "Yes" all count.
        if (confirmation.Equals("YES", StringComparison.OrdinalIgnoreCase))
        {
            bool deleted = db.Excluir(user.Id);

            // This is a one-line "if / else", written as:  condition ? value if true : value if false
            // Read it as: "if deleted is true, use the first text, otherwise use the second."
            Console.WriteLine(deleted ? "User deleted." : "Delete failed: the user no longer exists.");
        }
        else
        {
            Console.WriteLine("Delete cancelled. Nothing was changed.");
        }
    }

    // ---------------------------------------------------------------------
    // HELPER: finds ONE user, either by Id or by name.
    // Edit and Delete both need this, so it is written only once.
    //
    // "Usuario?" before the method name means: it hands back a user, or null (nothing)
    // if none was found or chosen.
    // ---------------------------------------------------------------------
    static Usuario? FindUser()
    {
        Console.WriteLine("Find the user by:");
        Console.WriteLine("1 - Id");
        Console.WriteLine("2 - Name");
        Console.Write("Choose an option: ");
        string how = (Console.ReadLine() ?? string.Empty).Trim();

        // ----- Search by Id -----
        if (how == "1")
        {
            int id = ReadInt("Insert the user's Id: ", 1);

            // Asks the database for the user with this Id. May come back null.
            Usuario? user = db.BuscarPorId(id);

            if (user == null)
            {
                Console.WriteLine("No user found with that Id.");
            }

            return user; // Hands back the user (or null) to whoever called FindUser.
        }

        // ----- Search by name -----
        if (how == "2")
        {
            Console.Write("Insert the name (or part of it): ");
            string name = (Console.ReadLine() ?? string.Empty).Trim();

            // The database can return several users, so we get a list.
            List<Usuario> matches = db.BuscarPorNome(name);

            if (matches.Count == 0)
            {
                Console.WriteLine("No user found with that name.");
                return null; // "null" = nothing found.
            }

            if (matches.Count == 1)
            {
                // matches[0] = the first item in the list. Counting starts at 0, not at 1.
                return matches[0]; // Only one match, so no need to ask anything else.
            }

            // Several users match. Show them all and ask which Id the person wants.
            Console.WriteLine("More than one user matches:");
            foreach (Usuario match in matches)
            {
                match.DisplayUser();
            }

            int chosenId = ReadInt("Insert the Id of the user you want: ", 1);

            // Look through the matches for the one with the chosen Id.
            foreach (Usuario match in matches)
            {
                if (match.Id == chosenId)
                {
                    return match;
                }
            }

            Console.WriteLine("That Id is not in the list above.");
            return null;
        }

        // If we get here, the person typed something other than 1 or 2.
        Console.WriteLine("Invalid option.");
        return null;
    }

    // ---------------------------------------------------------------------
    // HELPER: keeps asking until the person types a whole number that is at least "min".
    // This prevents crashes when someone types letters instead of numbers.
    //
    // "int" before the method name = it hands back a whole number.
    // The two values in parentheses are inputs the method receives:
    // the question to print, and the smallest number accepted.
    // ---------------------------------------------------------------------
    static int ReadInt(string question, int min)
    {
        while (true) // Repeat until we reach a "return".
        {
            Console.Write(question);
            string text = Console.ReadLine() ?? string.Empty;

            // Typed answers are always text, so we must convert the text into a number.
            // int.TryParse tries the conversion and answers true (worked) or false (failed).
            // If it worked, the number is placed in the new variable "number" (that is what "out int number" does).
            // "&&" means AND: both conditions must be true. ">=" means "greater than or equal to".
            if (int.TryParse(text, out int number) && number >= min)
            {
                return number; // Valid: hand the number back and end the method.
            }
            Console.WriteLine($"Please type a whole number ({min} or higher).");
        }
    }
}
