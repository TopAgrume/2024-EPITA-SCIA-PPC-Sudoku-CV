using Sudoku.Shared;
using Google.OrTools.Sat;
using System.Text;

namespace Sudoku.ORTools
{
	public class ORT_CPSAT_Solver : ISudokuSolver
	{
        // Init the sudoku data
        private const int gridSize = 9;
        private const int regionSize = 3;

		/// <summary>
        /// Solves the given Sudoku grid using Constraint Programming (CP) -> SAT solver.
        /// </summary>
        /// <param name="s">The Sudoku grid to be solved.</param>
        /// <returns>
        /// The solved Sudoku grid if a solution is found.
        /// </returns>
        /// <exception cref="Exception">Thrown when no feasible solution exists for the given Sudoku grid.</exception>
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Extract the initial grid
            int[,] grid = s.Cells;

            // Create a Constraint Programming Model (SAT) instance.
            CpModel model = new CpModel();

            // Initialize variables
            IntVar[,] matrix = InitVariables(model, grid);

            // Add constraints
            CreateConstraints(model, matrix);
            
            // Create a solver instance and solve the model
            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);

            // If the solution is optimal or feasible, build the solved Sudoku grid
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                string solvedString = BuildSolvedString(solver, matrix);
                return SudokuGrid.ReadSudoku(solvedString);
            }

            // If the Sudoku is unsolvable, throw an exception
            throw new Exception("Unfeasible Sudoku");
        }

        private static IntVar[,] InitVariables(CpModel model, int[,] grid)
        {
            // Create solver matrix with constraints for size
            IntVar[,] matrix = new IntVar[gridSize, gridSize];

            // Iterate through the grid to create variables and constant constraints
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] == 0)
                    {
                        matrix[i, j] = model.NewIntVar(1, gridSize, $"matrix[{i},{j}]");
                    }
                    else
                    {
                        matrix[i, j] = model.NewConstant(grid[i, j]);
                    }
                }
            }

            return matrix;
        }

        private static void CreateConstraints(CpModel model, IntVar[,] matrix)
        {
            // Add row and column constraints ensuring each number appears only once
            for (int i = 0; i < gridSize; i++)
            {
                model.AddAllDifferent((from j in Enumerable.Range(0, gridSize) select matrix[i, j]).ToArray());
                model.AddAllDifferent((from j in Enumerable.Range(0, gridSize) select matrix[j, i]).ToArray());
            }

            // Add region constraints ensuring each number appears only once in each sub-grid
            for (int row = 0; row < gridSize; row += regionSize)
            {
                for (int col = 0; col < gridSize; col += regionSize)
                {
                    var regionVars = Enumerable.Range(0, regionSize)
                                    .SelectMany(i => Enumerable.Range(0, regionSize)
                                    .Select(j => matrix[row + i, col + j]))
                                    .ToArray();

                    model.AddAllDifferent(regionVars);
                }
            }
        }

        /// <summary>
        /// Builds the Sudoku grid with the solved values.
        /// </summary>
        private static string BuildSolvedString(CpSolver solver, IntVar[,] matrix)
        {
            // Create String from solver value
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    sb.Append((int)solver.Value(matrix[i, j]));
                }
            }
            return sb.ToString();
        }
    }
}
