﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WorkReportReminder.Common;
using WorkReportReminder.Common.Logger;

namespace WorkReportReminder.DataManagement.XmlData
{
    class XmlDataWriter : IDataWriter
    {
        private readonly IDataReader _reportFileReader;

        public XmlDataWriter(IDataReader reader)
        {
            _reportFileReader = reader;
        }

        #region Implementation of IDataWriter

        /// <summary>
        /// Write work items data to specified file.
        /// </summary>
        public void Write(string filePath, WorkItemDto singleWorkItemData)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                CreateOutputFile(filePath);
            }

            var allWorkItemsData = _reportFileReader.ReadAllItems(filePath);
            var workItem = FindWorkItem(singleWorkItemData, allWorkItemsData);

            if (workItem == null)
            {
                Log.Instance.Info("Writing new item to the file");
                var newWorkItem = new WorkItem(singleWorkItemData.Id, singleWorkItemData.Title,
                                                    singleWorkItemData.Time, singleWorkItemData.Comment);
                allWorkItemsData.Add(newWorkItem);

                //update end time of previous work item when adding new one.
                UpdatePreviousItemEndTime(singleWorkItemData.Time, allWorkItemsData);
            }
            else
            {
                Log.Instance.Info("Updating existing item");
                if (workItem.LastComment.CompareTo(singleWorkItemData.Comment) == 0)
                {
                    workItem.LastComment.SetEndTime(singleWorkItemData.Time);
                }
                else
                {
                    workItem.AddComment(singleWorkItemData.Comment, singleWorkItemData.Time);
                }
            }

            UpdateReportFile(filePath, allWorkItemsData);
        }

        /// <summary>
        /// Updates end time of previous work item.
        /// </summary>
        private static void UpdatePreviousItemEndTime(DateTime time, WorkItemsList allWorkItemsData)
        {
            allWorkItemsData.LastItem.LastComment.SetEndTime(time);
        }

        #endregion

        /// <summary>
        /// Search in list for item with the same id and title as specified one.
        /// Returns null if does not find matching item.
        /// </summary>
        private WorkItem FindWorkItem(WorkItemDto item, WorkItemsList allItems)
        {
            var workItem = allItems.Find(wi => wi.Id == item.Id && wi.Title == item.Title);
            return workItem;
        }

        /// <summary>
        /// Update file, using data of all existing work items.
        /// </summary>
        private static void UpdateReportFile(string filePath, WorkItemsList workItemsData)
        {
            var workItemsDocument = new XDocument(
                new XElement(XmlElements.WorkItems.ToString(),
                             from workItem in workItemsData
                             select new XElement(
                                 XmlElements.WorkItem,
                                 new XElement(XmlElements.Id, workItem.Id),
                                 new XElement(XmlElements.Title, workItem.Title),
                                 from comment in workItem.Comments
                                     select new XElement(XmlElements.Comment, 
                                         new XAttribute(XmlElements.StartTime, comment.StartTime), 
                                         new XAttribute(XmlElements.EndTime, comment.EndTime), 
                                         comment.Title
                                 )
                    )
                )
            );

            workItemsDocument.Save(filePath);
        }

        /// <summary>
        /// Creates empty report file.
        /// </summary>
        private void CreateOutputFile(string filePath)
        {
            Log.Instance.Info("New output file created.");
            var newXml = new XDocument(
                new XElement(XmlElements.WorkItems.ToString()));
            newXml.Save(filePath, SaveOptions.None);
        }
    }
}
