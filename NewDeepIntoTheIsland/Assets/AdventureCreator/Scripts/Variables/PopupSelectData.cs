#if UNITY_EDITOR

namespace AC
{

	public struct PopupSelectData
	{

		public int ID;
		public string label;
		public int rootIndex;


		public PopupSelectData (int _ID, string _label, int _rootIndex)
		{
			ID = _ID;
			label = _label;
			rootIndex = _rootIndex;
		}

	}
}

#endif