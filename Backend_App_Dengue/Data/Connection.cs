using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;

namespace Backend_App_Dengue.Data
{
    public class Connection : IDisposable
    {
        protected MySqlConnection? connection;
        MySqlCommand? cmd;
        private readonly string _connectionString;
        private bool _disposed = false;

        public Connection()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = config.GetConnectionString("MySqlConnection")
                ?? throw new InvalidOperationException("MySql connection string not found");
        }

        protected void Conectar()
        {
            try
            {
                if (connection == null || connection.State != ConnectionState.Open)
                {
                    connection = new MySqlConnection(_connectionString);
                    connection.Open();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al conectar a la base de datos", e);
            }
        }

        protected void Desconectar()
        {
            try
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al desconectar: {e.Message}");
            }
        }

        public DataTable ProcedimientosSelect(string[]? Parametros, string NombreProcedimiento, string[]? valores)
        {
            DataTable dt = new DataTable();
            Conectar();
            try
            {
                cmd = new MySqlCommand(NombreProcedimiento, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                if (Parametros != null && valores != null)
                {
                    for (int i = 0; i < Parametros.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@" + Parametros[i], valores[i]);
                    }
                }

                using (MySqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error al ejecutar procedimiento {NombreProcedimiento}", e);
            }
            finally
            {
                Desconectar();
            }
            return dt;
        }

        public void procedimientosInEd(string[]? Parametros, string NombreProcedimiento, string[]? valores)
        {
            Conectar();
            try
            {
                cmd = new MySqlCommand(NombreProcedimiento, connection);
                cmd.CommandType = CommandType.StoredProcedure;

                if (Parametros != null && valores != null)
                {
                    for (int i = 0; i < Parametros.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@" + Parametros[i], valores[i]);
                    }
                }

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"Error al ejecutar procedimiento {NombreProcedimiento}", e);
            }
            finally
            {
                Desconectar();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    cmd?.Dispose();
                    connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
