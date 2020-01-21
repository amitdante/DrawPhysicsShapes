using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawCircle : DrawShape
{
    public Color FillColor = Color.white;

    private MeshFilter _meshFilter;
    private LineRenderer _lineRenderer;
    private Rigidbody2D _rigidbody2D;
    private CircleCollider2D _circleCollider2D;

    // Start and end vertices (in absolute coordinates)
    private readonly List<Vector2> _vertices = new List<Vector2>(2);

    public override bool ShapeFinished { get { return _vertices.Count >= 2; } }

    private bool _simulating;
    public override bool SimulatingPhysics
    {
        get { return _simulating; }
        set
        {
            _simulating = value;
            _rigidbody2D.bodyType = value ?
                    RigidbodyType2D.Dynamic : RigidbodyType2D.Static;
        }
    }

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _lineRenderer = GetComponent<LineRenderer>();
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.useAutoMass = true;
    }

    public override void AddVertex(Vector2 vertex)
    {
        if (ShapeFinished)
        {
            return;
        }

        _vertices.Add(vertex);
        UpdateShape(vertex);
    }

    public override void UpdateShape(Vector2 newVertex)
    {
        if (_vertices.Count < 2)
        {
            return;
        }

        _vertices[_vertices.Count - 1] = newVertex;

        // Set the gameobject's position to be the center of mass
        var center = _vertices.Centroid();
        transform.position = center;

        // Update the mesh relative to the transform
        var v0Relative = Vector2.zero;
        var v1Relative = _vertices[1] - _vertices[0];
        _meshFilter.mesh = CircleMesh(v0Relative, v1Relative, FillColor);

        // Update the shape's outline
        _lineRenderer.positionCount = _meshFilter.mesh.vertices.Length;
        _lineRenderer.SetPositions(_meshFilter.mesh.vertices);

        var dimensions = (_vertices[1] - _vertices[0]).Abs();
        _circleCollider2D.radius = Vector2.Distance(_vertices[0], _vertices[1]);
    }

    /// <summary>
    /// Creates and returns a rectangle mesh given two vertices on its 
    /// opposite corners and fills it with the given color. 
    /// </summary>
    private static Mesh CircleMesh(Vector2 v0, Vector2 v1, Color fillColor)
    {
        var radius = Vector2.Distance(v0, v1);

        // We want to make sure that the circle appears to be curved.
        // This can be approximated by drawing a regular polygon with lots of segments.
        // The number of segments can be increased based on the radius so that large circles also appear curved.
        // We use an offset and multiplier to create a tunable linear function.
        const float segmentOffset = 40f;
        const float segmentMultiplier = 2 * Mathf.PI;
        var numSegments = (int)(radius * segmentMultiplier + segmentOffset);

        // Create an array of points arround a cricle
        var circleVertices = Enumerable.Range(0, numSegments)
            .Select(i =>
            {
                var theta = 2 * Mathf.PI * i / numSegments;
                return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * radius;
            })
            .ToArray();

        // Find all the triangles in the shape
        var triangles = new Triangulator(circleVertices).Triangulate();

        // Assign each vertex the fill color
        var colors = Enumerable.Repeat(fillColor, circleVertices.Length).ToArray();

        var mesh = new Mesh
        {
            name = "Circle",
            vertices = circleVertices.ToVector3(),
            triangles = triangles,
            colors = colors
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
