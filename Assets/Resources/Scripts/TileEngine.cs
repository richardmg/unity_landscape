﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntCoord
{
	public IntCoord(int x = 0, int y = 0)
	{
		this.x = x;
		this.y = y;
	}

	public void set(IntCoord other)
	{
		this.x = other.x;
		this.y = other.y;
	}

	public void set(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public void add(int x, int y)
	{
		this.x += x;
		this.y += y;
	}

	public int x;
	public int y;
}

public class TileNeighbours
{
	public IntCoord left = new IntCoord();
	public IntCoord right = new IntCoord();
	public IntCoord top = new IntCoord();
	public IntCoord bottom = new IntCoord();
}

public class TileDescription
{
	public Vector3 worldPos = new Vector3();
	public IntCoord tileCoord = new IntCoord();
	public IntCoord matrixCoord = new IntCoord();
	public TileNeighbours neighbours = new TileNeighbours();
}

public class TileEngine
{
	public int tileCount = 4;
	public float tileWorldSize = 100;

	int m_tileCountHalf;
	Vector3 m_tileCenterOffset;
	IntCoord m_shiftedTilePos;
	IntCoord m_prevShiftedTilePos;
	IntCoord m_matrixTopRight;
	IntCoord m_matrixTopRightTileCoord;

	TileDescription[] m_tileMoveDesc;

	public TileEngine(int tileCount, float tileWorldSize)
	{
		this.tileCount = tileCount;
		this.tileWorldSize = tileWorldSize;
		m_tileCountHalf = tileCount / 2;
		Debug.Assert(m_tileCountHalf == tileCount / 2f, "tileCount must be an even number");

		m_tileCenterOffset = new Vector3(tileWorldSize / 2f, 0, tileWorldSize / 2f);
		m_tileMoveDesc = new TileDescription[tileCount];
		m_matrixTopRight = new IntCoord(tileCount - 1, tileCount - 1);
		m_matrixTopRightTileCoord = new IntCoord(m_tileCountHalf, m_tileCountHalf);
		m_shiftedTilePos = new IntCoord();
		shiftedTilePosFromWorldPos(Vector3.zero, ref m_shiftedTilePos);
		m_prevShiftedTilePos = new IntCoord();
		m_prevShiftedTilePos.set(m_shiftedTilePos);

		for (int i = 0; i < tileCount; ++i)
			m_tileMoveDesc[i] = new TileDescription();
	}

	public void worldPosForTileCoord(IntCoord tileCoord, ref Vector3 worldPos)
	{
		worldPos.Set(tileCoord.x * tileWorldSize, 0, tileCoord.y * tileWorldSize);
	}

	public void tileCoordAtWorldPos(Vector3 worldPos, out int tileX, out int tileZ)
	{
		tileX = (int)(worldPos.x / tileWorldSize);
		tileZ = (int)(worldPos.z / tileWorldSize);
	}

	public void matrixCoordForTileCoord(int tileX, int tileZ, out int matrixX, out int matrixY)
	{
		int tileOffsetX = tileX - m_matrixTopRightTileCoord.x;
		int tileOffsetZ = tileZ - m_matrixTopRightTileCoord.y;
		matrixX = matrixPos((int)m_matrixTopRight.x, -tileOffsetX);
		matrixY = matrixPos((int)m_matrixTopRight.y, -tileOffsetZ);
	}

	public void tileCoordForMatrixCoord(int matrixX, int matrixY, ref IntCoord tileCoord)
	{
		// Normalize arg matrix coord (as if the matrix were unshifted)
		int matrixXNormalized = matrixPos(matrixX, -(int)m_matrixTopRight.x + (tileCount - 1)); 
		int matrixYNormalized = matrixPos(matrixY, -(int)m_matrixTopRight.y + (tileCount - 1)); 
		int tileOffsetX = tileCount - matrixXNormalized;
		int tileOffsetZ = tileCount - matrixYNormalized;
		tileCoord.set(m_matrixTopRightTileCoord.x - tileOffsetX, m_matrixTopRightTileCoord.y - tileOffsetZ);
	}

	int matrixPos(int top, int offset)
	{
		return (tileCount + top + (offset % tileCount)) % tileCount;
	}

	void setNeighbours(IntCoord pos, ref TileNeighbours result)
	{
		int matrixTopEdge = (int)m_matrixTopRight.y;
		int matrixBottomEdge = matrixPos(matrixTopEdge, 1);
		int matrixRightEdge = (int)m_matrixTopRight.x;
		int matrixLeftEdge = matrixPos(matrixRightEdge, 1);

		bool onTopEdge = ((int)pos.y == matrixTopEdge);
		bool onBottomEdge = ((int)pos.y == matrixBottomEdge);
		bool onLeftEdge = ((int)pos.x == matrixLeftEdge);
		bool onRightEdge = ((int)pos.x == matrixRightEdge);

		if (onTopEdge) result.top.set(-1, -1); else result.top.set(pos.x, matrixPos((int)pos.y, 1));
		if (onBottomEdge) result.bottom.set(-1, -1); else result.bottom.set(pos.x, matrixPos((int)pos.y, -1));
		if (onLeftEdge) result.left.set(-1, -1); else result.left.set(matrixPos((int)pos.x, -1), pos.y);
		if (onRightEdge) result.right.set(-1, -1); else result.right.set(matrixPos((int)pos.x, 1), pos.y);
	}

	private void shiftedTilePosFromWorldPos(Vector3 worldPos, ref IntCoord shiftedTilePos)
	{
		// Note: shiftedTilePos is an internal concept, and is only used to
		// determine when to update the tile matrix. We use shiftedTilePos to
		// shift the user position half a tile north-east to roll the matrix
		// when the user passes the center of a tile, rather than at the edge.
		shiftedTilePos.x = (int)((worldPos.x + m_tileCenterOffset.x) / tileWorldSize);
		shiftedTilePos.y = (int)((worldPos.z + m_tileCenterOffset.z) / tileWorldSize);
	}

	public void updateAllTiles(Action<TileDescription[]> callback)
	{
		for (int matrixZ = 0; matrixZ < tileCount; ++matrixZ) {
			for (int matrixX = 0; matrixX < tileCount; ++matrixX) {
				m_tileMoveDesc[matrixX].matrixCoord.set(matrixX, matrixZ);
				tileCoordForMatrixCoord(matrixX, matrixZ, ref m_tileMoveDesc[matrixX].tileCoord);
				worldPosForTileCoord(m_tileMoveDesc[matrixX].tileCoord, ref m_tileMoveDesc[matrixX].worldPos);
				setNeighbours(m_tileMoveDesc[matrixX].matrixCoord, ref m_tileMoveDesc[matrixX].neighbours);
			}

			callback(m_tileMoveDesc);
		}
	}

	public void updateTiles(Vector3 worldPos, Action<TileDescription[]> callback)
	{
		m_prevShiftedTilePos.set(m_shiftedTilePos);
		shiftedTilePosFromWorldPos(worldPos, ref m_shiftedTilePos);
		int shiftedX = m_shiftedTilePos.x - m_prevShiftedTilePos.x;
		int shiftedZ = m_shiftedTilePos.y - m_prevShiftedTilePos.y;
		if (shiftedX == 0 && shiftedZ == 0)
			return;

		// Update matrix top-right
		m_matrixTopRightTileCoord.add(shiftedX, shiftedZ);
		m_matrixTopRight.set(matrixPos(m_matrixTopRight.x, shiftedX), matrixPos(m_matrixTopRight.y, shiftedZ));

		// Inform listeners about the change
		if (shiftedX != 0)
			updateXTiles(shiftedX, callback);

		if (shiftedZ != 0)
			updateZTiles(shiftedZ, callback);
	}

	private void updateXTiles(int shiftedX, Action<TileDescription[]> callback)
	{
		int moveDirection = shiftedX > 0 ? 1 : -1;
		int nuberOfColsToUpdate = Mathf.Min(Mathf.Abs(shiftedX), tileCount);

		for (int i = 0; i <= nuberOfColsToUpdate; ++i) {
			int matrixFrontX = matrixPos(m_matrixTopRight.x, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontX = matrixPos(matrixFrontX, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontY = matrixPos(m_matrixTopRight.y, -j);
				m_tileMoveDesc[j].matrixCoord.set(matrixFrontX, matrixFrontY);
				tileCoordForMatrixCoord(matrixFrontX, matrixFrontY, ref m_tileMoveDesc[j].tileCoord);
				worldPosForTileCoord(m_tileMoveDesc[j].tileCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			callback(m_tileMoveDesc);
		}
	}

	private void updateZTiles(int shiftedZ, Action<TileDescription[]> callback)
	{
		int moveDirection = shiftedZ > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(shiftedZ), tileCount);

		for (int i = 0; i <= nuberOfRowsToUpdate; ++i) {
			int matrixFrontY = matrixPos(m_matrixTopRight.y, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontY = matrixPos(matrixFrontY, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontX = matrixPos(m_matrixTopRight.x, -j);
				m_tileMoveDesc[j].matrixCoord.set(matrixFrontX, matrixFrontY);
				tileCoordForMatrixCoord(matrixFrontX, matrixFrontY, ref m_tileMoveDesc[j].tileCoord);
				worldPosForTileCoord(m_tileMoveDesc[j].tileCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			callback(m_tileMoveDesc);
		}
	}
}
