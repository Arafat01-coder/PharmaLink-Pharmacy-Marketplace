using Microsoft.Data.SqlClient;
using PharmaLinkApp.Database;
using PharmaLinkApp.Forms;

namespace PharmaLinkApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        ///  LoginForm is the single entry point for all three roles
        ///  (SuperAdmin, Admin / pharmacy owner and Customer).
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Anything a form fails to catch arrives here instead of killing the
            // process with the .NET crash dialog. By far the most likely cause is
            // SQL Server not running, so that case gets its own message.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => ReportFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => ReportFatal(e.ExceptionObject as Exception);

            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }

        /// <summary>
        /// Turns an unhandled exception into one message the user can act on.
        /// Database failures use DbHelper.Describe, the same English every form
        /// shows, whether they came through DbHelper (DataAccessException) or
        /// from a service that opened its own connection (raw SqlException).
        /// </summary>
        private static void ReportFatal(Exception ex)
        {
            string message;

            if (ex is DataAccessException || ex is SqlException || ex?.InnerException is SqlException)
            {
                message = DbHelper.Describe(ex);
            }
            else
            {
                message = "PharmaLink hit an unexpected problem.\r\n\r\n" +
                          (ex == null ? "No further detail is available." : ex.Message);
            }

            MessageBox.Show(message, "PharmaLink", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
