namespace CitizenTech.Desktop;

/// <summary>Fábrica de controles (mantém as telas curtas e consistentes).</summary>
internal static class UI
{
    public static Label Rotulo(string texto) =>
        new() { Text = texto, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 8, 3) };

    public static TableLayoutPanel Formulario()
    {
        var t = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(6) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return t;
    }

    public static void Linha(TableLayoutPanel t, string rotulo, Control campo)
    {
        campo.Dock = DockStyle.Fill;
        campo.AccessibleName = rotulo;
        t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(Rotulo(rotulo), 0, t.RowCount - 1);
        t.Controls.Add(campo, 1, t.RowCount - 1);
    }

    public static DataGridView Grade(string nomeAcessivel) => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window, AccessibleName = nomeAcessivel
    };

    public static ComboBox Combo<T>(IEnumerable<T> itens)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var item in itens) c.Items.Add(item!);   // Items (e não DataSource) evita sincronizar combos entre si
        if (c.Items.Count > 0) c.SelectedIndex = 0;
        return c;
    }

    public static Button Botao(string texto, EventHandler aoClicar)
    {
        var b = new Button { Text = texto, AutoSize = true, Padding = new Padding(8, 3, 8, 3), AccessibleName = texto };
        b.Click += aoClicar;
        return b;
    }

    public static FlowLayoutPanel Barra(params Control[] controles)
    {
        var p = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
        p.Controls.AddRange(controles);
        return p;
    }
}
