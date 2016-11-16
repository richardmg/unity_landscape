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
	public Vector3 worldPos = new Vector3();
	public Vector2 tileCoord = new Vector2();
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

	int m_matrixTopRightTileCoordX;
	int m_matrixTopRightTileCoordZ;

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

	public void init()
	{
		m_tileCountHalf = tileCount / 2f;
		Debug.Assert(m_tileCountHalf == (int)m_tileCountHalf, "tileCount must be an even number");

		m_tileCenterOffset = new Vector3(tileWorldSize / 2f, 0, tileWorldSize / 2f);
		m_tileMoveDesc = new TileDescription[tileCount];
		m_tileLayerList = new List<ITileLayer>(GetComponentsInChildren<ITileLayer>());

		m_matrixTopRight.Set(tileCount - 1, tileCount - 1);
		int playerTileX, playerTileZ;
		tileCoordAtWorldPos(player.transform.position, out playerTileX, out playerTileZ);
		m_matrixTopRightTileCoordX = (int)(playerTileX + m_tileCountHalf);
		m_matrixTopRightTileCoordZ = (int)(playerTileZ + m_tileCountHalf);
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

	public void worldPosForTileCoord(float tileX, float tileZ, ref Vector3 worldPos)
	{
		worldPos.Set(tileX * tileWorldSize, 0, tileZ * tileWorldSize);
	}

	public void tileCoordAtWorldPos(Vector3 worldPos, out int tileX, out int tileZ)
	{
		tileX = (int)(worldPos.x / tileWorldSize);
		tileZ = (int)(worldPos.z / tileWorldSize);
	}

	public void matrixCoordForTileCoord(float tileX, float tileZ, out int matrixX, out int matrixY)
	{
		int tileOffsetX = (int)(tileX - m_matrixTopRightTileCoordX);
		int tileOffsetZ = (int)(tileZ - m_matrixTopRightTileCoordZ);
		matrixX = matrixPos((int)m_matrixTopRight.x, -tileOffsetX);
		matrixY = matrixPos((int)m_matrixTopRight.y, -tileOffsetZ);
	}

	public void tileCoordForMatrixCoord(int matrixX, int matrixY, out float tileX, out float tileZ)
	{
		// Normalize arg matrix coord (as if the matrix were unshifted)
		int matrixXNormalized = matrixPos(matrixX, -(int)m_matrixTopRight.x + (tileCount - 1)); 
		int matrixYNormalized = matrixPos(matrixY, -(int)m_matrixTopRight.y + (tileCount - 1)); 
		int tileOffsetX = tileCount - matrixXNormalized;
		int tileOffsetZ = tileCount - matrixYNormalized;
		tileX = (int)m_matrixTopRightTileCoordX - tileOffsetX;
		tileZ = (int)m_matrixTopRightTileCoordZ - tileOffsetZ;
	}

	int matrixPos(int top, int offset)
	{
		return (tileCount + top + (offset % tileCount)) % tileCount;
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
		for (int matrixZ = 0; matrixZ < tileCount; ++matrixZ) {
			for (int matrixX = 0; matrixX < tileCount; ++matrixX) {
				m_tileMoveDesc[matrixX].matrixCoord.Set(matrixX, matrixZ);
				float tileX, tileZ;
				tileCoordForMatrixCoord(matrixX, matrixZ, out tileX, out tileZ);
				m_tileMoveDesc[matrixX].tileCoord.Set(tileX, tileZ);
				worldPosForTileCoord(tileX, tileZ, ref m_tileMoveDesc[matrixX].worldPos);
				setNeighbours(m_tileMoveDesc[matrixX].matrixCoord, ref m_tileMoveDesc[matrixX].neighbours);
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
		// when the user passes the center of a tile, rather than at the edge.
		centerPosX = Mathf.FloorToInt((worldPos.x + m_tileCenterOffset.x) / tileWorldSize);
		centerPosY = Mathf.FloorToInt((worldPos.z + m_tileCenterOffset.z) / tileWorldSize);
	}

	public void Update()
	{
		Vector3 playerWorldPos = player.transform.position;
		int prevPlayerShiftedPosX = m_playerShiftedPosX;
		int prevPlayerShiftedPosZ = m_playerShiftedPosZ;
		shiftedTilePosFromWorldPos(playerWorldPos, out m_playerShiftedPosX, out m_playerShiftedPosZ);
		int shiftedX = m_playerShiftedPosX - prevPlayerShiftedPosX;
		int shiftedZ = m_playerShiftedPosZ - prevPlayerShiftedPosZ;
		if (shiftedX == 0 && shiftedZ == 0)
			return;

		m_matrixTopRightTileCoordX += shiftedX;
		m_matrixTopRightTileCoordZ += shiftedZ;
		m_matrixTopRight.Set((float)matrixPos((int)m_matrixTopRight.x, shiftedX), (float)matrixPos((int)m_matrixTopRight.y, shiftedZ));

		if (shiftedX != 0)
			updateXTiles(shiftedX);

		if (shiftedZ != 0)
			updateZTiles(shiftedZ);
	}

	private void updateXTiles(int shiftedX)
	{
		int moveDirection = shiftedX > 0 ? 1 : -1;
		int nuberOfColsToUpdate = Mathf.Min(Mathf.Abs(shiftedX), tileCount);

		for (int i = 0; i < nuberOfColsToUpdate; ++i) {
			int matrixFrontX = matrixPos((int)m_matrixTopRight.x, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontX = matrixPos(matrixFrontX, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontY = matrixPos((int)m_matrixTopRight.y, -j);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontY);
				float tileX, tileZ;
				tileCoordForMatrixCoord(matrixFrontX, matrixFrontY, out tileX, out tileZ);
				m_tileMoveDesc[j].tileCoord.Set(tileX, tileZ);
				worldPosForTileCoord(tileX, tileZ, ref m_tileMoveDesc[j].worldPos);
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

	private void updateZTiles(int shiftedZ)
	{
		int moveDirection = shiftedZ > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(shiftedZ), tileCount);

		for (int i = 0; i < nuberOfRowsToUpdate; ++i) {
			int matrixFrontY = matrixPos((int)m_matrixTopRight.y, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontY = matrixPos(matrixFrontY, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontX = matrixPos((int)m_matrixTopRight.x, -j);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontY);
				float tileX, tileZ;
				tileCoordForMatrixCoord(matrixFrontX, matrixFrontY, out tileX, out tileZ);
				m_tileMoveDesc[j].tileCoord.Set(tileX, tileZ);
				worldPosForTileCoord(tileX, tileZ, ref m_tileMoveDesc[j].worldPos);
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
