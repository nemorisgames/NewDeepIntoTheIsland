/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"InvBin.cs"
 * 
 *	This script is a container class for inventory item categories.
 * 
 */


namespace AC
{

	/**
	 * A data container for an inventory item category.
	 */
	[System.Serializable]
	public class InvBin
	{

		/** The category's editor name */
		public string label;
		/** A unique identifier */
		public int id;


		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "idArray">An array of already-used ID numbers, so that a unique one can be generated</param>
		 */
		public InvBin (int[] idArray)
		{
			id = 0;

			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}

			label = "Category " + (id + 1).ToString ();
		}

	}

}