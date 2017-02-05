using System.Collections.Specialized;
using System.ComponentModel;

namespace KSoft.Blam.Megalo.Model
{
	partial class MegaloScriptAccessibleObjectBase
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		protected void NotifyPropertiesChanged(PropertyChangedEventArgs[] argsList, int startIndex = 0)
		{
			PropertyChanged.SafeNotify(this, argsList, startIndex);
		}
		#endregion

		static readonly PropertyChangedEventArgs kCodeNameChanged =
			ObjectModel.Util.CreatePropertyChangedEventArgs((MegaloScriptAccessibleObjectBase x) => x.CodeName);
	};
}