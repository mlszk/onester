using System;
using System.Collections.Generic;
using System.Linq;

namespace Onester;

public sealed class NestingInput
{
    public double PlateWidth { get; set; } = 600.0;
    public double PlateHeight { get; set; } = 300.0;
    public double ObjectDiameter { get; set; } = 64.0;
    public double ObjectGap { get; set; } = 4.5;
    public double EdgeClearance { get; set; } = 7.0;
}

public sealed class CircleItem
{
    public CircleItem(double x, double y) { X = x; Y = y; }
    public double X { get; }
    public double Y { get; }
}

public sealed class NestingResult
{
    public NestingInput Input { get; init; } = new();
    public List<CircleItem> Circles { get; init; } = new();
    public double MinimumCenterDistance { get; init; }
    public double MinimumObjectGap => Circles.Count < 2 ? 0.0 : MinimumCenterDistance - Input.ObjectDiameter;
    public string PatternName { get; init; } = "";
    public string Warning { get; init; } = "";
}

public static class NestingEngine
{
    public static NestingResult Calculate(NestingInput input)
    {
        Validate(input);

        double radius = input.ObjectDiameter / 2.0;
        double margin = radius + input.EdgeClearance;
        double minCenterDistance = input.ObjectDiameter + input.ObjectGap;
        double innerW = input.PlateWidth - 2.0 * margin;
        double innerH = input.PlateHeight - 2.0 * margin;
        double xMin = margin;
        double yMin = margin;

        if (innerW < 0 || innerH < 0)
            return new NestingResult { Input = input, Warning = "Object plus edge clearance is larger than the sheet." };

        int maxPerRow = Math.Max(1, (int)Math.Floor(innerW / minCenterDistance) + 1);
        int maxRows = Math.Min(60, Math.Max(1, (int)Math.Floor(innerH / (minCenterDistance * 0.45)) + 3));

        Candidate? best = null;

        for (int rows = 1; rows <= maxRows; rows++)
        {
            double yPitch = rows == 1 ? 0.0 : innerH / (rows - 1);

            for (int baseCount = maxPerRow; baseCount >= 1; baseCount--)
            {
                for (int delta = 0; delta <= Math.Min(3, baseCount - 1); delta++)
                {
                    TryPattern(rows, baseCount, delta, false);
                    if (delta > 0) TryPattern(rows, baseCount, delta, true);
                }
            }

            void TryPattern(int rowCount, int baseCount, int delta, bool reversed)
            {
                int[] counts = new int[rowCount];
                for (int r = 0; r < rowCount; r++)
                {
                    bool highRow = (r % 2 == 0) ^ reversed;
                    counts[r] = highRow ? baseCount : baseCount - delta;
                    if (counts[r] < 1) return;
                }

                int maxCount = counts.Max();
                double minPitch = maxCount <= 1 ? 0.0 : minCenterDistance;
                double maxPitch = maxCount <= 1 ? 0.0 : innerW / (maxCount - 1);
                if (maxCount > 1 && maxPitch + 1e-9 < minPitch) return;

                var pitchCandidates = BuildPitchCandidates(minPitch, maxPitch, maxCount);
                if (maxCount == 1) pitchCandidates.Add(0.0);

                foreach (double pitch in pitchCandidates)
                {
                    double maxSlack = counts.Select(c => c <= 1 ? innerW : innerW - (c - 1) * pitch).Min();
                    if (maxSlack < -1e-7) continue;

                    foreach (double offset in BuildOffsetCandidates(pitch, maxSlack))
                    {
                        TryStarts(counts, pitch, yPitch, offset, false);
                        if (offset > 1e-6) TryStarts(counts, pitch, yPitch, offset, true);
                    }
                }
            }

            void TryStarts(int[] counts, double pitch, double yPitch, double offset, bool reversedStarts)
            {
                var circles = new List<CircleItem>();

                for (int row = 0; row < counts.Length; row++)
                {
                    int n = counts[row];
                    double span = n <= 1 ? 0.0 : (n - 1) * pitch;
                    double slack = innerW - span;
                    if (slack < -1e-7) return;

                    bool shifted = (row % 2 == 1) ^ reversedStarts;
                    double start = shifted ? offset : 0.0;
                    if (start < -1e-7 || start > slack + 1e-7) return;
                    if (n == 1) start = innerW / 2.0;

                    double y = yMin + row * yPitch;
                    for (int i = 0; i < n; i++)
                    {
                        double x = n == 1 ? xMin + start : xMin + start + i * pitch;
                        circles.Add(new CircleItem(x, y));
                    }
                }

                Candidate? c = CheckCandidate(input, circles, minCenterDistance);
                if (c == null) return;
                c.PatternName = string.Join("-", counts);
                if (best == null || IsBetter(c, best)) best = c;
            }
        }

        if (best == null) return new NestingResult { Input = input, Warning = "No valid layout found." };

        return new NestingResult
        {
            Input = input,
            Circles = best.Circles,
            MinimumCenterDistance = best.MinimumCenterDistance,
            PatternName = best.PatternName
        };
    }

    private static List<double> BuildPitchCandidates(double minPitch, double maxPitch, int maxCount)
    {
        var values = new List<double>();
        if (maxCount <= 1) return values;

        void Add(double v)
        {
            if (v >= minPitch - 1e-7 && v <= maxPitch + 1e-7) values.Add(v);
        }

        Add(minPitch); Add(maxPitch);

        for (int extra = 1; extra <= 4; extra++)
        {
            double denominator = (maxCount - 1) + 0.5 * extra;
            if (denominator > 0) Add(maxPitch * (maxCount - 1) / denominator);
        }

        for (int i = 0; i <= 90; i++)
        {
            double t = (double)i / 90;
            Add(minPitch + (maxPitch - minPitch) * t);
        }

        return values.DistinctBy(v => Math.Round(v, 6)).OrderBy(v => v).ToList();
    }

    private static List<double> BuildOffsetCandidates(double pitch, double maxSlack)
    {
        var values = new List<double>();

        void Add(double v)
        {
            if (v >= -1e-7 && v <= maxSlack + 1e-7)
                values.Add(Math.Max(0.0, Math.Min(maxSlack, v)));
        }

        Add(0.0); Add(maxSlack); Add(maxSlack / 2.0);

        if (pitch > 0.0)
        {
            Add(pitch / 2.0);
            Add(pitch / 3.0);
            Add(2.0 * pitch / 3.0);
        }

        for (int i = 0; i <= 40; i++) Add(maxSlack * i / 40);

        return values.DistinctBy(v => Math.Round(v, 6)).OrderBy(v => v).ToList();
    }

    private static Candidate? CheckCandidate(NestingInput input, List<CircleItem> circles, double requiredCenterDistance)
    {
        if (circles.Count == 0) return null;
        double radius = input.ObjectDiameter / 2.0;
        double minCenter = double.PositiveInfinity;

        foreach (CircleItem c in circles)
        {
            if (c.X - radius < input.EdgeClearance - 1e-5) return null;
            if (input.PlateWidth - (c.X + radius) < input.EdgeClearance - 1e-5) return null;
            if (c.Y - radius < input.EdgeClearance - 1e-5) return null;
            if (input.PlateHeight - (c.Y + radius) < input.EdgeClearance - 1e-5) return null;
        }

        for (int i = 0; i < circles.Count; i++)
        {
            for (int j = i + 1; j < circles.Count; j++)
            {
                double dx = circles[i].X - circles[j].X;
                double dy = circles[i].Y - circles[j].Y;
                double d = Math.Sqrt(dx * dx + dy * dy);
                minCenter = Math.Min(minCenter, d);
                if (d + 1e-5 < requiredCenterDistance) return null;
            }
        }

        if (double.IsPositiveInfinity(minCenter)) minCenter = 0.0;
        return new Candidate { Circles = circles, MinimumCenterDistance = minCenter };
    }

    private static bool IsBetter(Candidate a, Candidate b)
    {
        if (a.Circles.Count != b.Circles.Count) return a.Circles.Count > b.Circles.Count;
        return a.MinimumCenterDistance > b.MinimumCenterDistance + 1e-6;
    }

    private static void Validate(NestingInput input)
    {
        if (input.PlateWidth <= 0) throw new ArgumentException("Sheet width must be positive.");
        if (input.PlateHeight <= 0) throw new ArgumentException("Sheet height must be positive.");
        if (input.ObjectDiameter <= 0) throw new ArgumentException("Object diameter must be positive.");
        if (input.ObjectGap < 0) throw new ArgumentException("Object gap cannot be negative.");
        if (input.EdgeClearance < 0) throw new ArgumentException("Edge clearance cannot be negative.");
    }

    private sealed class Candidate
    {
        public List<CircleItem> Circles { get; init; } = new();
        public double MinimumCenterDistance { get; init; }
        public string PatternName { get; set; } = "";
    }
}
