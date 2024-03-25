namespace Sudoku.GraphColoring;

public class SudokuGraph
{
    private const int SudokuGridSquareLength = 3;
    private const int SudokuGridLength = SudokuGraph.SudokuGridSquareLength * SudokuGraph.SudokuGridSquareLength;
    private const int SudokuGridSize = SudokuGraph.SudokuGridLength * SudokuGraph.SudokuGridLength;

    private readonly UndirectedGraph _graph;

    public SudokuGraph()
    {
        this._graph = new UndirectedGraph(SudokuGridSize);
        this.InitializeEdges();
    }

    private static int ToVertex(int row, int column)
    {
        return row * SudokuGraph.SudokuGridLength + column;
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

    private void InitializeEdges()
    {
        foreach (var (source, destination) in this._graph.VertexPairs())
            if (this.ShouldConnect(source, destination))
                this._graph.AddEdge(source, destination);
    }

    private bool ShouldConnect(int source, int destination)
    {
        int sourceRow = source / SudokuGraph.SudokuGridLength;
        int sourceColumn = source % SudokuGraph.SudokuGridLength;
        var sourceSquare = (sourceRow / SudokuGraph.SudokuGridSquareLength, sourceColumn / SudokuGraph.SudokuGridSquareLength);

        int destinationRow = destination / SudokuGraph.SudokuGridLength;
        int destinationColumn = destination % SudokuGraph.SudokuGridLength;
        var destinationSquare = (destinationRow / SudokuGraph.SudokuGridSquareLength, destinationColumn / SudokuGraph.SudokuGridSquareLength);

        bool sameRow = (sourceRow == destinationRow);
        bool sameColumn = (sourceColumn == destinationColumn);
        bool sameSquare = (sourceSquare == destinationSquare);

        return sameRow || sameColumn || sameSquare;
    }

    private void DumpPositions(TextWriter writer)
    {
        for (int row = 0; row < SudokuGraph.SudokuGridLength; ++row)
            for (int column = 0; column < SudokuGraph.SudokuGridLength; ++column)
                this.DumpPosition(writer, row, column);
    }

    private void DumpEdges(TextWriter writer)
    {
        foreach (var (source, destination) in this._graph.Edges())
            writer.WriteLine($"\t{source} -- {destination}");
    }

    private void DumpPosition(TextWriter writer, int row, int column)
    {
        int vertex = SudokuGraph.ToVertex(row, column);
        writer.WriteLine($"\t{vertex} [pos=\"{column},{SudokuGraph.SudokuGridLength - row - 1}!\"]");
    }
}
