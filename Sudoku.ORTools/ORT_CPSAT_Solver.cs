using Sudoku.Shared;
using Google.OrTools.Sat;

namespace Sudoku.ORTools
{
    public class ORT_CPSAT_Solver : ISudokuSolver
    {
        private const int gridSize = 9;
        private const int regionSize = 3;

        public SudokuGrid Solve(SudokuGrid s)
        {
            int[,] grid = s.Cells;

            CpModel model = new CpModel();
            IntVar[,] matrix = InitVariables(model, grid);
            CreateConstraints(model, matrix);

             // Parallelize solving
            CpSolver solver = new CpSolver();
            solver.StringParameters = $"max_time_in_seconds:10.0;num_search_workers:{Environment.ProcessorCount}";

            CpSolverStatus status = solver.Solve(model);

            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                UpdateSudokuGrid(grid, solver, matrix);
                return s;
            }

            // If the Sudoku is unsolvable, throw an exception
            throw new Exception("Unfeasible Sudoku");
        }

        private static IntVar[,] InitVariables(CpModel model, int[,] grid)
        {
            IntVar[,] matrix = new IntVar[gridSize, gridSize];

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
            for (int i = 0; i < gridSize; i++)
            {
                model.AddAllDifferent((from j in Enumerable.Range(0, gridSize) select matrix[i, j]).ToArray());
                model.AddAllDifferent((from j in Enumerable.Range(0, gridSize) select matrix[j, i]).ToArray());
            }

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

        private static void UpdateSudokuGrid(int[,] grid, CpSolver solver, IntVar[,] matrix)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    grid[i, j] = (int)solver.Value(matrix[i, j]);
                }
            }
        }
    }
}
