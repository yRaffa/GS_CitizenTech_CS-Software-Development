using CitizenTech.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop.Views;

/// <summary>Base das telas: escopo de DI por operação e tratamento padronizado de erros.</summary>
public abstract class ViewBase : UserControl
{
    private readonly IServiceScopeFactory _fabrica;

    protected ViewBase(IServiceScopeFactory fabrica)
    {
        _fabrica = fabrica;
        Dock = DockStyle.Fill;
    }

    /// <summary>Chamado quando a aba é aberta.</summary>
    public virtual Task AtualizarAsync() => Task.CompletedTask;

    protected async Task<T> NoEscopoAsync<T>(Func<IServiceProvider, Task<T>> acao)
    {
        using var escopo = _fabrica.CreateScope();
        return await acao(escopo.ServiceProvider);
    }

    protected async Task ExecutarNoEscopoAsync(Func<IServiceProvider, Task> acao)
    {
        using var escopo = _fabrica.CreateScope();
        await acao(escopo.ServiceProvider);
    }

    /// <summary>Executa a ação mostrando mensagens claras (sem derrubar o programa).</summary>
    protected async Task ExecutarAsync(Func<Task> acao)
    {
        try
        {
            UseWaitCursor = true;
            await acao();
        }
        catch (DomainException ex) { Aviso(ex.Message); }
        catch (NaoEncontradoException ex) { Aviso(ex.Message); }
        catch (Exception ex) { MessageBox.Show($"Erro inesperado: {ex.Message}", "CitizenTech", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; }
    }

    protected static void Aviso(string mensagem) =>
        MessageBox.Show(mensagem, "CitizenTech", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    protected static void Info(string mensagem) =>
        MessageBox.Show(mensagem, "CitizenTech", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
