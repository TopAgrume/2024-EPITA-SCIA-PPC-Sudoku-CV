using Sudoku.Shared;
using Google.OrTools.Sat;
using System.Text;

namespace Sudoku.ORTools
{
	public class OrToolsSatSolver : ISudokuSolver
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

            CpModel model = new CpModel();
            IntVar [,] x = new IntVar[gridSize, gridSize];;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] == 0)
                    {
                        x[i, j] = model.NewIntVar(1, gridSize, $"x[{i},{j}]");
                    }
                    else
                    {
                        x[i, j] = model.NewConstant(grid[i, j]);
                    }
                }
            }

            for (int i = 0; i < gridSize; i++)
            {
                model.AddAllDifferent((from j in Enumerable.Range(0, gridSize) select x[i, j]).ToArray());
                model.AddAllDifferent((from j in Enumerable.Range(0, gridSize) select x[j, i]).ToArray());
            }

            for (int row = 0; row < gridSize; row += regionSize)
            {
                for (int col = 0; col < gridSize; col += regionSize)
                {
                    var regionVars = Enumerable.Range(0, regionSize)
                                    .SelectMany(i => Enumerable.Range(0, regionSize)
                                    .Select(j => x[row + i, col + j]))
                                    .ToArray();

                    model.AddAllDifferent(regionVars);
                }
            }

            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        sb.Append((int)solver.Value(x[i, j]));
                    }
                }
                string solvedString = sb.ToString();

                return SudokuGrid.ReadSudoku(solvedString);
            }
            
            throw new Exception("Unfeasible Sudoku");
            
        }
	}
}
