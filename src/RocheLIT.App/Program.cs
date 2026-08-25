namespace RocheLIT;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        using var splash = new SplashForm();
        splash.Show();
        splash.Update();

        var mainForm = new MainForm();
        Thread.Sleep(1500);

        splash.Close();
        Application.Run(mainForm);
    }
}
