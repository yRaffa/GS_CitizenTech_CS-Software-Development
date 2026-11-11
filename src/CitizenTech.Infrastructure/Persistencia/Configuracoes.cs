using CitizenTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitizenTech.Infrastructure.Persistencia;

public class EstabelecimentoConfig : IEntityTypeConfiguration<Estabelecimento>
{
    public void Configure(EntityTypeBuilder<Estabelecimento> b)
    {
        b.ToTable("Estabelecimentos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).IsRequired().HasMaxLength(150);
        b.Property(x => x.Email).HasMaxLength(150);
        b.HasIndex(x => x.CodigoInep);
        b.Ignore(x => x.Regiao); // propriedade calculada

        b.OwnsOne(x => x.Endereco, e =>
        {
            e.Property(p => p.Logradouro).HasColumnName("Logradouro").IsRequired().HasMaxLength(200);
            e.Property(p => p.Numero).HasColumnName("Numero").HasMaxLength(20);
            e.Property(p => p.Bairro).HasColumnName("Bairro").HasMaxLength(100);
            e.Property(p => p.Cidade).HasColumnName("Cidade").IsRequired().HasMaxLength(100);
            e.Property(p => p.Uf).HasColumnName("Uf").IsRequired().HasMaxLength(2);
        });
        b.Navigation(x => x.Endereco).IsRequired();
    }
}

public class AvaliacaoConfig : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> b)
    {
        b.ToTable("Avaliacoes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Observacoes).HasMaxLength(500);

        b.HasOne(x => x.Estabelecimento).WithMany().HasForeignKey(x => x.EstabelecimentoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Respostas).WithOne().HasForeignKey(r => r.AvaliacaoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Medicoes).WithOne().HasForeignKey(m => m.AvaliacaoId).OnDelete(DeleteBehavior.Cascade);

        // As coleções são expostas como somente leitura: o EF usa o campo privado (_respostas/_medicoes)
        b.Navigation(x => x.Respostas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Medicoes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class RespostaItemConfig : IEntityTypeConfiguration<RespostaItem>
{
    public void Configure(EntityTypeBuilder<RespostaItem> b)
    {
        b.ToTable("RespostasItens");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AvaliacaoId, x.Item }).IsUnique();
    }
}

public class MedicaoConfig : IEntityTypeConfiguration<Medicao>
{
    public void Configure(EntityTypeBuilder<Medicao> b)
    {
        b.ToTable("Medicoes");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AvaliacaoId, x.Tipo }).IsUnique();
    }
}

public class DiagnosticoConfig : IEntityTypeConfiguration<Diagnostico>
{
    public void Configure(EntityTypeBuilder<Diagnostico> b)
    {
        b.ToTable("Diagnosticos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Resumo).HasMaxLength(500);
        b.Property(x => x.ContextoRegional).HasMaxLength(500);
        b.Ignore(x => x.EspecialidadesNecessarias); // propriedade calculada

        b.HasOne(x => x.Avaliacao).WithMany().HasForeignKey(x => x.AvaliacaoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Barreiras).WithOne().HasForeignKey(x => x.DiagnosticoId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Barreiras).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class BarreiraConfig : IEntityTypeConfiguration<Barreira>
{
    public void Configure(EntityTypeBuilder<Barreira> b)
    {
        b.ToTable("Barreiras");
        b.HasKey(x => x.Id);
        b.Property(x => x.Descricao).IsRequired().HasMaxLength(300);
        b.Property(x => x.Recomendacao).HasMaxLength(500);
    }
}

public class ParceiroConfig : IEntityTypeConfiguration<Parceiro>
{
    public void Configure(EntityTypeBuilder<Parceiro> b)
    {
        b.ToTable("Parceiros");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).IsRequired().HasMaxLength(150);
        b.Property(x => x.Descricao).HasMaxLength(500);
        b.Property(x => x.UfsAtendidas).IsRequired().HasMaxLength(200);
        b.Property(x => x.Email).IsRequired().HasMaxLength(150);
        b.Property(x => x.Telefone).HasMaxLength(30);
    }
}

public class EncaminhamentoConfig : IEntityTypeConfiguration<Encaminhamento>
{
    public void Configure(EntityTypeBuilder<Encaminhamento> b)
    {
        b.ToTable("Encaminhamentos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Observacao).HasMaxLength(500);
        b.HasIndex(x => new { x.DiagnosticoId, x.ParceiroId }).IsUnique();

        b.HasOne(x => x.Parceiro).WithMany().HasForeignKey(x => x.ParceiroId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Diagnostico>().WithMany().HasForeignKey(x => x.DiagnosticoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PrevisaoRiscoConfig : IEntityTypeConfiguration<PrevisaoRisco>
{
    public void Configure(EntityTypeBuilder<PrevisaoRisco> b)
    {
        b.ToTable("PrevisoesRisco");
        b.HasKey(x => x.Id);
        b.Property(x => x.Regiao).IsRequired().HasMaxLength(20);
        b.Property(x => x.Uf).IsRequired().HasMaxLength(2);
        b.HasIndex(x => x.CodigoInep).IsUnique();
        b.HasIndex(x => x.Uf);
    }
}
