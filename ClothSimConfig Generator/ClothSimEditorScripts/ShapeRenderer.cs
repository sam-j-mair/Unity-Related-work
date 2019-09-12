using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Shape
{
    public enum ShapeType
    {
        Sphere,
        Capsule,
        None
    }

    public ShapeType ShapeEnum { get; set; } = ShapeType.None;
    public float Radius { get; set; } = 0.0f;
    public float Length { get; set; }
    public Transform Transform { get; set; }
    
}


public class ShapeRenderer : MonoBehaviour
{
    public Shape ShapeDefinition { get; set; } = new Shape();

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Initialise(Shape.ShapeType shapeType, Quaternion rotation, Vector3 position, float radius, float length = 0.0f)
    {
        gameObject.SetActive(true);
        transform.rotation = rotation;
        transform.position = position;

        ShapeDefinition.ShapeEnum = shapeType;
        ShapeDefinition.Radius = radius;
        ShapeDefinition.Length = length;
    }

    public void Clear()
    {
        gameObject.SetActive(false);

        ShapeDefinition.ShapeEnum = Shape.ShapeType.None;

    }

    private void DrawCapsule()
    {
        float halfLength = ShapeDefinition.Length / 2.0f;
        float radius = ShapeDefinition.Radius;

        Vector3 up = transform.up;
        Vector3 pos = transform.position;
        Vector3 to = pos + (up * (halfLength + radius));
        Vector3 from = pos - (up * (halfLength + radius));

        DebugExtension.DrawCapsule(to, from, Gizmos.color, radius);
    }

    private void DrawSphere()
    {
        Gizmos.DrawWireSphere(transform.position, ShapeDefinition.Radius);
    }

    private void OnDrawGizmos()
    {
        switch(ShapeDefinition.ShapeEnum)
        {
            case Shape.ShapeType.Sphere:
                DrawSphere();

                break;

            case Shape.ShapeType.Capsule:
                DrawCapsule();
                break;

            default:
                break;
                
        }
    }
}
