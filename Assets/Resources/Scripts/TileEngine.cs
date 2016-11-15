﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileNeighbours
{
	public Vector2 left = new Vector2();
	public Vector2 right = new Vector2();
	public Vector2 top = new Vector2();
	public Vector2 bottom = new Vector2();
}

public class TileDescription
{
	// Note that a grid coordinate maps to four tiles.
	// And the tile described in this class will represent
	// the one that is upper right.
	public Vector3 worldPos = new Vector3();
	public Vector2 matrixCoord = new Vector2();
	public TileNeighbours neighbours = new TileNeighbours();
}

public interface ITileLayer
{
	void initTileLayer(TileEngine engine);
	void updateTiles(TileDescription[] tilesToMove);
	void removeAllTiles();
}

public interface ITileTerrainLayer : ITileLayer
{
	void updateTileNeighbours(TileDescription[] tilesWithNewNeighbours);
	float sampleHeight(Vector3 worldPos);
}

public class TileEngine : MonoBehaviour {
	[Range (2, 10)]
	public int tileCount = 4;
	[Range (1, 5000)]
	public float tileWorldSize = 100;
	public GameObject player;
	public bool showInEditor = false;

	float m_tileCountHalf;
	Vector3 m_tileCenterOffset;
	int m_playerShiftedPosX;
	int m_playerShiftedPosZ;
	Vector2 m_matrixTopRight = new Vector2();
	TileDescription[] m_tileMoveDesc;

	List<ITileLayer> m_tileLayerList;

	void Awake()
	{
		init();
		// Note: tile layer is responsible for calling
		// updateAllTiles from init, or upon callback that the
		// project has been loaded.
	}

	public void Update()
	{
		Vector3 playerWorldPos = player.transform.position;
		int prevPlayerShiftedPosX = m_playerShiftedPosX;
		int prevPlayerShiftedPosZ = m_playerShiftedPosZ;
		shiftedTilePosFromWorldPos(playerWorldPos, out m_playerShiftedPosX, out m_playerShiftedPosZ);

		int gridCrossedX = m_playerShiftedPosX - prevPlayerShiftedPosX;
		int gridCrossedZ = m_playerShiftedPosZ - prevPlayerShiftedPosZ;

		if (gridCrossedX == 0 && gridCrossedZ == 0)
			return;

		m_matrixTopRight.Set((float)matrixPos((int)m_matrixTopRight.x, gridCrossedX), (float)matrixPos((int)m_matrixTopRight.y, gridCrossedZ));

		if (gridCrossedX != 0)
			updateXTiles(gridCrossedX);

		if (gridCrossedZ != 0)
			updateZTiles(gridCrossedZ);
	}

	public void init()
	{
		m_tileCountHalf = tileCount / 2f;
		Debug.Assert(m_tileCountHalf == (int)m_tileCountHalf, "tileCount must be an even number");

		m_tileCenterOffset = new Vector3(tileWorldSize / 2f, 0, tileWorldSize / 2f);
		m_tileMoveDesc = new TileDescription[tileCount];
		m_tileLayerList = new List<ITileLayer>(GetComponentsInChildren<ITileLayer>());

		m_matrixTopRight.Set(tileCount - 1, tileCount - 1);
		shiftedTilePosFromWorldPos(player.transform.position, out m_playerShiftedPosX, out m_playerShiftedPosZ);

		for (int i = 0; i < tileCount; ++i)
			m_tileMoveDesc[i] = new TileDescription();

		foreach (ITileLayer tileLayer in m_tileLayerList)
			tileLayer.initTileLayer(this);
	}

	public void addTileLayer(ITileLayer tileLayer)
	{
		m_tileLayerList.Add(tileLayer);
	}

	public ITileLayer getTileLayer(int index)
	{
		return m_tileLayerList[index];
	}

	public void removeAllTiles()
	{
		ITileLayer[] tileLayers = GetComponentsInChildren<ITileLayer>();
		foreach (ITileLayer tileLayer in tileLayers)
			tileLayer.removeAllTiles();
	}

	public void worldPosFromTileCoord(float tileX, float tileY, ref Vector3 worldPos)
	{
		worldPos.Set(tileX * tileWorldSize, 0, tileY * tileWorldSize);
	}

	public void tileCoordFromWorldPos(Vector3 worldPos, out float tileX, out float tileY)
	{
		tileX = worldPos.x / tileWorldSize;
		tileY = worldPos.y / tileWorldSize;
	}

	public void matrixCoordFromWorldPos(Vector3 worldPos, out int matrixX, out int matrixY)
	{
		float argTileX, argTileY, playerTileX, playerTileY;
		tileCoordFromWorldPos(worldPos, out argTileX, out argTileY);
		tileCoordFromWorldPos(player.transform.position, out playerTileX, out playerTileY);
		float tileOffsetX = argTileX - playerTileX;
		float tileOffsetY = argTileY - playerTileY;

		Debug.Assert(Mathf.Abs(tileOffsetX) <= m_tileCountHalf && Mathf.Abs(tileOffsetY) <= m_tileCountHalf, "Worldpos outside current tiles");

		matrixX = matrixPos((int)m_matrixTopRight.x, (int)(tileOffsetX - m_tileCountHalf + 1));
		matrixY = matrixPos((int)m_matrixTopRight.y, (int)(tileOffsetY - m_tileCountHalf + 1));
	}

	public void worldPosFromMatrixCoord(int matrixX, int matrixY, ref Vector3 worldPos)
	{
		// Utilize the fact that the player is always at the center of the matrix
		Debug.Assert(matrixX >= 0 && matrixX < tileCount && matrixY >= 0 && matrixY < tileCount, "Matrix pos outside matrix");

		float playerTileX, playerTileY;
		tileCoordFromWorldPos(player.transform.position, out playerTileX, out playerTileY);
		float tileX = playerTileX - m_tileCountHalf + matrixX; // +1? -1?
		float tileY = playerTileY - m_tileCountHalf + matrixY;

		worldPosFromTileCoord(tileX, tileY, ref worldPos);
	}

	int matrixPos(int top, int rows)
	{
		return (tileCount + top + (rows % tileCount)) % tileCount;
	}

	void setNeighbours(Vector2 pos, ref TileNeighbours result)
	{
		int matrixTopEdge = (int)m_matrixTopRight.y;
		int matrixBottomEdge = matrixPos(matrixTopEdge, 1);
		int matrixRightEdge = (int)m_matrixTopRight.x;
		int matrixLeftEdge = matrixPos(matrixRightEdge, 1);

		bool onTopEdge = ((int)pos.y == matrixTopEdge);
		bool onBottomEdge = ((int)pos.y == matrixBottomEdge);
		bool onLeftEdge = ((int)pos.x == matrixLeftEdge);
		bool onRightEdge = ((int)pos.x == matrixRightEdge);

		if (onTopEdge) result.top.Set(-1, -1); else result.top.Set(pos.x, matrixPos((int)pos.y, 1));
		if (onBottomEdge) result.bottom.Set(-1, -1); else result.bottom.Set(pos.x, matrixPos((int)pos.y, -1));
		if (onLeftEdge) result.left.Set(-1, -1); else result.left.Set(matrixPos((int)pos.x, -1), pos.y);
		if (onRightEdge) result.right.Set(-1, -1); else result.right.Set(matrixPos((int)pos.x, 1), pos.y);
	}

	public void updateAllTiles()
	{
		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				m_tileMoveDesc[x].matrixCoord.Set(x, z);
				worldPosFromMatrixCoord(x, z, ref m_tileMoveDesc[x].worldPos);
				setNeighbours(m_tileMoveDesc[x].matrixCoord, ref m_tileMoveDesc[x].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerList) {
				tileLayer.updateTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

	private void shiftedTilePosFromWorldPos(Vector3 worldPos, out int centerPosX, out int centerPosY)
	{
		// Note: shiftedTilePos is an internal concept, and is only used to
		// determine when to update the tile matrix. We use shiftedTilePos to
		// shift the user position half a tile north-east to roll the matrix
		// when the user passes the center of a tile, rather than at the tile edge.
		centerPosX = Mathf.FloorToInt((worldPos.x + m_tileCenterOffset.x) / tileWorldSize);
		centerPosY = Mathf.FloorToInt((worldPos.z + m_tileCenterOffset.z) / tileWorldSize);
	}

	private void updateXTiles(int tilesCrossedX)
	{
		int moveDirection = tilesCrossedX > 0 ? 1 : -1;
		int nuberOfColsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedX), tileCount);

		for (int i = 0; i <= nuberOfColsToUpdate; ++i) {
			int matrixFrontX = matrixPos((int)m_matrixTopRight.x, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontX = matrixPos(matrixFrontX, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontZ = matrixPos((int)m_matrixTopRight.y, -j);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontZ);
				worldPosFromMatrixCoord(matrixFrontX, matrixFrontZ, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerList) {
				if (i < nuberOfColsToUpdate)
					tileLayer.updateTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

	private void updateZTiles(int tilesCrossedZ)
	{
		int moveDirection = tilesCrossedZ > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedZ), tileCount);

		for (int i = 0; i <= nuberOfRowsToUpdate; ++i) {
			int matrixFrontZ = matrixPos((int)m_matrixTopRight.y, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontZ = matrixPos(matrixFrontZ, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontX = matrixPos((int)m_matrixTopRight.x, -j);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontZ);
				worldPosFromMatrixCoord(matrixFrontX, matrixFrontZ, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerList) {
				if (i < nuberOfRowsToUpdate)
					tileLayer.updateTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}
}
