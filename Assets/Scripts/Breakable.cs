using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Breakable : MonoBehaviour
{
    public float health = 1f;
    public GameObject fragmentPrefab = null; 

    public void Damage(float damage) {
        var old_health = health;
        health -= damage;
        
        if (old_health >= 0f && health < 0f) {
            StartCoroutine("GetRekt");
        }
    }

    IEnumerator GetRekt() {
        yield return new WaitForSeconds(2f);
        BreakApart();
        Destroy(gameObject);
    }

    public void BreakApart() {
        var sprite = GetComponent<SpriteRenderer>().sprite;
        var width = sprite.rect.width / sprite.pixelsPerUnit;
        var height = sprite.rect.height / sprite.pixelsPerUnit;
        var uv_x = sprite.rect.x / sprite.texture.width;
        var uv_y = sprite.rect.y / sprite.texture.height;
        var uv_width  = sprite.rect.width / sprite.texture.width;
        var uv_height = sprite.rect.height / sprite.texture.height;

        var vertices = new Vector3[] {
            transform.TransformPoint(new Vector3(-0.5f * width, -0.5f * height, 0f)),
            transform.TransformPoint(new Vector3( 0.5f * width, -0.5f * height, 0f)),
            transform.TransformPoint(new Vector3( 0.5f * width,  0.5f * height, 0f)),
            transform.TransformPoint(new Vector3(-0.5f * width,  0.5f * height, 0f)),
        };

        var uv = new Vector2[] {
            new Vector2(uv_x, uv_y),
            new Vector2(uv_x + uv_width, uv_y),
            new Vector2(uv_x + uv_width, uv_y + uv_height),
            new Vector2(uv_x, uv_y + uv_height),
        };

        Split(vertices, uv);
    }

    public void Split(Vector3[] vertices, Vector2[] uvs, int recursion = 4) {
        Debug.Assert(vertices.Length == uvs.Length);
        Debug.Assert(vertices.Length >= 3);

        float total_length = 0f;
        for (int i = 0; i < vertices.Length; i++) {
            total_length += (vertices[(i + 1) % vertices.Length] - vertices[i]).magnitude;
        }

        float point_a = Random.Range(total_length * 0.15f, total_length * 0.35f);
        float point_b = point_a + total_length * 0.5f;

        var point_a_i = -1;
        var point_a_f = 0f;
        var point_b_i = -1;
        var point_b_f = 0f;

        for (int i = 0; i < vertices.Length; i++) {
            var line_length = (vertices[(i + 1) % vertices.Length] - vertices[i]).magnitude;
            if (point_a_i == -1 && point_a < line_length) {
                point_a_f = point_a / line_length;
                point_a_i = i;

                // The same line cannot contain both point_a and point_b.
                Debug.Assert(point_b > line_length);
            }

            if (point_b_i == -1 && point_b < line_length) {
                point_b_f = point_b / line_length;
                point_b_i = i;
            }

            point_a -= line_length;
            point_b -= line_length;
        }

        var point_a_pos = Vector3.Lerp(vertices[point_a_i], vertices[(point_a_i + 1) % vertices.Length], point_a_f);
        var point_a_uv = Vector2.Lerp(uvs[point_a_i], uvs[(point_a_i + 1) % vertices.Length], point_a_f);
        var point_b_pos = Vector3.Lerp(vertices[point_b_i], vertices[(point_b_i + 1) % vertices.Length], point_b_f);
        var point_b_uv = Vector2.Lerp(uvs[point_b_i], uvs[(point_b_i + 1) % vertices.Length], point_b_f);

        var polygon_a_length = point_a_i + (vertices.Length - point_b_i) + 2;
        var polygon_b_length = (point_b_i - point_a_i) + 2;

        var polygon_a_vertices = new Vector3[polygon_a_length];
        var polygon_a_uvs = new Vector2[polygon_a_length];
        for (int i = 0; i <= point_a_i; i++) {
            polygon_a_vertices[i] = vertices[i];
            polygon_a_uvs[i] = uvs[i];
        }
        polygon_a_vertices[point_a_i + 1] = point_a_pos;
        polygon_a_uvs[point_a_i + 1] = point_a_uv;
        polygon_a_vertices[point_a_i + 2] = point_b_pos;
        polygon_a_uvs[point_a_i + 2] = point_b_uv;
        for (int i = 0; i < (vertices.Length - point_b_i) - 1; i++) {
            polygon_a_vertices[i + point_a_i + 3] = vertices[i + point_b_i + 1];
            polygon_a_uvs[i + point_a_i + 3] = uvs[i + point_b_i + 1];
        }

        if (recursion == 0) {
            InstantiatePolygon(polygon_a_vertices, polygon_a_uvs);
        } else {
            Split(polygon_a_vertices, polygon_a_uvs, recursion - 1);
        }

        var polygon_b_vertices = new Vector3[polygon_b_length];
        var polygon_b_uvs = new Vector2[polygon_b_length];

        polygon_b_vertices[0] = point_a_pos;
        polygon_b_uvs[0] = point_a_uv;
        for (int i = 0; i < (point_b_i - point_a_i); i++) {
            polygon_b_vertices[i + 1] = vertices[point_a_i + i + 1];
            polygon_b_uvs[i + 1] = uvs[point_a_i + i + 1];
        }
        polygon_b_vertices[(point_b_i - point_a_i) + 1] = point_b_pos;
        polygon_b_uvs[(point_b_i - point_a_i) + 1] = point_b_uv;

        if (recursion == 0) {
            InstantiatePolygon(polygon_b_vertices, polygon_b_uvs);
        } else {
            Split(polygon_b_vertices, polygon_b_uvs, recursion - 1);
        }
    }

    public void InstantiatePolygon(Vector3[] vertices, Vector2[] uvs) {
        Vector3 center = Vector3.zero;
        foreach (var vertex in vertices) center += vertex;
        center /= (float)vertices.Length;

        for(int i = 0; i < vertices.Length; i++)
            vertices[i] = vertices[i] - center;
        
        var triangles = new int[(vertices.Length - 2) * 3];
        for (int i = 0; i < vertices.Length - 2; i++) {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        var instance = Instantiate(fragmentPrefab, center, Quaternion.identity);
        instance.GetComponent<MeshFilter>().mesh = mesh;

        var sprite = GetComponent<SpriteRenderer>().sprite;
        instance.GetComponent<MeshRenderer>().material.mainTexture = sprite.texture;

        var points = new Vector2[vertices.Length];
        for(int i = 0; i < vertices.Length; i++)
            points[i] = (Vector2)vertices[i];

        instance.GetComponent<PolygonCollider2D>().SetPath(0, points);

        Destroy(instance, Random.Range(2f, 7f));
    }
}
