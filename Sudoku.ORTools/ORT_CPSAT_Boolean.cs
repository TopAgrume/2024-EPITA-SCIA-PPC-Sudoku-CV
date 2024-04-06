using Sudoku.Shared;
using Google.OrTools.Sat;

namespace Sudoku.OrTools
{
    public class ORT_CPSAT_Boolean : ISudokuSolver
    {
        // Init the sudoku data
        private const int gridSize = 9;
        private const int regionSize = 3;

        /// <summary>
        /// Solves the given Sudoku grid using a backtracking algorithm.
        /// </summary>
        /// <param name="s">The Sudoku grid to be solved.</param>
        /// <returns>
        /// The solved Sudoku grid.
        /// </returns>
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Extract the initial grid
            int[,] grid = s.Cells;

            // Create a Constraint Programming Model (SAT) instance.
            CpModel model = new CpModel();

            var tensor = SetupTensor(model, grid);
            CreateConstraints(model, tensor);

            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);

            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                UpdateSudokuGrid(solver, tensor, grid, s);
                return s;
            }

            // If the Sudoku is unsolvable, throw an exception
            throw new Exception("Unfeasible Sudoku");
        }

        private static BoolVar[,,] SetupTensor(CpModel model, int[,] grid)
        {
            // Create solver boolean tensor with constraints for size
            BoolVar[,,] tensor = new BoolVar[gridSize, gridSize, gridSize];
            
            // Iterate through the grid to create booleans and constant constraints
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    // Create binary decision booleans
                    for (int k = 0; k < gridSize; k++)
                    {
                        tensor[i, j, k] = model.NewBoolVar($"tensor{i}{j}{k}");
                    }

                    if (grid[i, j] == 0)
                        continue;

                    // If cell value is not zero, add a constraint that only one variable can be true
                    ILiteral[] boolArray = new ILiteral[gridSize];
                    for (int k = 0; k < gridSize; k++)
                    {
                        boolArray[k] = tensor[i, j, k];
                        
                        if (k == grid[i, j] - 1)
                            continue;

                        // Set other variables in the cell to false
                        boolArray[k] = boolArray[k].Not();
                    }

                    model.AddBoolAnd(boolArray);
                }
            }

            return tensor;
        }

        private static void CreateConstraints(CpModel model, BoolVar[,,] tensor)
        {
            // Add exactly one value per cell
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    // At least one number per cell
                    BoolVar[] boolArray = new BoolVar[gridSize];
                    for (int k = 0; k < gridSize; k++)
                    {
                        boolArray[k] = tensor[i, j, k];
                    }

                    model.AddBoolOr(boolArray);

                    // At most one number per cell
                    for (int k = 0; k < gridSize - 1; k++)
                    {
                        for (int knext = k + 1; knext < gridSize; knext++)
                        {
                            // Constraints: not([i, j, k] and [i, j, knext])
                            model.AddBoolOr([tensor[i, j, k].Not(), tensor[i, j, knext].Not()]);
                        }
                    }
                }
            }

            // Add constraints for each row and column
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize - 1; j++)
                {
                    for (int jnext = j + 1; jnext < gridSize; jnext++)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            // Constraints for rows: not([i, j, k] and [inext, j, k])
                            model.AddBoolOr([tensor[j, i, k].Not(), tensor[jnext, i, k].Not()]);

                            // Constraints for columns: not([i, j, k] and [i, jnext, k])
                            model.AddBoolOr([tensor[i, j, k].Not(), tensor[i, jnext, k].Not()]);
                        }
                    }
                }
            }

            // Add constraints for each region
            for (int i = 0; i < gridSize; i += regionSize)
            {
                for (int j = 0; j < gridSize; j += regionSize)
                {
                    // Navigate through rows and columns in the current region
                    for (int regionRows = 0; regionRows < regionSize; regionRows++)
                    {
                        for (int regionColumns = 0; regionColumns < regionSize; regionColumns++)
                        {
                            // Navigate through unique cell pairs in the current region
                            for (int regionNextRows = regionRows; regionNextRows < regionSize; regionNextRows++)
                            {
                                int nextColumnStart = (regionNextRows == regionRows) ? regionColumns + 1 : 0;
                                for (int regionNextColumns = nextColumnStart; regionNextColumns < regionSize; regionNextColumns++)
                                {   
                                    // First and Second element coordinates
                                    int ifirst = i + regionRows;
                                    int jfirst = j + regionColumns;
                                    int isecond = i + regionNextRows;
                                    int jsecond = j + regionNextColumns;

                                    // Constraints for regions: not([ifirst, jfirst, k] and [isecond, jsecond, k])
                                    for (int k = 0; k < 9; k++)
                                    {
                                        model.AddBoolOr([
                                            tensor[ifirst, jfirst, k].Not(),
                                            tensor[isecond, jsecond, k].Not()
                                        ]); 
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Sudoku grid with the solved values.
        /// </summary>
        private static void UpdateSudokuGrid(CpSolver solver, BoolVar[,,] tensor, int[,] grid, SudokuGrid s)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i, j] != 0)
                        continue;
                    
                    for (int k = 0; k < 9; k++)
                    {
                        if (solver.Value(tensor[i, j, k]) != 0)
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