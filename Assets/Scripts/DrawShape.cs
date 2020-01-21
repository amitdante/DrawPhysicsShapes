using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DrawShape : MonoBehaviour
{
    /// <summary> 
    /// Whether all the points in the shape have been specified 
    /// </summary> 
    public abstract bool ShapeFinished { get; }

    /// <summary> 
    /// The status of whether the shape is simulating its physics. 
    /// Setting this property will enable or disable physics. 
    /// </summary> 
    public abstract bool SimulatingPhysics { get; set; }

    /// <summary> 
    /// Adds a new vertex to the shape. The shape should  
    /// also update its mesh and collider. 
    /// </summary> 
    public abstract void AddVertex(Vector2 vertex);

    /// <summary> 
    /// Updates the last added vertex with the new given position. 
    /// The shape should also update its mesh and collider. 
    /// </summary> 
    public abstract void UpdateShape(Vector2 newVertex);
}
