using UnityEngine.Events;

namespace warcode.GlobalKeys.Windows
{
	public class KeyRegistration
	{
		public KeyModifier Modifier { get; private set; }
		public Keys Key { get; private set; }
		public UnityAction<KeyModifier, Keys> Listener { get; private set; }
		public bool Registered { get; set; }

		public KeyRegistration(KeyModifier m, Keys k, UnityAction<KeyModifier, Keys> l)
		{
			Modifier = m;
			Key = k;
			Listener = l;
			Registered = false;
		}
	}
}