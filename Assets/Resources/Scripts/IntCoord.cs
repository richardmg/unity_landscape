﻿
public class IntCoord
{
	public int x;
	public int y;

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

	public void flip()
	{
		int tmp = this.x;
		this.x = this.y;
		this.y = tmp;
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}
}
