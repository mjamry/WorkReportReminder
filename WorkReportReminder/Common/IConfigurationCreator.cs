﻿namespace WorkReportReminder.SettingsManagement
{
    public interface IConfigurationCreator
    {
        /// <summary>
        /// Create configuration obect used to initialise time management service.
        /// </summary>
        TimeGuardConfiguration TimeGuardConfiguration();
    }
}