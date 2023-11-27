using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
	public static float[,] GenerateNoiseMap(int islandSize, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
	{

		float[,] noiseMap = new float[islandSize, islandSize];

		System.Random prng = new System.Random(seed);
		Vector2[] octaveOffsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < octaves; i++)
		{
			float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) - offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		float halfSize = islandSize / 2;


		for (int y = 0; y < islandSize; y++)
		{
			for (int x = 0; x < islandSize; x++)
			{

				float noiseHeight = 0;
				amplitude = 1;
				frequency = 1;

				for (int i = 0; i < octaves; i++)
				{
					float sampleX = (x - halfSize + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y - halfSize + octaveOffsets[i].y) / scale * frequency;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxNoiseHeight)
				{
					maxNoiseHeight = noiseHeight;
				}
				else if (noiseHeight < minNoiseHeight)
				{
					minNoiseHeight = noiseHeight;
				}
				noiseMap[x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < islandSize; y++)
		{
			for (int x = 0; x < islandSize; x++)
			{
				//noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
				float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
				noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
			}
		}

		return noiseMap;
	}

}
