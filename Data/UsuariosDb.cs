// These "using" lines at the top IMPORT code written by others, so this file can use it.
//   Microsoft.Data.SqlClient   -> the tools to talk to SQL Server
//   CadastroUsuarios.Models    -> our own Usuario class
// (Don't confuse them with the "using" INSIDE methods below, which has a different job.)
using Microsoft.Data.SqlClient;
using CadastroUsuarios.Models;

namespace CadastroUsuarios.Data
{
    // This class is the ONLY place that talks to SQL Server.
    // Program.cs never writes SQL. It just calls the methods below by name.
    // Keeping database code in one place makes it easier to find and fix.
    public class UsuariosDb
    {
        // The connection string tells C# how to reach the database:
        //   Server=localhost         -> SQL Server runs on this same computer
        //   Database=Cadastro        -> the name of the database to use
        //   Trusted_Connection=True  -> log in with your current Windows account (no password in the code)
        //   TrustServerCertificate=True -> accept the server's security certificate without extra checks
        //                                  (fine for local testing)
        // "private" = only this class can see it.
        private string connectionString = "Server=localhost; Database=Cadastro; Trusted_Connection=True; TrustServerCertificate=True";

        // ---------------------------------------------------------------------
        // ADD: inserts a new user and writes the new Id back into the object.
        // "void" = this method doesn't hand a value back.
        // "Usuario novoUser" in parentheses = the method receives one user to save.
        // ---------------------------------------------------------------------
        public void Adicionar(Usuario novoUser)
        {
            // "new SqlConnection(...)" creates a connection object using the address above.
            // "using" at the start of this line means: close this connection automatically
            // when the method ends, even if an error happens. This way connections never stay open by accident.
            using SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();

            // This is the SQL command, written as text.
            // The "+" joins pieces of text together (split over lines only to be easier to read).
            // "OUTPUT INSERTED.Id" asks SQL Server to send back the Id it just created.
            // @Name, @Age and @Address are placeholders. The real values are sent separately
            // (below), so what the person types can never be run as SQL code.
            // This protects against an attack called "SQL injection".
            string sql = "INSERT INTO Usuarios (Nome, Idade, Endereco) " +
                         "OUTPUT INSERTED.Id " +
                         "VALUES (@Name, @Age, @Address)";

            using SqlCommand comando = new SqlCommand(sql, conexao);

            // Fill each placeholder with the real value from the user.
            comando.Parameters.AddWithValue("@Name", novoUser.Name);
            comando.Parameters.AddWithValue("@Age", novoUser.Age);
            comando.Parameters.AddWithValue("@Address", novoUser.Address);

            // ExecuteScalar runs the command and gives back ONE single value (here: the new Id).
            // Convert.ToInt32 turns that value into a whole number, and we store it in the user.
            novoUser.Id = Convert.ToInt32(comando.ExecuteScalar());
        }

        // ---------------------------------------------------------------------
        // LIST: returns up to "quantidade" users, ordered by Id.
        //
        // "List<Usuario>" before the method name means: this method hands back a list.
        // The part inside < > says what the list holds: only Usuario objects.
        // ---------------------------------------------------------------------
        public List<Usuario> Listar(int quantidade)
        {
            // Create an empty list. We will fill it with the users found.
            List<Usuario> usuarios = new List<Usuario>();

            using SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();

            // TOP (@Quantidade) = "give me only this many rows".
            // ORDER BY Id = sort the result by Id, lowest first.
            string sql = "SELECT TOP (@Quantidade) Id, Nome, Idade, Endereco FROM Usuarios ORDER BY Id";

            using SqlCommand comando = new SqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Quantidade", quantidade);

            // ExecuteReader runs the command and gives back the result rows.
            // The "reader" lets us go through them one at a time.
            using SqlDataReader leitor = comando.ExecuteReader();

            // leitor.Read() moves to the next row. It answers true if there is one,
            // and false when there are no more rows. So this loop runs once per row.
            while (leitor.Read())
            {
                usuarios.Add(LerUsuario(leitor)); // Convert the row into a Usuario and add it to the list.
            }

            return usuarios; // Hand the finished list back to whoever called this method.
        }

        // ---------------------------------------------------------------------
        // FIND BY ID: returns the user, or null if no user has that Id.
        //
        // "null" means "nothing" / "no value".
        // The "?" after Usuario means: this method is allowed to return null.
        // The code that calls it must check for null before using the result.
        // ---------------------------------------------------------------------
        public Usuario? BuscarPorId(int id)
        {
            using SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();

            // WHERE Id = @Id means: only the row whose Id matches.
            // (In SQL a single "=" compares. In C# a single "=" stores a value.)
            string sql = "SELECT Id, Nome, Idade, Endereco FROM Usuarios WHERE Id = @Id";

            using SqlCommand comando = new SqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Id", id);

            using SqlDataReader leitor = comando.ExecuteReader();

            // "if" runs the block only when the condition is true.
            // Here: if a row was found, convert it and hand it back.
            if (leitor.Read())
            {
                return LerUsuario(leitor);
            }

            return null; // No row found, so we hand back "nothing".
        }

        // ---------------------------------------------------------------------
        // FIND BY NAME: returns every user whose name CONTAINS the text typed.
        // Two users can share the same name, so this returns a list, not one user.
        // ---------------------------------------------------------------------
        public List<Usuario> BuscarPorNome(string nome)
        {
            List<Usuario> usuarios = new List<Usuario>();

            using SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();

            // LIKE searches for text that matches a pattern.
            // The % symbol means "any characters". "%ana%" matches "Ana", "Mariana", "Ana Paula"...
            string sql = "SELECT Id, Nome, Idade, Endereco FROM Usuarios WHERE Nome LIKE @Nome ORDER BY Nome";

            using SqlCommand comando = new SqlCommand(sql, conexao);

            // We add the % on both sides here, in C#, before sending the value.
            comando.Parameters.AddWithValue("@Nome", "%" + nome + "%");

            using SqlDataReader leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                usuarios.Add(LerUsuario(leitor));
            }

            return usuarios;
        }

        // ---------------------------------------------------------------------
        // UPDATE: overwrites Name, Age and Address of the user with this Id.
        //
        // "bool" means this method hands back true or false.
        // Here: true if a row was changed, false if that Id doesn't exist.
        // ---------------------------------------------------------------------
        public bool Atualizar(Usuario usuario)
        {
            using SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();

            // WHERE Id = @Id is the safety filter. Without it, EVERY user in the table would be changed.
            string sql = "UPDATE Usuarios SET Nome = @Name, Idade = @Age, Endereco = @Address WHERE Id = @Id";

            using SqlCommand comando = new SqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Name", usuario.Name);
            comando.Parameters.AddWithValue("@Age", usuario.Age);
            comando.Parameters.AddWithValue("@Address", usuario.Address);
            comando.Parameters.AddWithValue("@Id", usuario.Id);

            // ExecuteNonQuery runs a command that doesn't return rows (like UPDATE or DELETE).
            // It tells us HOW MANY rows were affected.
            int linhasAfetadas = comando.ExecuteNonQuery();

            // ">" means "greater than". If at least 1 row changed, we answer true.
            return linhasAfetadas > 0;
        }

        // ---------------------------------------------------------------------
        // DELETE: removes the user with this Id.
        // Returns true if a row was deleted, false if that Id doesn't exist.
        // ---------------------------------------------------------------------
        public bool Excluir(int id)
        {
            using SqlConnection conexao = new SqlConnection(connectionString);
            conexao.Open();

            // Same safety rule: WHERE Id = @Id makes sure only ONE user is removed.
            string sql = "DELETE FROM Usuarios WHERE Id = @Id";

            using SqlCommand comando = new SqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@Id", id);

            int linhasAfetadas = comando.ExecuteNonQuery();
            return linhasAfetadas > 0;
        }

        // ---------------------------------------------------------------------
        // HELPER: turns the row the reader is currently on into a Usuario object.
        // Listar, BuscarPorId and BuscarPorNome all need this, so we write it
        // once here instead of three times.
        //
        // "private" = only this class can use it.
        // ---------------------------------------------------------------------
        private Usuario LerUsuario(SqlDataReader leitor)
        {
            // "new Usuario()" creates a fresh, empty user.
            Usuario usuario = new Usuario();

            // leitor["Nome"] means: "from the current row, give me the value of the column called Nome".
            // The column values come back as a generic type, so we convert them:
            //   Convert.ToInt32(...)  -> into a whole number
            //   .ToString()           -> into text
            // "?? string.Empty" = if the value is empty (null), use blank text instead.
            usuario.Id = Convert.ToInt32(leitor["Id"]);
            usuario.Name = leitor["Nome"].ToString() ?? string.Empty;
            usuario.Age = Convert.ToInt32(leitor["Idade"]);
            usuario.Address = leitor["Endereco"].ToString() ?? string.Empty;

            return usuario;
        }
    }
}
