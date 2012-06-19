﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using WorkReportReminder.Common;

namespace WorkReportReminder.DataManagement
{
    class XmlDataWriter : IDataWriter
    {
        #region Implementation of IDataWriter

        /// <summary>
        /// Write work items data to specified file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="workItemsData"></param>
        /// <returns></returns>
        public bool Write(string filePath, List<WorkItemDto> workItemsData)
        {
            XDocument workItemsDocument = XDocument.Load(filePath);

            if (workItemsDocument != null)
            {
                List<WorkItemDto> fileData = (
                                                 from workItem in workItemsDocument.Root.Elements("WorkItem")
                                                 select new WorkItemDto
                                                     (
                                                     int.Parse(workItem.Element("ID").Value),
                                                     workItem.Element("Title").Value,
                                                     workItem.Element("Comment").Value,
                                                     DateTime.Parse(workItem.Element("Time").Value)
                                                     )
                                             ).ToList<WorkItemDto>();

                workItemsData.AddRange(fileData);
            }

            workItemsDocument = new XDocument(
                new XElement("WorkItems",
                             from workItem in workItemsData
                             select new XElement(
                                 "WorkItem",
                                 new XElement("ID", workItem.Id),
                                 new XElement("Title", workItem.Title),
                                 new XElement("Comment", workItem.Comment),
                                 new XElement("Time", DateTime.Now)
                                 )
                    )
                );

            workItemsDocument.Save(filePath);
            return true;
        }

        #endregion
    }
}
