using Npgsql;
using System;

namespace Pedestal.Models
{
    public class Conexion
    {
        private NpgsqlConnection _connection;

        public bool ConexionExitosa { get; private set; } = false;

        public Conexion()
        {
            string servidor = "ec2-52-22-136-117.compute-1.amazonaws.com";
            string Bd = "d4gh2v3i5p549d";
            string Usuario = "xupuufxtqinskd";
            string Password = "c8f36e06dda45351374f252d25579eee3de9ef362c3429ed29398e5004bcca48";
            string Puerto = "5432";

            string connectionString = $"Host={servidor};Database={Bd};Username={Usuario};Password={Password};Port={Puerto}";

            _connection = new NpgsqlConnection(connectionString);
        }

        public void AbrirConexion()
        {
            try
            {
                _connection.Open();
                ConexionExitosa = true;
                Console.WriteLine("Conexión exitosa a la base de datos PostgreSQL.");
            }
            catch (NpgsqlException)
            {
                Console.WriteLine("Error en la conexión");
            }
        }

        public void CerrarConexion()
        {
            _connection.Close();
        }

        public NpgsqlConnection GetConnection()
        {
            return _connection;
        }
    }
}
