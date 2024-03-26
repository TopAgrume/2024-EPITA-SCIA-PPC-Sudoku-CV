using Sudoku.GraphColoring.Solvers;
using Sudoku.Shared;

namespace Sudoku.GraphColoring;

public class GraphColoringDotNetSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid grid)
    {
        var graph = new SudokuGraph(grid);
        ISudokuGraphSolver solver = new GreedySolver();
        solver.Solve(graph);
        return graph.ToGrid();
    }
}
