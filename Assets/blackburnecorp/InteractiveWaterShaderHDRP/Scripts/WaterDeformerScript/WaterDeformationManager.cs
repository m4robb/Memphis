using UnityEngine;
/// <summary>
/// Main water deformation class
/// Using Mesh API for deforming vertices and recalculating normals
/// Attach to water object that has MeshFilter component 
/// </summary>
public class WaterDeformationManager : MonoBehaviour
{
    public float surfaceForce = 20f;
    public float dampValue = 5f;
    private  Mesh _deformingMesh;
    private Vector3[] _initialVerticesArray;
    private Vector3[] _deformedVerticesArray; 
    private Vector3[] _verticesVelocitiesArray;
    float uniformScale = 1f;

    void Start()
    {
        _deformingMesh = GetComponent<MeshFilter>().mesh;
        _initialVerticesArray = _deformingMesh.vertices;
        _deformedVerticesArray = new Vector3[_initialVerticesArray.Length];
        for (int i = 0; i < _initialVerticesArray.Length; i++)
        {
            _deformedVerticesArray[i] = _initialVerticesArray[i];
        }
        _verticesVelocitiesArray = new Vector3[_initialVerticesArray.Length];
    }
    void Update()
    {
        uniformScale = transform.localScale.x;
        for (int i = 0; i < _deformedVerticesArray.Length; i++)
        {
            RefreshReformation(i);
        }
        _deformingMesh.vertices = _deformedVerticesArray;
        _deformingMesh.RecalculateNormals();
    }
    /// <summary>
    /// Refresh vertices positions and reform into normal shape after each deform attempt 
    /// </summary>
    /// <param name="i">vertex index</param>
    void RefreshReformation(int index)
    {
        Vector3 velocity = _verticesVelocitiesArray[index];
        Vector3 displacement = _deformedVerticesArray[index] - _initialVerticesArray[index];
        displacement *= uniformScale;
        velocity -= displacement * surfaceForce * Time.deltaTime;
        velocity *= 1f - dampValue * Time.deltaTime;
        _verticesVelocitiesArray[index] = velocity;
        _deformedVerticesArray[index] += velocity * (Time.deltaTime / uniformScale);
    }

    /// <summary>
    /// Apply deformation action from other scripts 
    /// </summary>
    /// <param name="point">Touch water position</param>
    /// <param name="force">Deformation force</param>
    public void ApplyDeformation(Vector3 touchPoint, float Appliedforce)
    {
        touchPoint = transform.InverseTransformPoint(touchPoint);
        for (int i = 0; i < _deformedVerticesArray.Length; i++)
        {
            AddForceToVertex(i, touchPoint, Appliedforce);
        }
    }

    /// <summary>
    /// Apply deformation force on each vertex index from the mesh
    /// </summary>
    /// <param name="i">Vertex index</param>
    /// <param name="point">Touch water position</param>
    /// <param name="force">Deform force</param>
    void AddForceToVertex(int index, Vector3 touchPoint, float force)
    {
        Vector3 pointToVertex = _deformedVerticesArray[index] - touchPoint;
        pointToVertex *= uniformScale;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        _verticesVelocitiesArray[index] += pointToVertex.normalized * velocity;
    }
}