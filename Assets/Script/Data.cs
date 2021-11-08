using System.Collections.Generic;
using BreakInfinity;

public class Data
{
	public BigDouble Elon;
	public List<BigDouble> clickUpgradeLevel;

	public Data()
	{
		Elon = 0;
		clickUpgradeLevel = Methods.CreateList<BigDouble>(3);
	}
}
