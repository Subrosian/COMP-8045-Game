using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererEx : MonoBehaviour
{
    public int vertexCount = 40; // 4 vertices == square
    public float lineWidth = 0.2f;
    public float radius;
    public Color color;
    public float alphaFloat;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupCircle();
        alphaFloat = color.a;
    }

    public void updateColor()
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.material.color = color;
    }

    private void SetupCircle()
    {
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = color;
        lineRenderer.sortingOrder = 10;

        float deltaTheta = (2f * Mathf.PI) / (vertexCount-1); //'arc' covered between each pair of vertices, where the last vertex, at vertexCount-1, would be at the same position as the first one
        float theta = 0f;

        lineRenderer.positionCount = vertexCount; //edited name from positionCount to numPositions, where numPositions would be <<legacy code><YKWIM>>
        for (int i = 0; i < lineRenderer.positionCount; i++) //edited <rmh: <<to >to<rmh:  <<numPositions><YKWIM>><< numPositions><YKWIM>>
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0f);
            lineRenderer.SetPosition(i, transform.position + pos);
            theta += deltaTheta;
        }
    }
    private void Update()
    {
        
    }

    /// <summary>
    /// Set radius and update corresponding circle drawn.
    /// </summary>
    /// <param name="newRad"></param>
    public void setRadius(float newRad)
    {
        radius = newRad;

        //update corresponding circle
        float deltaTheta = (2f * Mathf.PI) / (vertexCount - 1); //'arc' covered between each pair of vertices, where the last vertex, at vertexCount-1, would be at the same position as the first one
        float theta = 0f;

        lineRenderer.positionCount = vertexCount; //edited name from positionCount to numPositions, where numPositions would be <<legacy code><YKWIM>>
        for (int i = 0; i < lineRenderer.positionCount; i++) //edited <rmh: <<to >to<rmh:  <<numPositions><YKWIM>><< numPositions><YKWIM>>
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0f);
            lineRenderer.SetPosition(i, transform.position + pos);
            theta += deltaTheta;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float deltaTheta = (2f * Mathf.PI) / vertexCount;
        float theta = 0f;

        Vector3 oldPos = Vector3.zero;
        for (int i = 0; i < vertexCount + 1; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0f);
            Gizmos.DrawLine(oldPos, transform.position + pos);
            oldPos = transform.position + pos;

            theta += deltaTheta;
        }
    }
#endif
}