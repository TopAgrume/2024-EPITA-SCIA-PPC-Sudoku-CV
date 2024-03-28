namespace Sudoku.GraphColoring.Solvers;

public class GreedySolver : ISudokuGraphSolver
{
    public void Solve(SudokuGraph graph)
    {
        var source = graph.First(SudokuGraph.Blank);
        if (!source.HasValue)
            return;
        GreedySolver.SolveRecursive(graph, source.Value);
    }

    private static bool SolveRecursive(SudokuGraph graph, int source)
    {
        foreach (var color in graph.AvailableColors(source))
        {
            graph[source] = color;
            var next = graph.First(SudokuGraph.Blank);
            if (!next.HasValue || GreedySolver.SolveRecursive(graph, next.Value))
                return true;
        }
        graph[source] = SudokuGraph.Blank;
        return false;
    }
}
