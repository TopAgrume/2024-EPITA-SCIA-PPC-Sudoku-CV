namespace Sudoku.GraphColoring.Solvers;

// https://fr.wikipedia.org/wiki/DSATUR
public class DsaturSolver : ISudokuGraphSolver
{
    public void Solve(SudokuGraph graph)
    {
        DsaturSolver.SolveRecursive(graph);
    }

    private static bool SolveRecursive(SudokuGraph graph)
    {
        var source = DsaturSolver.BlankMaxSaturation(graph);
        if (!source.HasValue)
            return true;

        foreach (var color in graph.AvailableColors(source.Value)) {
            graph[source.Value] = color;
            if (DsaturSolver.SolveRecursive(graph))
                return true;
        }

        graph[source.Value] = SudokuGraph.Blank;
        return false;
    }

    private static int? BlankMaxSaturation(SudokuGraph graph)
    {
        try
        {
            return Enumerable.Range(0, graph.GridSize)
                .Where(vertex => graph[vertex] == SudokuGraph.Blank)
                .MaxBy(vertex => DsaturSolver.Saturation(graph, vertex));
        }
        catch (InvalidOperationException) // Empty sequence
        {
            return null;
        }
    }

    private static int Saturation(SudokuGraph graph, int source)
    {
        return graph.UnavailableColors(source).Count();
    }
}
