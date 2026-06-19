using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Onester;

public sealed class MainForm : Form
{
    private const string AppTitle = "onester v0.1.0 experimental";

    private readonly NumericUpDown _plateW = CreateNumberBox(600);
    private readonly NumericUpDown _plateH = CreateNumberBox(300);
    private readonly NumericUpDown _diameter = CreateNumberBox(64);
    private readonly NumericUpDown _gap = CreateNumberBox(4.5m);
    private readonly NumericUpDown _edge = CreateNumberBox(7);

    private readonly CheckBox _showGapZones = new()
    {
        Text = "Show full object gap envelope",
        Checked = false,
        AutoSize = true,
        Dock = DockStyle.Fill
    };

    private readonly CheckBox _showCenterField = new()
    {
        Text = "Show edge clearance rectangle",
        Checked = true,
        AutoSize = true,
        Dock = DockStyle.Fill
    };

    private readonly Label _status = new();
    private readonly DrawingPanel _drawingPanel = new();

    private NestingResult? _result;

    public MainForm()
    {
        Text = AppTitle;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1040, 700);
        Width = 1180;
        Height = 760;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 0
        };
        left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

        AddInputRow(left, "Sheet width", _plateW);
        AddInputRow(left, "Sheet height", _plateH);
        AddInputRow(left, "Object diameter", _diameter);
        AddInputRow(left, "Gap between objects", _gap);
        AddInputRow(left, "Edge clearance / border gap", _edge);

        AddFullRow(left, _showGapZones, 30);
        AddFullRow(left, _showCenterField, 30);

        var calculate = new Button { Text = "Calculate", Dock = DockStyle.Fill };
        calculate.Click += (_, _) => Calculate();
        AddFullRow(left, calculate, 38);

        var export = new Button { Text = "Export DXF", Dock = DockStyle.Fill };
        export.Click += (_, _) => ExportDxf();
        AddFullRow(left, export, 38);

        var resultBox = new GroupBox
        {
            Text = "Result",
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };

        _status.Dock = DockStyle.Fill;
        _status.AutoSize = false;
        _status.TextAlign = ContentAlignment.TopLeft;
        _status.Font = new Font(FontFamily.GenericSansSerif, 9.0f);
        resultBox.Controls.Add(_status);
        AddFullRow(left, resultBox, 120);

        var legend = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            Text =
                "Preview legend:\n" +
                "white = sheet border\n" +
                "red = object/circle\n" +
                "yellow = closest pair\n" +
                "dim dash-dot circles = full object gap envelope\n" +
                "dim dash-dot rectangle = edge clearance from sheet border",
            ForeColor = SystemColors.ControlDarkDark
        };
        AddFullRow(left, legend, 120);

        // Flexible spacer keeps controls at the top and prevents any button from stretching.
        int spacerRow = left.RowCount++;
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, spacerRow);
        left.SetColumnSpan(left.GetControlFromPosition(0, spacerRow), 2);

        _drawingPanel.Dock = DockStyle.Fill;
        _drawingPanel.BackColor = Color.FromArgb(34, 34, 34);

        root.Controls.Add(left, 0, 0);
        root.Controls.Add(_drawingPanel, 1, 0);

        _showGapZones.CheckedChanged += (_, _) =>
        {
            _drawingPanel.ShowGapZones = _showGapZones.Checked;
            _drawingPanel.Invalidate();
        };

        _showCenterField.CheckedChanged += (_, _) =>
        {
            _drawingPanel.ShowCenterField = _showCenterField.Checked;
            _drawingPanel.Invalidate();
        };

        Calculate();
    }

    private static NumericUpDown CreateNumberBox(decimal value)
    {
        return new NumericUpDown
        {
            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 100000,
            Increment = 0.5m,
            Value = value,
            Dock = DockStyle.Fill,
            TextAlign = HorizontalAlignment.Right
        };
    }

    private static void AddInputRow(TableLayoutPanel panel, string label, Control control)
    {
        int row = panel.RowCount++;
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        panel.Controls.Add(new Label
        {
            Text = label,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, row);

        panel.Controls.Add(control, 1, row);
    }

    private static void AddFullRow(TableLayoutPanel panel, Control control, int height)
    {
        int row = panel.RowCount++;
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        panel.Controls.Add(control, 0, row);
        panel.SetColumnSpan(control, 2);
    }

    private void Calculate()
    {
        try
        {
            var input = new NestingInput
            {
                PlateWidth = (double)_plateW.Value,
                PlateHeight = (double)_plateH.Value,
                ObjectDiameter = (double)_diameter.Value,
                ObjectGap = (double)_gap.Value,
                EdgeClearance = (double)_edge.Value
            };

            _result = NestingEngine.Calculate(input);
            _drawingPanel.Result = _result;
            _drawingPanel.ShowGapZones = _showGapZones.Checked;
            _drawingPanel.ShowCenterField = _showCenterField.Checked;
            _drawingPanel.Invalidate();

            if (!string.IsNullOrWhiteSpace(_result.Warning))
            {
                _status.Text = _result.Warning;
                return;
            }

            _status.Text =
                $"Count: {_result.Circles.Count} pcs    Pattern: {_result.PatternName}\r\n" +
                $"Minimum gap: {_result.MinimumObjectGap:0.###} mm\r\n" +
                $"Edge clear: {input.EdgeClearance:0.###} mm    Object-border: {(input.EdgeClearance + input.ObjectGap):0.###} mm\r\n" +
                $"Sheet: {input.PlateWidth:0.###} × {input.PlateHeight:0.###} mm    Object Ø: {input.ObjectDiameter:0.###} mm";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Calculation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportDxf()
    {
        if (_result == null || _result.Circles.Count == 0)
        {
            MessageBox.Show(this, "Nothing to export.", "Export DXF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "DXF files (*.dxf)|*.dxf|All files (*.*)|*.*",
            FileName = $"onester-{_result.Circles.Count}pcs.dxf"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            DxfWriter.Write(dialog.FileName, _result);
            MessageBox.Show(this, "DXF exported.", "Export DXF", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Export error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class DrawingPanel : Panel
    {
        public NestingResult? Result { get; set; }
        public bool ShowGapZones { get; set; }
        public bool ShowCenterField { get; set; } = true;

        public DrawingPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);

            if (Result == null)
                return;

            var input = Result.Input;
            if (input.PlateWidth <= 0 || input.PlateHeight <= 0)
                return;

            float pad = 48;
            float scale = Math.Min(
                (ClientSize.Width - 2 * pad) / (float)input.PlateWidth,
                (ClientSize.Height - 2 * pad) / (float)input.PlateHeight);

            if (scale <= 0 || float.IsInfinity(scale))
                return;

            float ox = (ClientSize.Width - (float)input.PlateWidth * scale) / 2;
            float oy = (ClientSize.Height + (float)input.PlateHeight * scale) / 2;

            PointF ToScreen(double x, double y)
            {
                return new PointF(ox + (float)x * scale, oy - (float)y * scale);
            }

            using var borderPen = new Pen(Color.WhiteSmoke, 1.5f);
            using var objectPen = new Pen(Color.FromArgb(232, 78, 78), 1.35f);
            using var textBrush = new SolidBrush(Color.WhiteSmoke);
            using var dimBrush = new SolidBrush(Color.FromArgb(210, 210, 210));
            using var font = new Font(FontFamily.GenericSansSerif, 9);
            using var bigFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);

            var p0 = ToScreen(0, 0);
            var p1 = ToScreen(input.PlateWidth, input.PlateHeight);
            var sheetRect = RectangleF.FromLTRB(p0.X, p1.Y, p1.X, p0.Y);
            e.Graphics.DrawRectangle(borderPen, sheetRect.X, sheetRect.Y, sheetRect.Width, sheetRect.Height);

            double objectRadius = input.ObjectDiameter / 2.0;
            double gapZoneRadius = objectRadius + input.ObjectGap;

            if (ShowGapZones)
            {
                using var gapPen = new Pen(Color.FromArgb(130, 245, 200, 80), 1.05f)
                {
                    DashStyle = DashStyle.DashDot
                };

                foreach (CircleItem c in Result.Circles)
                {
                    var center = ToScreen(c.X, c.Y);
                    float rr = (float)(gapZoneRadius * scale);
                    e.Graphics.DrawEllipse(gapPen, center.X - rr, center.Y - rr, 2 * rr, 2 * rr);
                }
            }

            foreach (CircleItem c in Result.Circles)
            {
                var center = ToScreen(c.X, c.Y);
                float rr = (float)(objectRadius * scale);
                e.Graphics.DrawEllipse(objectPen, center.X - rr, center.Y - rr, 2 * rr, 2 * rr);
            }

            if (ShowCenterField)
            {
                // Visual edge-clearance rectangle:
                // if edge clearance is 7 mm, this rectangle is exactly 7 mm from each sheet border.
                // It shows where the outer boundary of the object+gap envelope is allowed.
                double edge = input.EdgeClearance;
                var sm0 = ToScreen(edge, edge);
                var sm1 = ToScreen(input.PlateWidth - edge, input.PlateHeight - edge);
                var edgeClearanceRect = RectangleF.FromLTRB(sm0.X, sm1.Y, sm1.X, sm0.Y);

                using var edgePen = new Pen(Color.FromArgb(190, 120, 220, 120), 1.6f)
                {
                    DashStyle = DashStyle.DashDot
                };

                e.Graphics.DrawRectangle(edgePen, edgeClearanceRect.X, edgeClearanceRect.Y, edgeClearanceRect.Width, edgeClearanceRect.Height);
            }

            string title = Result.Circles.Count > 0
                ? $"{Result.Circles.Count} pcs   min gap {Result.MinimumObjectGap:0.###} mm"
                : Result.Warning;

            e.Graphics.DrawString(title, bigFont, textBrush, 12, 12);
            e.Graphics.DrawString($"{input.PlateWidth:0.###} × {input.PlateHeight:0.###}   Ø{input.ObjectDiameter:0.###}", font, dimBrush, 12, 34);

            if (Result.Circles.Count >= 2)
            {
                var pair = FindClosestPair(Result.Circles);
                if (pair != null)
                {
                    var a = ToScreen(pair.Value.A.X, pair.Value.A.Y);
                    var b = ToScreen(pair.Value.B.X, pair.Value.B.Y);
                    using var p = new Pen(Color.FromArgb(240, 210, 80), 1.0f);
                    e.Graphics.DrawLine(p, a, b);
                }
            }
        }

        private static (CircleItem A, CircleItem B)? FindClosestPair(List<CircleItem> circles)
        {
            double best = double.PositiveInfinity;
            (CircleItem, CircleItem)? pair = null;

            for (int i = 0; i < circles.Count; i++)
            {
                for (int j = i + 1; j < circles.Count; j++)
                {
                    double dx = circles[i].X - circles[j].X;
                    double dy = circles[i].Y - circles[j].Y;
                    double d = dx * dx + dy * dy;

                    if (d < best)
                    {
                        best = d;
                        pair = (circles[i], circles[j]);
                    }
                }
            }

            return pair;
        }
    }
}
