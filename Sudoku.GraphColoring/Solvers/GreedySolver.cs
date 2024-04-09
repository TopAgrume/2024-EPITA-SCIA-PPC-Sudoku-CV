namespace Sudoku.GraphColoring.Solvers;

using Vertex = int;
using Color = int;

public class GreedySolver : ISudokuGraphSolver
{
    public void Solve(SudokuGraph graph)
    {
        Vertex? source = graph.First(SudokuGraph.Blank);
        if (!source.HasValue)
            return;
        GreedySolver.SolveRecursive(graph, source.Value);
    }

    private static bool SolveRecursive(SudokuGraph graph, Vertex source)
    {
        foreach (Color color in graph.AvailableColors(source))
        {
            graph[source] = color;
            Vertex? next = graph.First(SudokuGraph.Blank);
            if (!next.HasValue || GreedySolver.SolveRecursive(graph, next.Value))
                return true;
        }
        graph[source] = SudokuGraph.Blank;
        return false;
    }
}
