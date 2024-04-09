using Sudoku.Shared;
using Google.OrTools.LinearSolver;

namespace Sudoku.ORTools
{
    public class ORT_Linear_CBC_MIP : ISudokuSolver
    {
        // Init the sudoku data
        private const int gridSize = 9;
        private const int regionSize = 3;

        /// <summary>
        /// Solves the given Sudoku grid using a LinearSolver algorithm.
        /// </summary>
        /// <param name="s">The Sudoku grid to be solved.</param>
        /// <returns>
        /// The solved Sudoku grid.
        /// </returns>
        /// <exception cref="Exception">Thrown when no feasible solution exists for the given Sudoku grid.</exception>
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Extract the initial grid
            int[,] grid = s.Cells;

            // Create a Linear Solver instance
            Solver solver = new Solver("SudokuSolver", Solver.OptimizationProblemType.CBC_MIXED_INTEGER_PROGRAMMING);

            // Setup decision variables and constraints
            var tensor = SetupTensor(solver, grid);
            CreateConstraints(solver, tensor, grid);

            Solver.ResultStatus status = solver.Solve();
            if (status != Solver.ResultStatus.OPTIMAL)
            {
                // If the Sudoku is unsolvable, throw an exception
                throw new Exception("Unfeasible Sudoku");
            }
            
            UpdateSudokuGrid(tensor, grid, s);
            return s;
        }

        private static Variable[,,] SetupTensor(Solver solver, int[,] grid)
        {
            // Create solver variables tensor with constraints for size
            Variable[,,] tensor = new Variable[gridSize, gridSize, gridSize];
            
            // Iterate through the grid to create variables and constant constraints
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != 0)
                    {
                        tensor[i, j, grid[i, j] - 1] = solver.MakeIntVar(1, 1, $"tensor[{i},{j},{grid[i, j] - 1}]");
                        continue;
                    }
                    
                    for (int k = 0; k < gridSize; k++)
                    {
                        tensor[i, j, k] = solver.MakeIntVar(0, 1, $"tensor[{i},{j},{k}]");
                    }
                }
            }

            return tensor;
        }

        private static void CreateConstraints(Solver solver, Variable[,,] tensor, int[,] grid)
        {
            // Add exactly one value per cell
            // Add constraints for each row and column
            for (int i = 0; i < gridSize; i++)
            {
                for (int k = 0; k < gridSize; k++)
                {
                    if (grid[i, k] == 0)
                    {
                        Constraint oneValuePerCell = solver.MakeConstraint(1, 1, "");
                        for (int j = 0; j < gridSize; j++)
                        {
                            oneValuePerCell.SetCoefficient(tensor[i, k, j], 1);
                        }
                    }

                    Constraint rowConstraint = solver.MakeConstraint(1, 1, "");
                    Constraint colConstraint = solver.MakeConstraint(1, 1, "");
                    
                    for (int j = 0; j < gridSize; j++)
                    {
                        rowConstraint.SetCoefficient(tensor[i, j, k], 1);
                        colConstraint.SetCoefficient(tensor[j, i, k], 1);
                    }
                }
            }
            
            // Add constraints for each region
            for (int row = 0; row < gridSize; row += regionSize)
            {
                for (int col = 0; col < gridSize; col += regionSize)
                {
                    for (int k = 0; k < gridSize; k++)
                    {
                        Constraint subgridConstraint = solver.MakeConstraint(1, 1, "");
                        for (int r = 0; r < regionSize; r++)
                        {
                            for (int c = 0; c < regionSize; c++)
                            {
                                subgridConstraint.SetCoefficient(tensor[row + r, col + c, k], 1);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Sudoku grid with the solved values.
        /// </summary>
        private static void UpdateSudokuGrid(Variable[,,] tensor, int[,] grid, SudokuGrid s)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != 0)
                        continue;
                    
                    for (int k = 0; k < gridSize; k++)
                    {
                        if (tensor[i, j, k].SolutionValue() == 1)
                        {
                            s.Cells[i, j] = k + 1;
                            break;
                        }
                    }
                }
            }
        }
    }
}