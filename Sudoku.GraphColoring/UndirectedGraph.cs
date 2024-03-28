namespace Sudoku.GraphColoring;

public class UndirectedGraph
{
    public int VertexCount { get;  }

    private readonly bool[,] _adjacencyMatrix;

    public UndirectedGraph(int vertexCount)
    {
        this.VertexCount = vertexCount;
        this._adjacencyMatrix = new bool[vertexCount, vertexCount];
    }

    public IEnumerable<int> Neighbors(int source)
    {
        foreach (var edge in this.Edges(source))
        {
            if (edge.Item1 == source)
                yield return edge.Item2;
            else if (edge.Item2 == source)
                yield return edge.Item1;
        }
    }

    public void AddEdge(int source, int destination)
    {
        this._adjacencyMatrix[source, destination] = true;
        this._adjacencyMatrix[destination, source] = true;
    }

    public bool HasEdge(int source, int destination)
    {
        return this._adjacencyMatrix[source, destination];
    }

    public IEnumerable<(int, int)> Edges()
    {
        foreach (var (source, destination) in this.VertexPairs())
            if (this.HasEdge(source, destination))
                yield return (source, destination);
    }

    public IEnumerable<(int, int)> Edges(int source)
    {
        for (int destination = 0; destination < this.VertexCount; ++destination)
        {
            if (destination == source)
                continue;
            if (this.HasEdge(source, destination))
                yield return (Math.Min(source, destination), Math.Max(source, destination));
        }
    }

    public IEnumerable<(int, int)> VertexPairs()
    {
        for (int source = 0; source < this.VertexCount - 1; ++source)
            for (int destination = source + 1; destination < this.VertexCount; ++destination)
                yield return (source, destination);
    }
}
