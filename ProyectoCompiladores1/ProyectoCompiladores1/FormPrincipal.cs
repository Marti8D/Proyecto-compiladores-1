using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCompiladores1.Core;
using ProyectoCompiladores1.Models;

namespace ProyectoCompiladores1.UI
{
    /// <summary>
    /// Interfaz gráfica principal del analizador léxico.
    /// Permite al usuario ingresar expresiones regulares, texto a analizar
    /// y visualizar tokens, tabla de símbolos y errores léxicos.
    /// </summary>
    public partial class FormPrincipal : Form
    {
        private readonly AnalizadorLexico _analizador;

        // ─── Controles ─────────────────────────────────────────────────────────

        // Panel izquierdo – Expresiones regulares
        private Panel _panelReglas;
        private Label _lblReglas;
        private DataGridView _gridReglas;
        private Button _btnAgregarRegla;
        private Button _btnEliminarRegla;
        private Button _btnLimpiarReglas;

        // Panel central – Entrada y botón
        private Panel _panelEntrada;
        private Label _lblEntrada;
        private RichTextBox _txtEntrada;
        private Button _btnAnalizar;
        private Button _btnLimpiarTabla;

        // Panel derecho – Resultados
        private TabControl _tabs;
        private TabPage _tabTokens;
        private TabPage _tabTabla;
        private TabPage _tabErrores;
        private DataGridView _gridTokens;
        private DataGridView _gridTabla;
        private RichTextBox _txtErrores;

        // Barra de estado
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblEstado;

        // ───────────────────────────────────────────────────────────────────────

        public FormPrincipal()
        {
            _analizador = new AnalizadorLexico();
            InitializeComponent();
            InicializarInterfaz();
            CargarReglasEjemplo();
        }

        // ─────────────────────────────────────────────
        //  DISEÑO DE LA INTERFAZ
        // ─────────────────────────────────────────────

        private void InitializeComponent()
        {
            this.Text = "Analizador Léxico – Algoritmo de Thompson";
            this.Size = new Size(1300, 800);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 40);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);
        }

        private void InicializarInterfaz()
        {
            // ── Barra de estado ────────────────────────────────────────────────
            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(20, 20, 30) };
            _lblEstado = new ToolStripStatusLabel("Listo") { ForeColor = Color.LightGray };
            _statusStrip.Items.Add(_lblEstado);
            this.Controls.Add(_statusStrip);

            // ── Layout principal: 3 columnas ──────────────────────────────────
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            this.Controls.Add(mainLayout);

            // ── Panel izquierdo: Reglas ───────────────────────────────────────
            _panelReglas = CrearPanel("EXPRESIONES REGULARES");
            CrearPanelReglas();
            mainLayout.Controls.Add(_panelReglas, 0, 0);

            // ── Panel central: Entrada ────────────────────────────────────────
            _panelEntrada = CrearPanel("TEXTO A ANALIZAR");
            CrearPanelEntrada();
            mainLayout.Controls.Add(_panelEntrada, 1, 0);

            // ── Panel derecho: Resultados ─────────────────────────────────────
            CrearPanelResultados();
            var panelResultados = CrearPanel("RESULTADOS");
            panelResultados.Controls.Add(_tabs);
            _tabs.Dock = DockStyle.Fill;
            mainLayout.Controls.Add(panelResultados, 2, 0);
        }

        // ── Helpers de diseño ──────────────────────────────────────────────────

        private Panel CrearPanel(string titulo)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Margin = new Padding(5),
                BackColor = Color.FromArgb(40, 40, 55)
            };
            panel.Paint += (s, e) =>
            {
                e.Graphics.DrawString(titulo, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Brushes.CornflowerBlue, 8, 4);
                using var pen = new Pen(Color.CornflowerBlue, 1);
                e.Graphics.DrawLine(pen, 0, 22, panel.Width, 22);
            };
            panel.Padding = new Padding(8, 26, 8, 8);
            return panel;
        }

        private void CrearPanelReglas()
        {
            // DataGridView de reglas
            _gridReglas = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 42),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(60, 60, 80),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DefaultCellStyle = { BackColor = Color.FromArgb(30, 30, 42), ForeColor = Color.White, SelectionBackColor = Color.CornflowerBlue },
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(50, 50, 70), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) }
            };
            _gridReglas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRegex", HeaderText = "Expresión Regular", FillWeight = 60 });
            _gridReglas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo", HeaderText = "Tipo de Token", FillWeight = 40 });
            _gridReglas.EnableHeadersVisualStyles = false;

            // Botones
            _btnAgregarRegla = CrearBoton("+ Agregar", Color.FromArgb(60, 150, 80));
            _btnEliminarRegla = CrearBoton("✕ Eliminar", Color.FromArgb(160, 60, 60));
            _btnLimpiarReglas = CrearBoton("⟳ Limpiar Todo", Color.FromArgb(80, 80, 120));

            _btnAgregarRegla.Click += BtnAgregarRegla_Click;
            _btnEliminarRegla.Click += BtnEliminarRegla_Click;
            _btnLimpiarReglas.Click += BtnLimpiarReglas_Click;

            var botonesPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight
            };
            botonesPanel.Controls.AddRange(new Control[] { _btnAgregarRegla, _btnEliminarRegla, _btnLimpiarReglas });

            _panelReglas.Controls.Add(_gridReglas);
            _panelReglas.Controls.Add(botonesPanel);
        }

        private void CrearPanelEntrada()
        {
            _lblEntrada = new Label
            {
                Text = "Código fuente / Cadena de texto:",
                Dock = DockStyle.Top,
                ForeColor = Color.LightGray,
                Height = 22
            };

            _txtEntrada = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 32),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 11),
                BorderStyle = BorderStyle.None,
                AcceptsTab = true
            };

            _btnAnalizar = new Button
            {
                Text = "▶  ANALIZAR",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.CornflowerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnAnalizar.FlatAppearance.BorderSize = 0;
            _btnAnalizar.Click += BtnAnalizar_Click;

            _btnLimpiarTabla = new Button
            {
                Text = "Limpiar tabla de símbolos",
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(80, 60, 110),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnLimpiarTabla.FlatAppearance.BorderSize = 0;
            _btnLimpiarTabla.Click += BtnLimpiarTabla_Click;

            _panelEntrada.Controls.Add(_txtEntrada);
            _panelEntrada.Controls.Add(_btnLimpiarTabla);
            _panelEntrada.Controls.Add(_btnAnalizar);
            _panelEntrada.Controls.Add(_lblEntrada);
        }

        private void CrearPanelResultados()
        {
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(120, 30),
                Padding = new Point(10, 6)
            };
            _tabs.DrawItem += Tabs_DrawItem;

            // Tab Tokens
            _tabTokens = new TabPage("📋  Tokens");
            _gridTokens = CrearGridResultados(new[] { "Lexema", "Tipo", "Valor", "Fila", "Columna" });
            _tabTokens.Controls.Add(_gridTokens);
            _tabTokens.BackColor = Color.FromArgb(30, 30, 42);

            // Tab Tabla de Símbolos
            _tabTabla = new TabPage("📚  Tabla de Símbolos");
            _gridTabla = CrearGridResultados(new[] { "Lexema", "Tipo", "Valor", "Fila", "Columna" });
            _tabTabla.Controls.Add(_gridTabla);
            _tabTabla.BackColor = Color.FromArgb(30, 30, 42);

            // Tab Errores
            _tabErrores = new TabPage("⚠️  Errores");
            _txtErrores = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 15, 15),
                ForeColor = Color.Tomato,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            _tabErrores.Controls.Add(_txtErrores);
            _tabErrores.BackColor = Color.FromArgb(30, 30, 42);

            _tabs.TabPages.AddRange(new[] { _tabTokens, _tabTabla, _tabErrores });
        }

        private DataGridView CrearGridResultados(string[] columnas)
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 42),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(60, 60, 80),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DefaultCellStyle = { BackColor = Color.FromArgb(30, 30, 42), ForeColor = Color.White, SelectionBackColor = Color.CornflowerBlue },
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(50, 50, 70), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) }
            };
            grid.EnableHeadersVisualStyles = false;

            foreach (var col in columnas)
                grid.Columns.Add(col, col);

            return grid;
        }

        private Button CrearBoton(string texto, Color color)
        {
            return new Button
            {
                Text = texto,
                Height = 30,
                Width = 100,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 4, 0)
            };
        }

        private void Tabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tab = (TabControl)sender;
            var tabPage = tab.TabPages[e.Index];
            var tabBounds = tab.GetTabRect(e.Index);

            bool selected = e.Index == tab.SelectedIndex;
            var bgColor = selected ? Color.CornflowerBlue : Color.FromArgb(50, 50, 70);

            using var brush = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(brush, tabBounds);

            using var textBrush = new SolidBrush(Color.White);
            e.Graphics.DrawString(tabPage.Text, new Font("Segoe UI", 8.5f, selected ? FontStyle.Bold : FontStyle.Regular),
                textBrush, tabBounds, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        // ─────────────────────────────────────────────
        //  EVENTOS
        // ─────────────────────────────────────────────

        private void BtnAgregarRegla_Click(object sender, EventArgs e)
        {
            using var dialogo = new FormAgregarRegla();
            if (dialogo.ShowDialog(this) == DialogResult.OK)
            {
                string regex = dialogo.Regex;
                string tipo = dialogo.Tipo;

                if (string.IsNullOrWhiteSpace(regex) || string.IsNullOrWhiteSpace(tipo))
                {
                    MostrarError("La expresión regular y el tipo no pueden estar vacíos.");
                    return;
                }

                try
                {
                    // Validar que la expresión se pueda construir
                    Thompson.ConstruirAFN(regex);
                    _gridReglas.Rows.Add(regex, tipo);
                    SetEstado($"Regla agregada: [{tipo}] → {regex}");
                }
                catch (Exception ex)
                {
                    MostrarError($"Expresión regular inválida: {ex.Message}");
                }
            }
        }

        private void BtnEliminarRegla_Click(object sender, EventArgs e)
        {
            if (_gridReglas.SelectedRows.Count == 0) return;
            foreach (DataGridViewRow row in _gridReglas.SelectedRows)
            {
                if (!row.IsNewRow)
                    _gridReglas.Rows.Remove(row);
            }
            SetEstado("Regla(s) eliminada(s).");
        }

        private void BtnLimpiarReglas_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Eliminar todas las reglas?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _gridReglas.Rows.Clear();
                SetEstado("Todas las reglas eliminadas.");
            }
        }

        private void BtnLimpiarTabla_Click(object sender, EventArgs e)
        {
            _analizador.LimpiarTablaSimbolos();
            _gridTabla.Rows.Clear();
            SetEstado("Tabla de símbolos limpiada.");
        }

        private void BtnAnalizar_Click(object sender, EventArgs e)
        {
            string entrada = _txtEntrada.Text;

            if (string.IsNullOrWhiteSpace(entrada))
            {
                MostrarError("Ingresa texto a analizar.");
                return;
            }

            if (_gridReglas.Rows.Count == 0)
            {
                MostrarError("Agrega al menos una expresión regular.");
                return;
            }

            try
            {
                // Reconstruir las reglas cada vez
                _analizador.LimpiarReglas();
                foreach (DataGridViewRow row in _gridReglas.Rows)
                {
                    if (row.IsNewRow) continue;
                    string regex = row.Cells["colRegex"].Value?.ToString() ?? "";
                    string tipo = row.Cells["colTipo"].Value?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(regex) && !string.IsNullOrWhiteSpace(tipo))
                        _analizador.AgregarRegla(regex, tipo);
                }

                var (tokens, errores) = _analizador.Analizar(entrada);

                // Mostrar tokens
                _gridTokens.Rows.Clear();
                foreach (var t in tokens)
                    _gridTokens.Rows.Add(t.Lexema, t.Tipo, t.Valor, t.Fila, t.Columna);

                // Mostrar tabla de símbolos (acumulativa)
                _gridTabla.Rows.Clear();
                foreach (var t in _analizador.ObtenerTablaSimbolos())
                    _gridTabla.Rows.Add(t.Lexema, t.Tipo, t.Valor, t.Fila, t.Columna);

                // Mostrar errores
                _txtErrores.Clear();
                if (errores.Count == 0)
                {
                    _txtErrores.ForeColor = Color.LightGreen;
                    _txtErrores.Text = "✔  No se encontraron errores léxicos.";
                }
                else
                {
                    _txtErrores.ForeColor = Color.Tomato;
                    foreach (var err in errores)
                        _txtErrores.AppendText(err.ToString() + Environment.NewLine);
                    _tabs.SelectedTab = _tabErrores;
                }

                // Resaltar tab de errores si los hay
                SetEstado($"Análisis completado: {tokens.Count} token(s), {errores.Count} error(es).");
            }
            catch (Exception ex)
            {
                MostrarError($"Error durante el análisis: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────
        //  UTILITARIOS
        // ─────────────────────────────────────────────

        private void CargarReglasEjemplo()
        {
            // NOTA: Los caracteres especiales de la regex deben escaparse con \
            // Caracteres especiales: ( ) | * + ? .
            // Todo lo demás (letras, dígitos, -, /, =, ;) NO necesita escape.

            // Identificadores: letra seguida de letras o dígitos
            _gridReglas.Rows.Add(
                @"(a|b|c|d|e|f|g|h|i|j|k|l|m|n|o|p|q|r|s|t|u|v|w|x|y|z)(a|b|c|d|e|f|g|h|i|j|k|l|m|n|o|p|q|r|s|t|u|v|w|x|y|z|0|1|2|3|4|5|6|7|8|9)*",
                "Identificador");

            // Números enteros
            _gridReglas.Rows.Add(
                @"(0|1|2|3|4|5|6|7|8|9)(0|1|2|3|4|5|6|7|8|9)*",
                "Numero");

            // Operadores aritméticos — son caracteres especiales, se escapan con \
            _gridReglas.Rows.Add(@"\+", "OpSuma");
            _gridReglas.Rows.Add(@"-", "OpResta");   // - no es especial
            _gridReglas.Rows.Add(@"\*", "OpMul");
            _gridReglas.Rows.Add(@"\/", "OpDiv");      // / no es especial pero escapado por claridad

            // Asignación
            _gridReglas.Rows.Add(@"=", "Asignacion");

            // Paréntesis — son caracteres especiales, se escapan con \
            _gridReglas.Rows.Add(@"\(", "ParAbre");
            _gridReglas.Rows.Add(@"\)", "ParCierra");

            // Punto y coma — no es especial
            _gridReglas.Rows.Add(@";", "PuntoYComa");
            _gridReglas.Rows.Add(@"\.", "Punto");
        }

        private void SetEstado(string mensaje)
        {
            _lblEstado.Text = mensaje;
        }

        private void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}