using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Blam.Engine
{
	using EngineSystemCtorSignature = Func<EngineSystemBase>;

	/// <summary>Annotates a type as being an interface for</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
	public sealed class EngineSystemAttribute : Attribute
	{
		#region SystemGuid util
		const string kSystemGuidPropertyName = "SystemGuid";

		static PropertyInfo GetSystemGuidPropertyInfo(Type systemType)
		{
			const BindingFlags k_property_binding_flags
				= BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.DeclaredOnly;

			return (
					from prop in systemType.GetProperties(k_property_binding_flags)
					where prop.Name == kSystemGuidPropertyName
					select prop
				).FirstOrDefault();
		}

		/// <summary>Get the system guid from an <see cref="EngineSystemBase"/> type</summary>
		/// <param name="systemType"></param>
		/// <returns></returns>
		internal static Values.KGuid GetSystemGuid(Type systemType)
		{
			Contract.Requires(systemType != null && systemType.IsSubclassOf(typeof(EngineSystemBase)));

			var guid_prop = GetSystemGuidPropertyInfo(systemType);

			return (Values.KGuid)guid_prop.GetValue(null);
		}
		/// <summary>Get the system guid from an <see cref="EngineSystemBase"/> type</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal static Values.KGuid GetSystemGuid<T>()
			where T : EngineSystemBase
		{
			var system_type = typeof(T);
			return GetSystemGuid(system_type);
		}
		#endregion

		public Type EngineSystemType { get; private set; }

		public Values.KGuid Guid { get; private set; }

		EngineSystemCtorSignature mFactoryMethod;

		#region Parameters
		/// <summary>
		/// Do instances of this system require external definitions in memory to function? Default is <b>true</b>.
		/// </summary>
		public bool RequiresExternsFile { get; set; }

		/// <summary>
		/// Should instances of this system keep the external definitions in memory once loaded, even after all
		/// handles to this system are closed? Default is <b>false</b>.
		/// </summary>
		public bool KeepExternsLoaded { get; set; }
		#endregion

		public EngineSystemAttribute()
		{
			RequiresExternsFile = true;
			KeepExternsLoaded = false;
		}

		#region Overrides
		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is EngineSystemAttribute && ((EngineSystemAttribute)obj).Guid == Guid;
		}

		public override string ToString()
		{
			return Guid.ToString(Values.KGuid.kFormatHyphenated);
		}
		#endregion
		
		/// <summary>For unit testing purposes only. Verifies this instance was properly initialized</summary>
		internal bool IsValid { get {
			return
				mFactoryMethod != null;
		} }

		/// <summary>Create a new instance of the system. Only call me if you are <see cref="BlamEngine"/>!</summary>
		/// <typeparam name="T"><see cref="EngineSystemBase"/> type</typeparam>
		/// <returns></returns>
		internal EngineSystemBase NewInstance()
		{
			Contract.Assert(mFactoryMethod != null, "Rerun engine unit tests");

			return mFactoryMethod();
		}

		#region InitializeForNewProgram
		void FindSystemGuid()
		{
			var guid_prop = GetSystemGuidPropertyInfo(EngineSystemType);

			#region validate guid_prop
			if (guid_prop == null)
				throw new InvalidOperationException(string.Format(
					"{0} doesn't specify a static property named {1}",
					EngineSystemType.Name, kSystemGuidPropertyName));
			else if (guid_prop.PropertyType != typeof(Values.KGuid))
				throw new InvalidOperationException(string.Format(
					"{0}'s {1} isn't defined as a {2}",
					EngineSystemType.Name, kSystemGuidPropertyName, typeof(Values.KGuid).Name));
			#endregion

			#region Get and validate Guid
			Guid = (Values.KGuid)guid_prop.GetValue(null);

			if(Guid == Values.KGuid.Empty)
				throw new InvalidOperationException(string.Format(
					"{0}'s {1} is Empty, this isn't allowed",
					EngineSystemType.Name, kSystemGuidPropertyName));
			#endregion
		}
		void AttachToSystemType(Type systemType)
		{
			// Validate the type is valid for being described as a engine system
			if (!systemType.IsSubclassOf(typeof(EngineSystemBase)))
				throw new System.InvalidOperationException(string.Format(
					"{0} is applied to {1} which is not a subclass of {2}",
					typeof(EngineSystemAttribute).Name, systemType.Name, typeof(EngineSystemBase).Name));

			EngineSystemType = systemType;

			FindSystemGuid();
		}
		
		void FindEngineSystemFactoryMethod()
		{
			const BindingFlags k_factory_method_binding_flags =
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			try
			{
				mFactoryMethod = Reflection.Util.GenerateConstructorFunc<EngineSystemBase, EngineSystemCtorSignature>(
					EngineSystemType, k_factory_method_binding_flags);
			}
			catch(Exception ex)
			{
				Debug.Trace.Engine.TraceInformation("Failed to get factory method for {0}: {1}",
					EngineSystemType, ex);
			}
		}
		
		void RegisterSystem()
		{
			if (Engine.EngineRegistry.Systems.ContainsKey(Guid))
				throw new InvalidOperationException(string.Format(
					"An engine system other than {0} is already registered with the {1} guid",
					EngineSystemType.Name, Guid.ToString(Values.KGuid.kFormatHyphenated)));

			EngineRegistry.Register(this);
		}
		void InitializeForNewProgram(Type systemType)
		{
			Contract.Assume(systemType != null);

			AttachToSystemType(systemType);
			FindEngineSystemFactoryMethod();

			RegisterSystem();
		}
		#endregion

		/// <summary>Process the new assembly for types which have this attribute applied to them</summary>
		/// <param name="assembly"></param>
		internal static void InitializeForNewAssembly(Assembly assembly)
		{
			Contract.Requires<ArgumentNullException>(assembly != null);

			var attrs = from type in assembly.GetTypes()
						// attribute should only be applied to non-abstract classes
						where type.IsClass && !type.IsAbstract
						// there should actually only be one result...
						from attribute in type.GetCustomAttributes<EngineSystemAttribute>()
						select new KeyValuePair<Type, EngineSystemAttribute>(type, attribute);

			foreach (var esa in attrs)
				esa.Value.InitializeForNewProgram(esa.Key);
		}
	};
}