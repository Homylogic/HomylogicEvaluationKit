using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Schedule;
using static X.Homylogic.Models.Schedule.ScheduleRecord;

namespace HomylogicAsp.Models.Devices.Homyoko
{
    public class EditScheduleIVTControllerViewModel : ScheduleViewModel
    {
        public Exception SaveException;
        public Int64 ScheduleID { get; set; }
        public DateTime ScheduleTime { get; set; }
        public bool DayMonday { get; set; }
        public bool DayTuesday { get; set; }
        public bool DayWednesday { get; set; }
        public bool DayThursday { get; set; }
        public bool DayFriday { get; set; }
        public bool DaySaturday { get; set; }
        public bool DaySunday { get; set; }
        public ActionTypes Action { get; set; }
        public string MethodName { get; set; }

        public EditScheduleIVTControllerViewModel()
        {
            this.SetViewModel(new ScheduleRecord(Body.Database.DBClient, this.OwnerID));
        }
        public EditScheduleIVTControllerViewModel(ScheduleRecord scheduleRecord)
        {
            this.SetViewModel(scheduleRecord);
        }
        public ScheduleRecord GetDeviceScheduleRecord()
        {
            ScheduleRecord scheduleRecord;
            if (this.ScheduleID > 0)
            {
                DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(this.OwnerID);
                scheduleRecord = (ScheduleRecord)device.Scheduler.FindDataRecord(this.ScheduleID);
            }
            else {
                DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(this.OwnerID);
                scheduleRecord = (ScheduleRecord)device.Scheduler.GetInitializedDataRecord();
            }
            scheduleRecord.ScheduleTime = this.ScheduleTime;
            scheduleRecord.Action = ActionTypes.CallMethod;
            scheduleRecord.MethodName = this.MethodName;
            scheduleRecord.DayMonday = this.DayMonday;
            scheduleRecord.DayTuesday = this.DayTuesday;
            scheduleRecord.DayWednesday = this.DayWednesday;
            scheduleRecord.DayThursday = this.DayThursday;
            scheduleRecord.DayFriday = this.DayFriday;
            scheduleRecord.DaySaturday = this.DaySaturday;
            scheduleRecord.DaySunday = this.DaySunday;
            return scheduleRecord;
        }
        public List<KeyValuePair<string, string>> GetDeviceActions() 
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(this.OwnerID);
            if (device != null) 
            {
                MethodInfo[] methods = device.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (MethodInfo method in methods) 
                {
                    DescriptionAttribute description = method.GetCustomAttribute<DescriptionAttribute>();
                    if (description != null) 
                    {
                        result.Add(new KeyValuePair<string, string>(method.Name, description.Description));
                    }
                }
            }
            return result;
        }

        private void SetViewModel(ScheduleRecord scheduleRecord) 
        {
            this.ScheduleID = scheduleRecord.ID;
            this.ScheduleTime = scheduleRecord.ScheduleTime;
            this.Action = scheduleRecord.Action;
            this.MethodName = scheduleRecord.MethodName;
            this.DayMonday = scheduleRecord.DayMonday;
            this.DayTuesday = scheduleRecord.DayTuesday;
            this.DayWednesday = scheduleRecord.DayWednesday;
            this.DayThursday = scheduleRecord.DayThursday;
            this.DayFriday = scheduleRecord.DayFriday;
            this.DaySaturday = scheduleRecord.DaySaturday;
            this.DaySunday = scheduleRecord.DaySunday;
        }

    }
}
