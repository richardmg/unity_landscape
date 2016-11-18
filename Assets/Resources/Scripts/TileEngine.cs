using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	Vector2 m_shiftedTileOffset;

	IntCoord m_shiftedTileCoord;
	IntCoord m_prevShiftedTileCoord;
	IntCoord m_matrixTopRightCoord;
	IntCoord m_matrixTopRightTileCoord;

	TileDescription[] m_tileMoveDesc;

	public TileEngine(int tileCount, float tileWorldSize)
	{
		this.tileCount = tileCount;
		this.tileWorldSize = tileWorldSize;
		m_tileCountHalf = tileCount / 2;
		Debug.Assert(m_tileCountHalf == tileCount / 2f, "tileCount must be an even number");

		m_shiftedTileOffset = new Vector2(tileWorldSize / 2f, tileWorldSize / 2f);
		m_tileMoveDesc = new TileDescription[tileCount];
		m_matrixTopRightCoord = new IntCoord(tileCount - 1, tileCount - 1);
		m_matrixTopRightTileCoord = new IntCoord(m_tileCountHalf, m_tileCountHalf);
		m_shiftedTileCoord = new IntCoord(0, 0);
		m_prevShiftedTileCoord = new IntCoord(0, 0);

		for (int i = 0; i < tileCount; ++i)
			m_tileMoveDesc[i] = new TileDescription();
	}

	public void worldPosForTileCoord(IntCoord tileCoord, ref Vector3 worldPos)
	{
		worldPos.Set(tileCoord.x * tileWorldSize, 0, tileCoord.y * tileWorldSize);
	}

	public void tileCoordAtWorldPos(Vector3 worldPos, out int tileX, out int tileY)
	{
		tileX = (int)(worldPos.x / tileWorldSize);
		tileY = (int)(worldPos.z / tileWorldSize);
	}

	public IntCoord matrixCoordForWorldPos(Vector3 worldPos)
	{
		int tileX, tileY;
		IntCoord matrixCoord = new IntCoord();
		tileCoordAtWorldPos(worldPos, out tileX, out tileY);
		matrixCoordForTileCoord(tileX, tileY, ref matrixCoord);
		return matrixCoord;
	}

	public void matrixCoordForTileCoord(int tileX, int tileY, ref IntCoord matrixCoord)
	{
		int tileOffsetX = tileX - m_matrixTopRightTileCoord.x;
		int tileOffsetY = tileY - m_matrixTopRightTileCoord.y;
		Debug.Assert(tileOffsetX < 0 && tileOffsetY < 0 && tileOffsetX > -tileCount && tileOffsetY > -tileCount, "Tile coord outside current matrix window");
		matrixCoord.set(matrixPos(m_matrixTopRightCoord.x, -tileOffsetX), matrixPos(m_matrixTopRightCoord.y, -tileOffsetY));
	}

	public void tileCoordForMatrixCoord(int matrixX, int matrixY, ref IntCoord tileCoord)
	{
		// Normalize arg matrix coord (as if the matrix were unshifted)
		int matrixXNormalized = matrixPos(matrixX, -m_matrixTopRightCoord.x + (tileCount - 1)); 
		int matrixYNormalized = matrixPos(matrixY, -m_matrixTopRightCoord.y + (tileCount - 1)); 
		int tileOffsetX = tileCount - matrixXNormalized;
		int tileOffsetY = tileCount - matrixYNormalized;
		tileCoord.set(m_matrixTopRightTileCoord.x - tileOffsetX, m_matrixTopRightTileCoord.y - tileOffsetY);
	}

	int matrixPos(int top, int offset)
	{
		return (tileCount + top + (offset % tileCount)) % tileCount;
	}

	void setNeighbours(IntCoord pos, ref TileNeighbours result)
	{
		int matrixTopEdge = m_matrixTopRightCoord.y;
		int matrixBottomEdge = matrixPos(matrixTopEdge, 1);
		int matrixRightEdge = m_matrixTopRightCoord.x;
		int matrixLeftEdge = matrixPos(matrixRightEdge, 1);

		bool onTopEdge = (pos.y == matrixTopEdge);
		bool onBottomEdge = (pos.y == matrixBottomEdge);
		bool onLeftEdge = (pos.x == matrixLeftEdge);
		bool onRightEdge = (pos.x == matrixRightEdge);

		if (onTopEdge) result.top.set(-1, -1); else result.top.set(pos.x, matrixPos(pos.y, 1));
		if (onBottomEdge) result.bottom.set(-1, -1); else result.bottom.set(pos.x, matrixPos(pos.y, -1));
		if (onLeftEdge) result.left.set(-1, -1); else result.left.set(matrixPos(pos.x, -1), pos.y);
		if (onRightEdge) result.right.set(-1, -1); else result.right.set(matrixPos(pos.x, 1), pos.y);
	}

	public void updateAllTiles(Action<TileDescription[]> callback)
	{
		for (int matrixY = 0; matrixY < tileCount; ++matrixY) {
			for (int matrixX = 0; matrixX < tileCount; ++matrixX) {
				m_tileMoveDesc[matrixX].matrixCoord.set(matrixX, matrixY);
				tileCoordForMatrixCoord(matrixX, matrixY, ref m_tileMoveDesc[matrixX].tileCoord);
				worldPosForTileCoord(m_tileMoveDesc[matrixX].tileCoord, ref m_tileMoveDesc[matrixX].worldPos);
				setNeighbours(m_tileMoveDesc[matrixX].matrixCoord, ref m_tileMoveDesc[matrixX].neighbours);
			}

			callback(m_tileMoveDesc);
		}
	}

	private void shiftedTilePosFromWorldPos(Vector3 worldPos, ref IntCoord shiftedTilePos)
	{
		// Note: shiftedTilePos is an internal concept, and is only used to
		// determine when to update the tile matrix. We use shiftedTilePos to
		// shift the user position half a tile north-east to roll the matrix
		// when the user passes the center of a tile, rather than at the edge.
		shiftedTilePos.x = (int)((worldPos.x + (m_shiftedTileOffset.x * Mathf.Sign(worldPos.x))) / tileWorldSize);
		shiftedTilePos.y = (int)((worldPos.z + (m_shiftedTileOffset.y * Mathf.Sign(worldPos.z))) / tileWorldSize);
	}

	public void updateTiles(Vector3 worldPos, Action<TileDescription[]> callback)
	{
		m_prevShiftedTileCoord.set(m_shiftedTileCoord);
		shiftedTilePosFromWorldPos(worldPos, ref m_shiftedTileCoord);
		int shiftedX = m_shiftedTileCoord.x - m_prevShiftedTileCoord.x;
		int shiftedY = m_shiftedTileCoord.y - m_prevShiftedTileCoord.y;
		if (shiftedX == 0 && shiftedY == 0)
			return;

		// Update matrix top-right
		m_matrixTopRightTileCoord.add(shiftedX, shiftedY);
		m_matrixTopRightCoord.set(matrixPos(m_matrixTopRightCoord.x, shiftedX), matrixPos(m_matrixTopRightCoord.y, shiftedY));

		// Inform listeners about the change
		if (shiftedX != 0)
			updateXTiles(shiftedX, callback);

		if (shiftedY != 0)
			updateYTiles(shiftedY, callback);
	}

	private void updateXTiles(int shiftedX, Action<TileDescription[]> callback)
	{
		int moveDirection = shiftedX > 0 ? 1 : -1;
		int nuberOfColsToUpdate = Mathf.Min(Mathf.Abs(shiftedX), tileCount);

		for (int i = 0; i <= nuberOfColsToUpdate; ++i) {
			int matrixFrontX = matrixPos(m_matrixTopRightCoord.x, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontX = matrixPos(matrixFrontX, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontY = matrixPos(m_matrixTopRightCoord.y, -j);
				m_tileMoveDesc[j].matrixCoord.set(matrixFrontX, matrixFrontY);
				tileCoordForMatrixCoord(matrixFrontX, matrixFrontY, ref m_tileMoveDesc[j].tileCoord);
				worldPosForTileCoord(m_tileMoveDesc[j].tileCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			callback(m_tileMoveDesc);
		}
	}

	private void updateYTiles(int shiftedY, Action<TileDescription[]> callback)
	{
		int moveDirection = shiftedY > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(shiftedY), tileCount);

		for (int i = 0; i <= nuberOfRowsToUpdate; ++i) {
			int matrixFrontY = matrixPos(m_matrixTopRightCoord.y, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontY = matrixPos(matrixFrontY, 1);

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontX = matrixPos(m_matrixTopRightCoord.x, -j);
				m_tileMoveDesc[j].matrixCoord.set(matrixFrontX, matrixFrontY);
				tileCoordForMatrixCoord(matrixFrontX, matrixFrontY, ref m_tileMoveDesc[j].tileCoord);
				worldPosForTileCoord(m_tileMoveDesc[j].tileCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			callback(m_tileMoveDesc);
		}
	}
}
