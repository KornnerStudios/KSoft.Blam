using System.ComponentModel;

namespace KSoft.Blam.Games.Halo4.RuntimeData.Variants
{
	partial class WeaponTuningBarrelModifierData
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs[] kValueChanged;

		static WeaponTuningBarrelModifierData()
		{
			kValueChanged = [
				kValue0ChangedEventArgs,
				kValue1ChangedEventArgs,
				kValue2ChangedEventArgs,
				kValue3ChangedEventArgs,
				kValue4ChangedEventArgs,
				kValue5ChangedEventArgs,
				kValue6ChangedEventArgs,
				kValue7ChangedEventArgs,
				kValue8ChangedEventArgs,
				kValue9ChangedEventArgs,
				kValue10ChangedEventArgs,
				kValue11ChangedEventArgs,
				kValue12ChangedEventArgs,
				kValue13ChangedEventArgs,
				kValue14ChangedEventArgs,
				kValue15ChangedEventArgs,
				kValue16ChangedEventArgs,
				kValue17ChangedEventArgs,
				kValue18ChangedEventArgs,
				kValue19ChangedEventArgs,
				kValue20ChangedEventArgs,
				kValue21ChangedEventArgs,
				kValue22ChangedEventArgs,
				kValue23ChangedEventArgs,
				kValue24ChangedEventArgs,
				kValue25ChangedEventArgs,
				kValue26ChangedEventArgs,
			];
		}
	};

	partial class WeaponTuningWeaponModifierData
		: INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler? PropertyChanged;

		void NotifyPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged.SafeNotify(this, args);
		}
		#endregion

		static readonly PropertyChangedEventArgs[] kValueChanged;

		static WeaponTuningWeaponModifierData()
		{
			kValueChanged = [
				kValue0ChangedEventArgs,
				kValue1ChangedEventArgs,
				kValue2ChangedEventArgs,
				kValue3ChangedEventArgs,
				kValue4ChangedEventArgs,
				kValue5ChangedEventArgs,
				kValue6ChangedEventArgs,
				kValue7ChangedEventArgs,
				kValue8ChangedEventArgs,
				kValue9ChangedEventArgs,
				kValue10ChangedEventArgs,
				kValue11ChangedEventArgs,
				kValue12ChangedEventArgs,
				kValue13ChangedEventArgs,
			];
		}
	};
}
