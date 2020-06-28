using UnityEngine.Events;

namespace warcode.GlobalKeys.Windows
{
	[System.Serializable]
	public class GlobalKeyEvent : UnityEvent<KeyModifier, Keys>
	{
	}
}
