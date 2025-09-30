using System.Data;
using MySqlConnector;
using SYJBD.Models;

namespace SYJBD.Data
{
    public class UsersRepository
    {
        private readonly string _cs;

        public UsersRepository(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("Default")
                  ?? throw new InvalidOperationException("ConnectionStrings:Default no configurada.");
        }

        /// <summary>
        /// Valida credenciales por id_usuario o correo (texto plano, igual que tu BD actual).
        /// </summary>
        public async Task<User?> ValidateAsync(string userOrEmail, string password)
        {
            const string sql = @"
SELECT id_usuario, nombre, apellido, correo, rol
FROM Usuarios
WHERE (id_usuario = @u OR correo = @u) AND contraseña = @p
LIMIT 1;";

            await using var cn = new MySqlConnection(_cs);
            await cn.OpenAsync();

            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@u", userOrEmail);
            cmd.Parameters.AddWithValue("@p", password);

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rd.ReadAsync()) return null;

            return new User
            {
                IdUsuario = rd.GetString("id_usuario"),
                Nombre = rd.GetString("nombre"),
                Apellido = rd.GetString("apellido"),
                Correo = rd.IsDBNull(rd.GetOrdinal("correo")) ? null : rd.GetString("correo"),
                Rol = rd.IsDBNull(rd.GetOrdinal("rol")) ? null : rd.GetString("rol"),
            };
        }

        /// <summary>
        /// Obtiene un usuario por ID.
        /// </summary>
        public async Task<User?> GetByIdAsync(string id)
        {
            const string sql = @"
SELECT id_usuario, nombre, apellido, correo, rol
FROM Usuarios
WHERE id_usuario = @id
LIMIT 1;";

            await using var cn = new MySqlConnection(_cs);
            await cn.OpenAsync();

            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rd.ReadAsync()) return null;

            return new User
            {
                IdUsuario = rd.GetString("id_usuario"),
                Nombre = rd.GetString("nombre"),
                Apellido = rd.GetString("apellido"),
                Correo = rd.IsDBNull(rd.GetOrdinal("correo")) ? null : rd.GetString("correo"),
                Rol = rd.IsDBNull(rd.GetOrdinal("rol")) ? null : rd.GetString("rol"),
            };
        }
    }
}
