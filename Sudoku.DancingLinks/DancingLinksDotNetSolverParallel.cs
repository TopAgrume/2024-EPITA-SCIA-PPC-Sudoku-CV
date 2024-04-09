using System.Collections.Concurrent;
using Sudoku.Shared;

namespace Sudoku.DancingLinks
{
    public class DancingLinksSolverParallel
    {
        private readonly ColumnNode head;
        private readonly SudokuGrid grid;
        private readonly int size;
        private ColumnNode[] columns;
        private readonly Stack<Node> solution = new Stack<Node>();

        public DancingLinksSolverParallel(SudokuGrid grid)
        {
            this.grid = grid;
            this.size = grid.Cells.GetLength(0);
            this.head = new ColumnNode();

            InitializeColumnNodes();
            InitializeMatrix();
        }

        /// <summary>
        /// Represents a node in the matrix for the Dancing Links algorithm.
        /// </summary>
        private class Node
        {
            public Node Left, Right, Up, Down;
            public ColumnNode Column;
            public int RowIndex; // Index representing the position of the node in the grid.

            public Node()
            {
                Left = this;
                Right = this;
                Up = this;
                Down = this;
            }
        }

        /// <summary>
        /// Represents a column node in the matrix.
        /// </summary>
        private class ColumnNode : Node
        {
            public int Size; // Number of nodes in the column.
            
            public ColumnNode()
            {
                this.Size = 0;
            }
        }

        /// <summary>
        /// Initializes the column nodes.
        /// </summary>
        private void InitializeColumnNodes()
        {
            var columnNodes = new ColumnNode[size * size * 4]; // 4 constraints for each cell.
            for (int col = 0; col < size * size * 4; col++)
            {
                var columnNode = new ColumnNode();
                columnNode.Right = head;
                columnNode.Left = head.Left;
                head.Left.Right = columnNode;
                head.Left = columnNode;
                columnNodes[col] = columnNode;
            }

            columns = columnNodes;
        }

        /// <summary>
        /// Initializes the matrix from the given Sudoku grid.
        /// </summary>
        private void InitializeMatrix()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int num = grid.Cells[x, y];
                    if (num == 0)
                    {
                        for (int n = 0; n < size; n++)
                        {
                            AddNode(x, y, n);
                        }
                    }
                    else
                    {
                        AddNode(x, y, num - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a node to the matrix representing a possible value for a cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="num"></param>
        private void AddNode(int x, int y, int num)
        {
            // Calculate the constraint indices
            var posConstraint = x * size + y;
            var rowConstraint = size * size + x * size + num;
            var colConstraint = 2 * size * size + y * size + num;
            var boxConstraint = 3 * size * size + (x / 3 * 3 + y / 3) * size + num;

            Node pos = new Node { Column = columns[posConstraint], RowIndex = x * size * size + y * size + num };
            Node row = new Node { Column = columns[rowConstraint], RowIndex = x * size * size + y * size + num };
            Node col = new Node { Column = columns[colConstraint], RowIndex = x * size * size + y * size + num };
            Node box = new Node { Column = columns[boxConstraint], RowIndex = x * size * size + y * size + num };

            LinkRows(pos, row, col, box);
            LinkNodes(pos);
            LinkNodes(row);
            LinkNodes(col);
            LinkNodes(box);
        }

        /// <summary>
        /// Links the rows of the nodes.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        private void LinkRows(Node a, Node b, Node c, Node d)
        {
            a.Right = b;
            b.Right = c;
            c.Right = d;
            d.Right = a;
            a.Left = d;
            d.Left = c;
            c.Left = b;
            b.Left = a;
        }

        /// <summary>
        /// Links the nodes vertically in the matrix.
        /// </summary>
        /// <param name="node"></param>
        private void LinkNodes(Node node)
        {
            ColumnNode c = node.Column;
            c.Size++;
            node.Down = c;
            node.Up = c.Up;
            c.Up.Down = node;
            c.Up = node;
        }

        /// <summary>
        /// Covers the column specified.
        /// </summary>
        /// <param name="column"></param>
        private void Cover(ColumnNode column)
        {
            column.Right.Left = column.Left;
            column.Left.Right = column.Right;
            for (Node i = column.Down; i != column; i = i.Down)
            {
                for (Node j = i.Right; j != i; j = j.Right)
                {
                    j.Down.Up = j.Up;
                    j.Up.Down = j.Down;
                    j.Column.Size--;
                }
            }
        }

        /// <summary>
        /// Uncovers the column specified.
        /// </summary>
        /// <param name="column"></param>
        private void Uncover(ColumnNode column)
        {
            for (Node i = column.Up; i != column; i = i.Up)
            {
                for (Node j = i.Left; j != i; j = j.Left)
                {
                    j.Down.Up = j;
                    j.Up.Down = j;
                    j.Column.Size++;
                }
            }

            column.Right.Left = column;
            column.Left.Right = column;
        }

        /// <summary>
        /// Chooses a column node.
        /// </summary>
        /// <returns></returns>
        private ColumnNode ChooseColumnNode()
        {
            ColumnNode chosenColumn = null;
            int size = int.MaxValue;
            ColumnNode current = (ColumnNode)head.Right;
            while (current != head)
            {
                if (current.Size < size)
                {
                    size = current.Size;
                    chosenColumn = current;
                }

                current = (ColumnNode)current.Right;
            }

            return chosenColumn;
        }

        /// <summary>
        /// Searches for a solution recursively using backtracking.
        /// </summary>
        /// <returns></returns>
        public bool Search()
        {
            if (head.Right == head)
            {
                return true;
            }

            var column = ChooseColumnNode();
            Cover(column);

            for (Node r = column.Down; r != column; r = r.Down)
            {
                solution.Push(r);

                for (Node j = r.Right; j != r; j = j.Right)
                {
                    Cover(j.Column);
                }

                if (Search())
                {
                    return true;
                }

                r = solution.Pop();
                column = r.Column;

                for (Node j = r.Left; j != r; j = j.Left)
                {
                    Uncover(j.Column);
                }
            }

            Uncover(column);
            return false;
        }

        /// <summary>
        /// Gets the row nodes.
        /// </summary>
        /// <param name="columnNode"></param>
        /// <returns></returns>
        private List<Node> GetRowNodes(ColumnNode columnNode)
        {
            List<Node> rowNodes = new List<Node>();
            for (Node i = columnNode.Down; i != columnNode; i = i.Down)
            {
                rowNodes.Add(i);
            }

            return rowNodes;
        }
        
        public bool ParallelSearch()
        {
            if (head.Right == head)
            {
                return true;
            }

            var column = ChooseColumnNode();
            Cover(column);

            bool solutionFound = false;

            // Use a ConcurrentStack to store partial solutions
            ConcurrentStack<Node> partialSolutions = new ConcurrentStack<Node>();

            Parallel.ForEach(GetRowNodes(column), (r, state) =>
            {
                
                Stack<Node> localSolution = new Stack<Node>();
                localSolution.Push(r);
                
                for (Node j = r.Right; j != r; j = j.Right)
                {
                    Cover(j.Column);
                }
                
                if (ParallelSearchRecursive(localSolution))
                {
                    solutionFound = true;
                    state.Stop(); 
                }
                
                if (!solutionFound)
                {
                    foreach (Node j in localSolution)
                    {
                        Uncover(j.Column);
                    }
                }
            });

            if (!solutionFound)
            {
                Uncover(column);
            }

            return solutionFound;
        }
        
        private bool ParallelSearchRecursive(Stack<Node> localSolution)
        {
           
            if (head.Right == head)
            {
                lock (solution)
                {
                    // Update the global solution with the local solution
                    while (localSolution.Count > 0)
                    {
                        solution.Push(localSolution.Pop());
                    }
                }
                return true;
            }
            
            var column = ChooseColumnNode();
            
            Cover(column);

            bool solutionFound = false;
            
            foreach (Node r in GetRowNodes(column))
            {
                
                if (!localSolution.Contains(r))
                {
                    
                    localSolution.Push(r);
                    
                    for (Node j = r.Right; j != r; j = j.Right)
                    {
                        Cover(j.Column);
                    }
                    
                    solutionFound = ParallelSearchRecursive(localSolution);
                    
                    if (solutionFound)
                    {
                        break;
                    }
                    
                    localSolution.Pop();
                    
                    for (Node j = r.Left; j != r; j = j.Left)
                    {
                        Uncover(j.Column);
                    }
                }
            }
            
            Uncover(column);

            return solutionFound;
        }

        
        /// <summary>
        /// Searches for a solution with parallel programming.
        /// </summary>
        /// <returns></returns>
        public bool ParallelSearchFVersion()
        {
            if (head.Right == head)
            {
                return true;
            }

            var column = ChooseColumnNode();
            Cover(column);

            bool solutionFound = false;

            Parallel.ForEach(GetRowNodes(column), (r, state) =>
            {
                lock (solution)
                {
                    solution.Push(r);
                }

                for (Node j = r.Right; j != r; j = j.Right)
                {
                    Cover(j.Column);
                }

                if (!solutionFound && ParallelSearch())
                {
                    solutionFound = true;
                    state.Stop();
                }

                if (!solutionFound)
                {
                    lock (solution)
                    {
                        solution.Pop();
                    }

                    column = r.Column;
                    for (Node j = r.Left; j != r; j = j.Left)
                    {
                        Uncover(j.Column);
                    }
                }
            });

            if (!solutionFound)
            {
                Uncover(column);
            }

            return solutionFound;
        }
        // <summary>
        /// Gets the solved grid.
        /// </summary>
        /// <returns>
        /// The solved sudoku grid.
        /// </returns>
        public SudokuGrid GetSolvedGrid()
        {
            foreach (var node in solution)
            {
                int idx = node.RowIndex;
                int row = idx / (size * size);
                int col = idx % (size * size) / size;
                int num = idx % (size * size) % size + 1;
                grid.Cells[row, col] = num;
            }

            return grid;
        }
    }

    public class DancingLinksDotNetSolverParallel : ISudokuSolver
    {
        /// <summary>
        /// Solves the given Sudoku grid using a dancing links algorithm with parallel programming.
        /// </summary>
        /// <param name="s">The Sudoku grid to be solved.</param>
        /// <returns>
        /// The solved Sudoku grid.
        /// </returns>
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Solve using dancing links solver
            DancingLinksSolverParallel dancingLinksSolver = new DancingLinksSolverParallel(s);
            // Solve with parallel programming (does not work for hard sudokus)
            dancingLinksSolver.ParallelSearch();
            return dancingLinksSolver.GetSolvedGrid();
        }
    }
}

