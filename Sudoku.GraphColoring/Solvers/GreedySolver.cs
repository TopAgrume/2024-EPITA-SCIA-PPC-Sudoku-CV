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
        var neighbors = graph.Neighbors(source);
        foreach (var color in GreedySolver.AvailableColors(graph, neighbors))
        {
            graph[source] = color;
            var next = graph.First(SudokuGraph.Blank);
            if (!next.HasValue || GreedySolver.SolveRecursive(graph, next.Value))
                return true;
        }
        graph[source] = SudokuGraph.Blank;
        return false;
    }

    private static IEnumerable<int> AvailableColors(SudokuGraph graph, IEnumerable<int> vertices)
    {
        var colors = Enumerable.Range(1, graph.GridLength);
        var colorsUsed = vertices.Select(vertex => graph[vertex]);
        return colors.Except(colorsUsed);
    }
}
