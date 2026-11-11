namespace CitizenTech.Application.Utilitarios;

/// <summary>Localiza um arquivo relativo subindo as pastas a partir do diretório do programa e do diretório atual.</summary>
public static class LocalizadorArquivos
{
    public static string? Localizar(string caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho)) return null;
        if (Path.IsPathRooted(caminho)) return File.Exists(caminho) ? caminho : null;

        foreach (var inicio in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var dir = new DirectoryInfo(inicio);
            while (dir is not null)
            {
                var candidato = Path.Combine(dir.FullName, caminho);
                if (File.Exists(candidato)) return candidato;
                dir = dir.Parent;
            }
        }
        return null;
    }
}
