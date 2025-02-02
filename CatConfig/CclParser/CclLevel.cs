public struct CclLevel
{
	private readonly object lck = new();
	private readonly int steps;
	private int stepCount = 0;
	private int level = 0;

	public CclLevel(int step)
	{
		this.steps = step;
	}


	public void Step()
	{
		lock (lck)
		{
			stepCount++;

			if (stepCount == steps)
			{
				level++;
				stepCount = 0;
			}
		}
	}

	public void HardReset()
	{
		if (stepCount == 0 && level == 0)
			return;

		lock (lck)
		{
			stepCount = 0;
			level = 0;
		}
	}

	public void Reset()
	{
		if (stepCount == 0)
			return;

		lock (lck)
			stepCount = 0;
	}

	public static implicit operator int(CclLevel cLevel) => cLevel.level;

}