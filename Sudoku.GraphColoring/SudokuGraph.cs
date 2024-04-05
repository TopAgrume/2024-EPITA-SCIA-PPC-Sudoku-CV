using Sudoku.Shared;

namespace Sudoku.GraphColoring;

using Vertex = int;
using Color = int;

public class SudokuGraph
{
    public static readonly Color Blank = 0;

    public int GridSize { get; }
    public int GridLength { get; }
    private readonly int _gridSquareLength;

    private readonly UndirectedGraph _graph;

    private readonly Color[] _colors;

    public SudokuGraph(SudokuGrid grid)
    {
        this.GridSize = grid.Cells.Length;
        this.GridLength = (int) Math.Sqrt(this.GridSize);
        this._gridSquareLength = (int) Math.Sqrt(this.GridLength);

        this._graph = new UndirectedGraph(this.GridSize);
        this._colors = new Color[this._graph.VertexCount];

        this.InitializeEdges();
        this.InitializeColors(grid.Cells);
    }

    public Color this[Vertex vertex]
    {
        get => this._colors[vertex];
        set => this._colors[vertex] = value;
    }

    public Vertex? First(Vertex target)
    {
        for (Vertex vertex = 0; vertex < this._graph.VertexCount; ++vertex)
            if (this[vertex] == target)
                return vertex;
        return null;
    }

    public IEnumerable<Color> AvailableColors(Vertex source)
    {
        var colors = Enumerable.Range(1, this.GridLength);
        return colors.Except(this.UnavailableColors(source));
    }

    public IEnumerable<Color> UnavailableColors(Vertex source)
    {
        return this.Neighbors(source)
            .Select(vertex => this[vertex])
            .Where(color => color != SudokuGraph.Blank)
            .Distinct();
    }

    public int Degree(Vertex source)
    {
        return this._graph.Degree(source);
    }

    public IEnumerable<Vertex> Neighbors(Vertex source)
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
        this.InitializeEdgesBase();
        this.InitializeEdgesHints();
    }

    private void InitializeColors(int[,] cells)
    {
        for (int row = 0; row < this.GridLength; ++row)
            for (int column = 0; column < this.GridLength; ++column)
                this._colors[this.ToVertex(row, column)] = cells[row, column];
    }

    private bool ShouldConnect(Vertex source, Vertex destination)
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

    private void InitializeEdgesBase()
    {
        for (Vertex source = 0; source < this._graph.VertexCount - 1; ++source)
            for (Vertex destination = source + 1; destination < this._graph.VertexCount; ++destination)
                if (this.ShouldConnect(source, destination))
                    this._graph.AddEdge(source, destination);
    }

    private void InitializeEdgesHints()
    {
        var hints = new HashSet<Vertex>[this.GridLength];
        for (Color color = 0; color < hints.Length; ++color)
            hints[color] = new HashSet<Vertex>();
        for (Vertex vertex = 0; vertex < this.GridSize; ++vertex)
        {
            var color = this[vertex];
            if (color != SudokuGraph.Blank)
                hints[color].Add(vertex);
        }
        for (Color color = 0; color < hints.Length; ++color)
        {
            for (Vertex source = 0; source < hints[color].Count; ++source)
            {
                for (Vertex destination = 0; destination < hints[color].Count; ++destination)
                {
                    if (source == destination)
                        continue;
                    this._graph.ConnectNeighbors(source, destination);
                }
            }
        }
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
        Vertex vertex = this.ToVertex(row, column);
        writer.WriteLine($"\t{vertex} [pos=\"{column},{this.GridLength - row - 1}!\"]");
    }

    private Vertex ToVertex(int row, int column)
    {
        return row * this.GridLength + column;
    }
}
