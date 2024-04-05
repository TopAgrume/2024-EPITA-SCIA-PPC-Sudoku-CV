namespace Sudoku.GraphColoring;

using Vertex = int;

public class UndirectedGraph
{
    public int VertexCount { get;  }

    private readonly HashSet<Vertex>[] _adjacencyLists;

    public UndirectedGraph(int vertexCount)
    {
        this.VertexCount = vertexCount;
        this._adjacencyLists = new HashSet<Vertex>[vertexCount];
        for (Vertex vertex = 0; vertex < vertexCount; ++vertex)
            this._adjacencyLists[vertex] = [];
    }

    public IEnumerable<(Vertex, Vertex)> Edges()
    {
        for (Vertex source = 0; source < this.VertexCount; ++source)
            foreach (Vertex destination in this.Neighbors(source))
                yield return (Math.Min(source, destination), Math.Max(source, destination));
    }

    public int Degree(Vertex source)
    {
        return this.Neighbors(source).Count;
    }

    public IReadOnlyCollection<Vertex> Neighbors(Vertex source)
    {
        return this._adjacencyLists[source];
    }

    public void AddEdge(Vertex source, Vertex destination)
    {
        this._adjacencyLists[source].Add(destination);
        this._adjacencyLists[destination].Add(source);
    }

    public void ConnectNeighbors(Vertex source, Vertex target)
    {
        foreach (var neighbor in this.Neighbors(source))
            this.AddEdge(neighbor, target);
    }
}
