using Microsoft.Extensions.Configuration;

namespace CitizenTech.Infrastructure.Persistencia;

/// <summary>Resolve o arquivo SQLite. API e app Windows Forms usam o MESMO arquivo por padrão.</summary>
public static class CaminhoBanco
{
    public static string Resolver(IConfiguration configuracao)
    {
        var configurado = configuracao["Banco:CaminhoSqlite"];
        var caminho = string.IsNullOrWhiteSpace(configurado)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CitizenTech", "citizentech.db")
            : Environment.ExpandEnvironmentVariables(configurado);

        caminho = Path.GetFullPath(caminho);
        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        return caminho;
    }
}

/// <summary>Informação exibida no app Windows Forms (para o usuário abrir o mesmo arquivo no DBeaver).</summary>
public record InformacoesBanco(string Caminho);
