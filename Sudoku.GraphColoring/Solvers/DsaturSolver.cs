namespace Sudoku.GraphColoring.Solvers;

using Vertex = int;
using Color = int;

// https://fr.wikipedia.org/wiki/DSATUR
public class DsaturSolver : ISudokuGraphSolver
{
    public void Solve(SudokuGraph graph)
    {
        DsaturSolver.SolveRecursive(graph);
    }

    private static bool SolveRecursive(SudokuGraph graph)
    {
        Vertex? source = DsaturSolver.BlankMaxSaturation(graph);
        if (!source.HasValue)
            return true;

        foreach (Color color in graph.AvailableColors(source.Value)) {
            graph[source.Value] = color;
            if (DsaturSolver.SolveRecursive(graph))
                return true;
        }

        graph[source.Value] = SudokuGraph.Blank;
        return false;
    }

    private static Vertex? BlankMaxSaturation(SudokuGraph graph)
    {
        Vertex? max = null;
        int maxSaturation = 0;
        int maxDegree = 0;
        for (Vertex vertex = 0; vertex < graph.GridSize; ++vertex)
        {
            Color color = graph[vertex];
            if (color != SudokuGraph.Blank)
                continue;
            int saturation = DsaturSolver.Saturation(graph, vertex);
            int degree = graph.Degree(vertex);
            bool unset = !max.HasValue;
            bool greaterSaturation = (saturation > maxSaturation);
            bool greaterDegree = (saturation == maxSaturation && degree > maxDegree);
            if (unset || greaterSaturation || greaterDegree)
            {
                max = vertex;
                maxSaturation = saturation;
            }
        }
        return max;
    }

    private static int Saturation(SudokuGraph graph, Vertex source)
    {
        return graph.UnavailableColors(source).Count();
    }
}
