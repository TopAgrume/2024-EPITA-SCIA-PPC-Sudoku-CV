using Sudoku.Shared;
using DlxLib;

namespace Sudoku.DancingLinks;

public class DancingLinksDotNetBase : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
        {
            // Solve using parallel programming and DlxLib
            int[,] matrix = GenerateDlxMat(s);
            IEnumerable<Solution> solutions = new Dlx().Solve(matrix);
            return ConvertToSudokuGrid(solutions.First(), matrix);
        }
        
        private int[,] GenerateDlxMat(SudokuGrid inputSudoku)
        {
            int[,] dlxMatrix = new int[729, 324];
            // Pourquoi 729 : 9 * 9 (=81) cases, et chacune des cases peut avoir 9 chiffres => 9 * 9 * 9 = 81 * 9 = 729 
            // Pourquoi 324 : chaque case (9 * 9 = 81) est soumise à 4 contraintes : case, box, row, column => 4 * 81 = 324

            Parallel.For(0, 9, i =>
            {
                Parallel.For(0, 9, j =>
                {
                    int cellValue = inputSudoku.Cells[i, j];
                    if (cellValue != 0)
                    {
                        int rowIndex = i + j * 9;
                        int colIndex1 = 81 + j * 9 + cellValue - 1;
                        int colIndex2 = 81 * 2 + i * 9 + cellValue - 1;
                        int colIndex3 = 81 * 3 + ((i / 3) + (j / 3) * 3) * 9 + cellValue - 1;

                        dlxMatrix[rowIndex * 9, rowIndex] = 1;
                        dlxMatrix[rowIndex * 9, colIndex1] = 1;
                        dlxMatrix[rowIndex * 9, colIndex2] = 1;
                        dlxMatrix[rowIndex * 9, colIndex3] = 1;
                    }
                    else
                    {
                        for (int val = 1; val <= 9; val++)
                        {
                            int rowIndex = i + j * 9;
                            int colIndex1 = 81 + j * 9 + val - 1;
                            int colIndex2 = 81 * 2 + i * 9 + val - 1;
                            int colIndex3 = 81 * 3 + ((i / 3) + (j / 3) * 3) * 9 + val - 1;

                            dlxMatrix[rowIndex * 9 + val - 1, rowIndex] = 1;
                            dlxMatrix[rowIndex * 9 + val - 1, colIndex1] = 1;
                            dlxMatrix[rowIndex * 9 + val - 1, colIndex2] = 1;
                            dlxMatrix[rowIndex * 9 + val - 1, colIndex3] = 1;
                        }
                    }
                });
            });
            
            return dlxMatrix;
        }
        
        private SudokuGrid ConvertToSudokuGrid(Solution solution, int[,] inputMatrix)
        {
            var sudokuGrid = new SudokuGrid();

            Parallel.ForEach(solution.RowIndexes, row =>
            {
                int? cellId = Enumerable.Range(0, inputMatrix.GetLength(1))
                    .FirstOrDefault(index => inputMatrix[row, index] == 1, -1);
                
                int? cellValue = Enumerable.Range(81, 162)
                    .FirstOrDefault(index => inputMatrix[row, index] == 1, -1);
                cellValue %= 9;
                
                sudokuGrid.Cells[(int)(cellId % 9), (int)(cellId / 9)] = (int)(cellValue + 1);
            });

            return sudokuGrid;
        }
}