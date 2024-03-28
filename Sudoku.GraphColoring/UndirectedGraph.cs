namespace Sudoku.GraphColoring;

public class UndirectedGraph
{
    public int VertexCount { get;  }

    private readonly List<int>[] _adjacencyLists;

    public UndirectedGraph(int vertexCount)
    {
        this.VertexCount = vertexCount;
        this._adjacencyLists = new List<int>[vertexCount];
        for (int vertex = 0; vertex < vertexCount; ++vertex)
            this._adjacencyLists[vertex] = [];
    }

    public IEnumerable<(int, int)> Edges()
    {
        for (int source = 0; source < this.VertexCount; ++source)
            foreach (var destination in this.Neighbors(source))
                yield return (Math.Min(source, destination), Math.Max(source, destination));
    }

    public ReadOnlyCollection<int> Neighbors(int source)
    {
        return this._adjacencyLists[source].AsReadOnly();
    }

    public void AddEdge(int source, int destination)
    {
        this._adjacencyLists[source].Add(destination);
        this._adjacencyLists[destination].Add(source);
    }
}
