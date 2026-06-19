using System.Globalization;
using System.Text;

namespace Onester;

public static class DxfWriter
{
    public static void Write(string path, NestingResult result)
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();

        void Add(params object[] values)
        {
            foreach (object value in values)
            {
                if (value is double d)
                    sb.AppendLine(d.ToString("0.######", inv));
                else
                    sb.AppendLine(Convert.ToString(value, inv));
            }
        }

        Add(0, "SECTION", 2, "HEADER",
            9, "$ACADVER", 1, "AC1009",
            9, "$MEASUREMENT", 70, 1,
            9, "$INSUNITS", 70, 4,
            0, "ENDSEC");

        Add(0, "SECTION", 2, "TABLES");
        Add(0, "TABLE", 2, "LTYPE", 70, 1);
        Add(0, "LTYPE", 2, "CONTINUOUS", 70, 0, 3, "Solid line", 72, 65, 73, 0, 40, 0.0);
        Add(0, "ENDTAB");

        Add(0, "TABLE", 2, "LAYER", 70, 3);
        Add(0, "LAYER", 2, "0", 70, 0, 62, 7, 6, "CONTINUOUS");
        Add(0, "LAYER", 2, "BORDER", 70, 0, 62, 7, 6, "CONTINUOUS");
        Add(0, "LAYER", 2, "OBJECTS", 70, 0, 62, 1, 6, "CONTINUOUS");
        Add(0, "ENDTAB");
        Add(0, "ENDSEC");

        Add(0, "SECTION", 2, "ENTITIES");

        double w = result.Input.PlateWidth;
        double h = result.Input.PlateHeight;
        double r = result.Input.ObjectDiameter / 2.0;

        AddLine(0, 0, w, 0);
        AddLine(w, 0, w, h);
        AddLine(w, h, 0, h);
        AddLine(0, h, 0, 0);

        foreach (CircleItem circle in result.Circles)
            Add(0, "CIRCLE", 8, "OBJECTS", 10, circle.X, 20, circle.Y, 30, 0.0, 40, r);

        Add(0, "ENDSEC", 0, "EOF");

        File.WriteAllText(path, sb.ToString(), Encoding.ASCII);

        void AddLine(double x1, double y1, double x2, double y2)
        {
            Add(0, "LINE", 8, "BORDER", 10, x1, 20, y1, 30, 0.0, 11, x2, 21, y2, 31, 0.0);
        }
    }
}
