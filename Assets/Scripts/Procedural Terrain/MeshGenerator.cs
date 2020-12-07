using UnityEngine;
using System.Collections;

public static class MeshGenerator
{
	public static float mapHeightStep = 1.5f;

	public static MeshData GenerateTerrainMesh(float[,] heightMap, int[,] terrainType, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
	{
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

		MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
		int vertexIndex = 0;

		for (int y = 0; y < height; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < width; x += meshSimplificationIncrement)
			{
				meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, RoundToStep(heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier), topLeftZ - y);
				meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
				meshData.terrainTypes[vertexIndex] = terrainType[x, y];

				if (x < width - 1 && y < height - 1) { 
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		meshData.FlatShading();

		return meshData;
	}

	public static float RoundToStep(float height)
	{
		return Mathf.Round(height / mapHeightStep) * mapHeightStep;

	}
}



public class MeshData
{
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;
	public Vector3[] terrainUVS;
	public int[] terrainTypes;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight)
	{
		vertices = new Vector3[meshWidth * meshHeight];
		terrainTypes = new int[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
	}

	public void AddTriangle(int a, int b, int c)
	{
		triangles[triangleIndex] = a;
		triangles[triangleIndex + 1] = b;
		triangles[triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public void FlatShading()
	{
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		Vector2[] flatShadedUvs = new Vector2[triangles.Length];
		terrainUVS = new Vector3[triangles.Length];

		for (int i = 0; i < triangles.Length; i++)
		{
			flatShadedVertices[i] = vertices[triangles[i]];
			flatShadedUvs[i] = uvs[triangles[i]];
			terrainUVS[i] = new Vector3(terrainTypes[triangles[i]], terrainTypes[triangles[i]], terrainTypes[triangles[i]]);
			triangles[i] = i;
		}

		vertices = flatShadedVertices;
		uvs = flatShadedUvs;

	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.SetUVs(2, terrainUVS);
		mesh.RecalculateNormals();
		return mesh;
	}
}