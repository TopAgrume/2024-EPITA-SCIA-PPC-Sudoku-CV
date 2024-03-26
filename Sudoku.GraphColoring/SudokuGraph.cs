using Sudoku.Shared;

namespace Sudoku.GraphColoring;

public class SudokuGraph
{
    public static readonly int Blank = 0;

    public int GridSize { get; }
    public int GridLength { get; }
    private readonly int _gridSquareLength;

    private readonly UndirectedGraph _graph;
    private readonly int[] _colors;

    public SudokuGraph(SudokuGrid grid)
    {
        this.GridSize = grid.Cells.Length;
        this.GridLength = (int) Math.Sqrt(this.GridSize);
        this._gridSquareLength = (int) Math.Sqrt(this.GridLength);

        this._graph = new UndirectedGraph(this.GridSize);
        this._colors = new int[this._graph.VertexCount];

        this.InitializeEdges();
        this.InitializeColors(grid.Cells);
    }

    public int this[int vertex]
    {
        get => this._colors[vertex];
        set => this._colors[vertex] = value;
    }

    public int? First(int target)
    {
        for (int vertex = 0; vertex < this._graph.VertexCount; ++vertex)
            if (this[vertex] == target)
                return vertex;
        return null;
    }

    public IEnumerable<int> Neighbors(int source)
    {
        return this._graph.Neighbors(source);
    }

    public void Dump(string path)
    {
        using (var writer = new StreamWriter(path))
        {
            writer.WriteLine("graph {");
            this.DumpPositions(writer);
            this.DumpEdges(writer);
            writer.WriteLine("}");
        }
    }

    public SudokuGrid ToGrid()
    {
        var grid = new SudokuGrid();
        for (int row = 0; row < this.GridLength; ++row)
            for (int column = 0; column < this.GridLength; ++column)
                grid.Cells[row, column] = this[this.ToVertex(row, column)];
        return grid;
    }

    private void InitializeEdges()
    {
        foreach (var (source, destination) in this._graph.VertexPairs())
            if (this.ShouldConnect(source, destination))
                this._graph.AddEdge(source, destination);
    }

    private void InitializeColors(int[,] cells)
    {
        for (int row = 0; row < this.GridLength; ++row)
            for (int column = 0; column < this.GridLength; ++column)
                this._colors[this.ToVertex(row, column)] = cells[row, column];
    }

    private bool ShouldConnect(int source, int destination)
    {
        int sourceRow = source / this.GridLength;
        int sourceColumn = source % this.GridLength;
        var sourceSquare = (sourceRow / this._gridSquareLength, sourceColumn / this._gridSquareLength);

        int destinationRow = destination / this.GridLength;
        int destinationColumn = destination % this.GridLength;
        var destinationSquare = (destinationRow / this._gridSquareLength, destinationColumn / this._gridSquareLength);

        bool sameRow = (sourceRow == destinationRow);
        bool sameColumn = (sourceColumn == destinationColumn);
        bool sameSquare = (sourceSquare == destinationSquare);

        return sameRow || sameColumn || sameSquare;
    }

    private void DumpPositions(TextWriter writer)
    {
        for (int row = 0; row < this.GridLength; ++row)
            for (int column = 0; column < this.GridLength; ++column)
                this.DumpPosition(writer, row, column);
    }

    private void DumpEdges(TextWriter writer)
    {
        foreach (var (source, destination) in this._graph.Edges())
            writer.WriteLine($"\t{source} -- {destination}");
    }

    private void DumpPosition(TextWriter writer, int row, int column)
    {
        int vertex = this.ToVertex(row, column);
        writer.WriteLine($"\t{vertex} [pos=\"{column},{this.GridLength - row - 1}!\"]");
    }

    private int ToVertex(int row, int column)
    {
        return row * this.GridLength + column;
    }
}
