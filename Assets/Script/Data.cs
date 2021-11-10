using System.Collections.Generic;
using System.Linq;
using BreakInfinity;

public class Data
{
	public BigDouble Elon;
	
	public List<int> clickUpgradeLevel;
	public List<int> productionUpgradeLevel;
	public Data()
	{
		Elon = 0;
		
		clickUpgradeLevel = new int[4].ToList();
		productionUpgradeLevel = new int[4].ToList();
	}
}
