using Sudoku.Shared;
using Google.OrTools.ConstraintSolver;
using System.Text;

namespace Sudoku.ORTools
{
    public class ORToolsConstraintSolver : ISudokuSolver
    {
        /// <summary>
        /// Solves the given Sudoku grid using constraint solver.
        /// </summary>
        /// <param name="s">The Sudoku grid to be solved.</param>
        /// <returns>
        /// The solved Sudoku grid if a solution is found.
        /// </returns>
        /// <exception cref="Exception">Thrown when no feasible solution exists for the given Sudoku grid.</exception>
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Extract the initial grid and necessary constants.
            int[,] grid = s.Cells;
            int gridSize = 9;
            int regionSize = 3;

            // Create a ConstraintSolver instance
            Solver solver = new Solver("Sudoku");

            // Create solver matrix with constraints for size and integers range
            IntVar[,] x = solver.MakeIntVarMatrix(gridSize, gridSize, 1, 9, "x");

            // Add constraints
            CreateConstraints(solver, grid, x, gridSize, regionSize);

            // Define the decision builder to use for the solver
            DecisionBuilder db = solver.MakePhase(x.Flatten(), Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);
            // Alternative decision builder for experimentation
            // solver.MakePhase(x.Flatten(), Solver.CHOOSE_FIRST_UNBOUND, Solver.ASSIGN_MIN_VALUE);
            
            // Start the search
            solver.NewSearch(db);

            // Iterate through solutions
            while (solver.NextSolution())
            {
                // Build the solved Sudoku string
                string solvedString = BuildSolvedString(x, gridSize);
                solver.EndSearch();

                return SudokuGrid.ReadSudoku(solvedString);
            }

            // If the Sudoku is unsolvable, throw an exception
            throw new Exception("Unfeasible Sudoku");
        }

        private static void CreateConstraints(Solver solver, int[,] grid, IntVar[,] x, int gridSize, int regionSize)
        {
            // Add constraints for pre-filled cells
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

            // Add constraints for rows and columns to have distinct values
            for (int i = 0; i < gridSize; i++)
            {
                solver.Add(solver.MakeAllDifferent((from j in Enumerable.Range(0, gridSize) select x[i, j]).ToArray()));
                solver.Add(solver.MakeAllDifferent((from j in Enumerable.Range(0, gridSize) select x[j, i]).ToArray()));
            }

            // Add constraints for each region to have distinct values
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
        }

        private static string BuildSolvedString(IntVar[,] x, int gridSize)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    sb.Append((int)x[i, j].Value());
                }
            }
            return sb.ToString();
        }
    }
}
