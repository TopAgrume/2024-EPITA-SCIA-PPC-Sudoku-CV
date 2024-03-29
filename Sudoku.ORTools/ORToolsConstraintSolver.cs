using Sudoku.Shared;
using Google.OrTools.ConstraintSolver;
using System.Text;

namespace Sudoku.ORTools
{
    public class ORToolsConstraintSolver : ISudokuSolver
    {
        /// <summary>
        /// Solves the given Sudoku grid using a backtracking algorithm.
        /// </summary>
        /// <param name="s">The Sudoku grid to be solved.</param>
        /// <returns>
        /// The solved Sudoku grid.
        /// </returns>
        public SudokuGrid Solve(SudokuGrid s)
        {
            int[,] grid = s.Cells;
            int gridSize = 9;
            int regionSize = 3;

            Solver solver = new Solver("Sudoku");

            IntVar[,] x = solver.MakeIntVarMatrix(gridSize, gridSize, 1, gridSize, "x");

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != 0)
                    {
                        solver.Add(x[i, j] == grid[i, j]);
                    }
                }
            }

            for (int i = 0; i < gridSize; i++)
            {
                solver.Add(solver.MakeAllDifferent((from j in Enumerable.Range(0, gridSize) select x[i, j]).ToArray()));
                solver.Add(solver.MakeAllDifferent((from j in Enumerable.Range(0, gridSize) select x[j, i]).ToArray()));
            }

            for (int row = 0; row < gridSize; row += regionSize)
            {
                for (int col = 0; col < gridSize; col += regionSize)
                {
                    IntVar[] regionVars = new IntVar[regionSize * regionSize];
                    for (int r = 0; r < regionSize; r++)
                    {
                        for (int c = 0; c < regionSize; c++)
                        {
                            regionVars[r * regionSize + c] = x[row + r, col + c];
                        }
                    }
                    solver.Add(solver.MakeAllDifferent(regionVars));
                }
            }

            DecisionBuilder db = solver.MakePhase(x.Flatten(), Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);

            solver.NewSearch(db);

            while (solver.NextSolution())
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        sb.Append((int)x[i, j].Value());
                    }
                }
                string solvedString = sb.ToString();

                solver.EndSearch();

                return SudokuGrid.ReadSudoku(solvedString);
            }

            throw new Exception("Unfeasible Sudoku");
        }
    }
}
