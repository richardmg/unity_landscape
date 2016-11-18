
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
