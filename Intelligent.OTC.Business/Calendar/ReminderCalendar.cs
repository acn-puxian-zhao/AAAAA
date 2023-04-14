using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Repository;
using Intelligent.OTC.Domain.DataModel;

namespace Intelligent.OTC.Business
{
    public class ReminderCalendar
    {
        /// <summary>
        /// Get tracking for the given reminders(pair of an customer and legal entity)
        /// </summary>
        /// <param name="reminders"></param>
        /// <returns></returns>
        public CurrentTracking GetTracking(List<CollectorAlert> reminders, CurrentTracking appendTracking = null)
        {
            // Build tracking from reminders
            CurrentTracking ct = null;
            if (appendTracking != null)
            {
                ct = appendTracking;
            }
            else
            {
                ct = new CurrentTracking();
            }
            
            foreach (var Reminder in reminders)
            {
                if (Reminder.AlertType == 1)
                {
                    ct.SoaId = Reminder.Id;
                    ct.SoaDate = Reminder.ActionDate;
                    if (Reminder.Status == "Finish")
                    {
                        ct.SoaStatus = 1;
                    }
                    else
                    {
                        if (Reminder.OrginalActionDate < AppContext.Current.User.Now)
                        {
                            ct.SoaStatus = 0;
                        }
                        else
                        {
                            ct.SoaStatus = 2;
                        }
                    }
                }

                if (Reminder.AlertType == 2)
                {
                    //Reminder2th
                    ct.R2Id = Reminder.Id;
                    ct.Reminder2thDate = Reminder.ActionDate;
                    if (Reminder.Status == "Finish")
                    {
                        ct.Reminder2thStatus = 1;
                    }
                    else
                    {
                        if (Reminder.OrginalActionDate < AppContext.Current.User.Now)
                        {
                            ct.Reminder2thStatus = 0;
                        }
                        else
                        {
                            ct.Reminder2thStatus = 2;
                        }
                    }
                }
                //Reminder3th
                if (Reminder.AlertType == 3)
                {
                    ct.R3Id = Reminder.Id;
                    ct.Reminder3thDate = Reminder.ActionDate;
                    if (Reminder.Status == "Finish")
                    {
                        ct.Reminder3thStatus = 1;
                    }
                    else
                    {
                        if (Reminder.OrginalActionDate < AppContext.Current.User.Now)
                        {
                            ct.Reminder3thStatus = 0;
                        }
                        else
                        {
                            ct.Reminder3thStatus = 2;
                        }
                    }
                }


                if (Reminder.AlertType == 4)
                {
                    ct.HoldId = Reminder.Id;
                    ct.HoldDate = Reminder.ActionDate;
                    if (Reminder.Status == "Finish")
                    {
                        ct.HoldStatus = 1;
                    }
                    else
                    {
                        if (Reminder.OrginalActionDate < AppContext.Current.User.Now)
                        {
                            ct.HoldStatus = 0;
                        }
                        else
                        {
                            ct.HoldStatus = 2;
                        }
                    }
                }
            }

            return ct;
        }

    }
}
