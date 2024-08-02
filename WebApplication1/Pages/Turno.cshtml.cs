using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pedestal.Models;
using Npgsql;
using System;
using System.Drawing;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;

namespace Pedestal.Pages
{
    public class TurnoModel : PageModel
    {
        private readonly Conexion _conexion;
        private readonly ILogger<TurnoModel> _logger;

        public string MensajeConfirmacion { get; private set; }

        public TurnoModel(ILogger<TurnoModel> logger)
        {
            _conexion = new Conexion();
            _conexion.AbrirConexion();
            _logger = logger;
        }

        public IActionResult OnPost(string turno)
        {
            string caf = HttpContext.Session.GetString("CAF");
            if (string.IsNullOrEmpty(caf))
            {
                return RedirectToPage("/Index");
            }

            if (!int.TryParse(caf, out int cafInt))
            {
                return RedirectToPage("/Index");
            }

            string querySelect = string.Empty;
            string queryInsert = string.Empty;
            int initialNumber = 0;
            string prefix = string.Empty;

            switch (turno)
            {
                case "general":
                    querySelect = "SELECT numero FROM general WHERE caf = @caf AND DATE(fecha) = CURRENT_DATE ORDER BY numero DESC LIMIT 1;";
                    queryInsert = "INSERT INTO general (numero, turno, caf) VALUES (@numero, @turno, @caf);";
                    initialNumber = 100;
                    prefix = "G";
                    break;
                case "preferencial":
                    querySelect = "SELECT numero FROM preferencial WHERE caf = @caf AND DATE(fecha) = CURRENT_DATE ORDER BY numero DESC LIMIT 1;";
                    queryInsert = "INSERT INTO preferencial (numero, turno, caf) VALUES (@numero, @turno, @caf);";
                    initialNumber = 300;
                    prefix = "PF";
                    break;
                case "pendientes":
                    querySelect = "SELECT numero FROM pendiente WHERE caf = @caf AND DATE(fecha) = CURRENT_DATE ORDER BY numero DESC LIMIT 1;";
                    queryInsert = "INSERT INTO pendiente (numero, turno, caf) VALUES (@numero, @turno, @caf);";
                    initialNumber = 500;
                    prefix = "P";
                    break;
                case "magisterio":
                    querySelect = "SELECT numero FROM magisterio WHERE caf = @caf AND DATE(fecha) = CURRENT_DATE ORDER BY numero DESC LIMIT 1;";
                    queryInsert = "INSERT INTO magisterio (numero, turno, caf) VALUES (@numero, @turno, @caf);";
                    initialNumber = 700;
                    prefix = "M";
                    break;
                default:
                    return RedirectToPage("/Index");
            }

            try
            {
                using (var cmdSelect = new NpgsqlCommand(querySelect, _conexion.GetConnection()))
                {
                    cmdSelect.Parameters.AddWithValue("@caf", cafInt);

                    object result = cmdSelect.ExecuteScalar();
                    int lastNumber = result != null ? Convert.ToInt32(result) + 1 : initialNumber;
                    string turnoGenerado = prefix + lastNumber.ToString();

                    using (var cmdInsert = new NpgsqlCommand(queryInsert, _conexion.GetConnection()))
                    {
                        cmdInsert.Parameters.AddWithValue("@numero", lastNumber);
                        cmdInsert.Parameters.AddWithValue("@turno", turnoGenerado);
                        cmdInsert.Parameters.AddWithValue("@caf", cafInt);
                        cmdInsert.ExecuteNonQuery();
                    }

                    // Imprimir el ticket
                    PrintTicket(turnoGenerado);

                    // Establecer el mensaje de confirmación
                    MensajeConfirmacion = $"Su turno {turnoGenerado} ha sido generado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el turno o imprimir el ticket.");
                MensajeConfirmacion = $"Error al generar el turno: {ex.Message}";
            }

            return Page();
        }

        private void PrintTicket(string turnoGenerado)
        {
            try
            {
                // Crear el contenido del ticket con un diseño específico
                string ticketContent = $"********************************\n" +
                                       $"  Bienvenido a Logifarma\n" +
                                       $"********************************\n" +
                                       $"Turno: {turnoGenerado}\n\n\n" +
                                       $"Fecha: {DateTime.Now.ToShortDateString()}\n" +
                                       $"Hora: {DateTime.Now.ToShortTimeString()}\n" +
                                       $"********************************\n" +
                                       $"Servicio de Dispensación";

                // Enviar el ticket a imprimir
                SendToPrinter(ticketContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir el ticket.");
            }
        }

        private void SendToPrinter(string content)
        {
            try
            {
                // Crear un objeto PrintDocument
                PrintDocument pd = new PrintDocument();

                // Asignar el evento PrintPage
                pd.PrintPage += (sender, e) =>
                {
                    // Definir la fuente y el tamaño del texto
                    Font titleFont = new Font("Arial", 14, FontStyle.Bold); // Título en tamaño 14 y negrita
                    Font contentFont = new Font("Arial", 14); // Contenido en tamaño 14

                    // Definir la posición inicial de la impresión
                    float yPos = 10;
                    float leftMargin = 10;

                    // Imprimir el título
                    e.Graphics.DrawString("********************************", titleFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                    yPos += titleFont.GetHeight();

                    // Imprimir el contenido del ticket
                    string[] lines = content.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Turno"))
                        {
                            // Imprimir el turno con un tamaño diferente
                            e.Graphics.DrawString(line, new Font("Arial", 32), Brushes.Black, leftMargin, yPos, new StringFormat());
                        }
                        else
                        {
                            // Imprimir el contenido con la fuente estándar
                            e.Graphics.DrawString(line, contentFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                        }
                        yPos += contentFont.GetHeight();
                    }

                    // Imprimir el separador final
                    //e.Graphics.DrawString("Servicio de Dispensación", titleFont, Brushes.Black, leftMargin, yPos, new StringFormat());

                    // Si hay más páginas para imprimir
                    e.HasMorePages = false;
                };

                // Iniciar la impresión
                pd.Print();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir el ticket.");
            }
        }
    }
}
