using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
	/// <summary>
	/// Base class for plugins which handle DX resources.
	/// </summary>
	public abstract class DXResourcePluginBase<T> : IPluginDXResource where T : DeviceData
	{
		//the device datas per device
		protected Dictionary<int, T> FDeviceData;
		
		public DXResourcePluginBase()
		{
			FDeviceData = new Dictionary<int, T>();
		}
		
		/// <summary>
		/// Set all device data to update in this frame.
		/// </summary>
		protected void Update()
		{
			foreach (var dd in FDeviceData.Values) dd.Update = true;
		}
		
		/// <summary>
		/// Set all device data to recreate in this frame.
		/// </summary>
		protected void Reinitialize()
		{
			foreach (var dd in FDeviceData.Values) dd.Recreate = true;
		}
		
		protected abstract T CreateDeviceData(Device device);
		protected abstract void UpdateDeviceData(T deviceData);
		protected abstract void DestroyDeviceData(T deviceData, bool OnlyUnManaged);
		
		private void CreateResource(IPluginOut ForPin, int OnDevice)
		{
			//destroy resource if it exists
			if (FDeviceData.ContainsKey(OnDevice)) DestroyResource(ForPin, OnDevice, false);
			
			//call user create method to set the new device data
			var dev = Device.FromPointer(new IntPtr(OnDevice));
			FDeviceData.Add(OnDevice, CreateDeviceData(dev));
			dev.Dispose();
		}
		
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			if (!FDeviceData.ContainsKey(OnDevice))
			{
				//create resource for this device
				CreateResource(ForPin, OnDevice);
			}
			else
			{
				//recreate data?
				if (FDeviceData[OnDevice].Recreate) CreateResource(ForPin, OnDevice);
				
				//update data?
				var dd = FDeviceData[OnDevice];
				if (dd.Update)
				{
					UpdateDeviceData(dd);
					dd.Update = false;
				}
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			if (FDeviceData.ContainsKey(OnDevice))
			{
				DestroyDeviceData(FDeviceData[OnDevice], OnlyUnManaged);
				FDeviceData.Remove(OnDevice);
			}
		}
	}
}
