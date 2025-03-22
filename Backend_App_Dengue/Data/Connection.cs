using MySql.Data.MySqlClient;
using System.Data;

namespace Backend_App_Dengue.Data
{
    public class Connection
    {
        protected MySqlConnection? connection;
        MySqlCommand? cmd;


        protected void Conectar()
        {
            try
            {
                connection = new MySqlConnection("Server=158.220.123.106;Port=3306;Database=app_dengue;Uid=tom21;Pwd=0518");
                connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected void Desconectar()
        {
            try
            {
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public DataTable ProcedimientosSelect(string[] Parametros, string NombreProcedimiento, string[] valores)
        {
            DataTable dt = new DataTable();
            Conectar();
            try
            {
                cmd = new MySqlCommand(NombreProcedimiento, connection);
                if (Parametros != null)
                {
                    for (int i = 0; i < Parametros.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@" + Parametros[i], valores[i]);
                    }
                }

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                MySqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                Desconectar();
            }
            return dt;
        }

        public void procedimientosInEd(string[] Parametros, string NombreProcedimiento, string[] valores)
        {
            Conectar();
            try
            {
                cmd = new MySqlCommand(NombreProcedimiento, connection);
                if (Parametros != null)
                {
                    for (int i = 0; i < Parametros.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@" + Parametros[i], valores[i]);
                    }
                }

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                Desconectar();
            }
        }

    }
}
