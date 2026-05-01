using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoCompiladores1.UI
{
    /// <summary>
    /// Diálogo simple para ingresar una nueva expresión regular y su tipo de token.
    /// </summary>
    public class FormAgregarRegla : Form
    {
        public string Regex { get; private set; }
        public string Tipo  { get; private set; }

        private TextBox _txtRegex;
        private TextBox _txtTipo;
        private Button  _btnAceptar;
        private Button  _btnCancelar;

        public FormAgregarRegla()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Agregar Expresión Regular";
            this.Size            = new Size(480, 230);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(40, 40, 55);
            this.ForeColor       = Color.White;
            this.Font            = new Font("Segoe UI", 10);

            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 3,
                Padding     = new Padding(15),
                BackColor   = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Fila 0: Expresión Regular
            layout.Controls.Add(new Label { Text = "Expresión Regular:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, ForeColor = Color.LightGray }, 0, 0);
            _txtRegex = new TextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = Color.FromArgb(25, 25, 38),
                ForeColor   = Color.LightGreen,
                Font        = new Font("Consolas", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            layout.Controls.Add(_txtRegex, 1, 0);

            // Fila 1: Tipo de token
            layout.Controls.Add(new Label { Text = "Tipo de Token:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, ForeColor = Color.LightGray }, 0, 1);
            _txtTipo = new TextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = Color.FromArgb(25, 25, 38),
                ForeColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Ej: Identificador, Numero, Operador…"
            };
            layout.Controls.Add(_txtTipo, 1, 1);

            // Fila 2: Botones
            var botonesPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 8, 0, 0)
            };

            _btnCancelar = new Button
            {
                Text      = "Cancelar",
                Width     = 100,
                Height    = 34,
                BackColor = Color.FromArgb(90, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            _btnCancelar.FlatAppearance.BorderSize = 0;

            _btnAceptar = new Button
            {
                Text      = "✔  Agregar",
                Width     = 110,
                Height    = 34,
                BackColor = Color.CornflowerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.None
            };
            _btnAceptar.FlatAppearance.BorderSize = 0;
            _btnAceptar.Click += BtnAceptar_Click;

            botonesPanel.Controls.Add(_btnCancelar);
            botonesPanel.Controls.Add(_btnAceptar);

            layout.Controls.Add(botonesPanel, 0, 2);
            layout.SetColumnSpan(botonesPanel, 2);

            this.Controls.Add(layout);
            this.AcceptButton = _btnAceptar;
            this.CancelButton = _btnCancelar;
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtRegex.Text))
            {
                MessageBox.Show("La expresión regular no puede estar vacía.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtRegex.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtTipo.Text))
            {
                MessageBox.Show("El tipo de token no puede estar vacío.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTipo.Focus();
                return;
            }

            Regex  = _txtRegex.Text.Trim();
            Tipo   = _txtTipo.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
