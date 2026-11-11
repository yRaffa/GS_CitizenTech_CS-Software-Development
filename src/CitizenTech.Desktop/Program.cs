using CitizenTech.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CitizenTech.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);

        // Mesma composição da API: banco SQLite, repositórios, regras e serviços de aplicação
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.Services.AddCitizenTech(builder.Configuration);
        builder.Services.AddSingleton<MainForm>();

        using var host = builder.Build();
        try
        {
            host.Services.InicializarBancoAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir o banco de dados:\n{ex.Message}", "CitizenTech",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        System.Windows.Forms.Application.Run(host.Services.GetRequiredService<MainForm>());
    }
}
