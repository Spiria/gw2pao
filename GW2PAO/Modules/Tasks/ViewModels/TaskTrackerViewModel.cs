﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GW2PAO.API.Data.Enums;
using GW2PAO.Modules.Tasks.Interfaces;
using GW2PAO.Modules.Tasks.Models;
using GW2PAO.Modules.Tasks.Views;
using GW2PAO.PresentationCore;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Win32;
using NLog;

namespace GW2PAO.Modules.Tasks.ViewModels
{
    /// <summary>
    /// Task Tracker view model class
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    public class TaskTrackerViewModel : BindableBase
    {
        /// <summary>
        /// Default logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The player tasks controller
        /// </summary>
        private IPlayerTasksController controller;

        /// <summary>
        /// Container object for composing parts
        /// </summary>
        private CompositionContainer container;

        /// <summary>
        /// Collection of player tasks
        /// </summary>
        public AutoRefreshCollectionViewSource PlayerTasks
        {
            get;
            private set;
        }

        /// <summary>
        /// The player tasks user data
        /// </summary>
        public TasksUserData UserData
        {
            get { return this.controller.UserData; }
        }

        /// <summary>
        /// True if the selected distance units are Feet, else false
        /// </summary>
        public bool IsFeetSelected
        {
            get { return this.UserData.DistanceUnits == Units.Feet; }
            set
            {
                if (value)
                {
                    this.UserData.DistanceUnits = Units.Feet;
                    this.OnPropertyChanged(() => this.IsFeetSelected);
                    this.OnPropertyChanged(() => this.IsMetersSelected);
                }
            }
        }

        /// <summary>
        /// True if the selected distance units are Meters, else false
        /// </summary>
        public bool IsMetersSelected
        {
            get { return this.UserData.DistanceUnits == Units.Meters; }
            set
            {
                if (value)
                {
                    this.UserData.DistanceUnits = Units.Meters;
                    this.OnPropertyChanged(() => this.IsFeetSelected);
                    this.OnPropertyChanged(() => this.IsMetersSelected);
                }
            }
        }

        /// <summary>
        /// True if the tasks should be sorted by Name, else false
        /// </summary>
        public bool SortByName
        {
            get
            {
                return this.UserData.TaskTrackerSortProperty == TasksUserData.TASK_TRACKER_SORT_NAME;
            }
            set
            {
                if (this.UserData.TaskTrackerSortProperty != TasksUserData.TASK_TRACKER_SORT_NAME)
                {
                    this.OnSortingPropertyChanged(TasksUserData.TASK_TRACKER_SORT_NAME, ListSortDirection.Ascending);
                }
            }
        }

        /// <summary>
        /// True if the tasks should be sorted by Distance, else false
        /// </summary>
        public bool SortByDistance
        {
            get
            {
                return this.UserData.TaskTrackerSortProperty == TasksUserData.TASK_TRACKER_SORT_DISTANCE;
            }
            set
            {
                if (this.UserData.TaskTrackerSortProperty != TasksUserData.TASK_TRACKER_SORT_DISTANCE)
                {
                    this.OnSortingPropertyChanged(TasksUserData.TASK_TRACKER_SORT_DISTANCE, ListSortDirection.Ascending);
                }
            }
        }

        /// <summary>
        /// Command to add a new task to the task list
        /// </summary>
        public ICommand AddNewTaskCommand { get; private set; }

        /// <summary>
        /// Command to delete all tasks from the task list
        /// </summary>
        public ICommand DeleteAllCommand { get; private set; }

        /// <summary>
        /// Command to load tasks from a file
        /// </summary>
        public ICommand LoadTasksCommand { get; private set; }

        /// <summary>
        /// Command to import all tasks from a file
        /// </summary>
        public ICommand ImportTasksCommand { get; private set; }

        /// <summary>
        /// Command to export all tasks to a file
        /// </summary>
        public ICommand ExportTasksCommand { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="playerTasksController">The player tasks controller</param>
        [ImportingConstructor]
        public TaskTrackerViewModel(
            IPlayerTasksController playerTasksController,
            CompositionContainer container)
        {
            this.controller = playerTasksController;
            this.container = container;

            this.AddNewTaskCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(this.AddNewTask);
            this.DeleteAllCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(this.DeleteAllTasks);
            this.LoadTasksCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(this.LoadTasks);
            this.ImportTasksCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(this.ImportTasks);
            this.ExportTasksCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(this.ExportTasks);

            var collectionViewSource = new AutoRefreshCollectionViewSource();
            collectionViewSource.Source = this.controller.PlayerTasks;
            this.PlayerTasks = collectionViewSource;

            switch (this.UserData.TaskTrackerSortProperty)
            {
                case TasksUserData.TASK_TRACKER_SORT_NAME:
                    this.OnSortingPropertyChanged(TasksUserData.TASK_TRACKER_SORT_NAME, ListSortDirection.Ascending);
                    break;
                case TasksUserData.TASK_TRACKER_SORT_DISTANCE:
                    this.OnSortingPropertyChanged(TasksUserData.TASK_TRACKER_SORT_DISTANCE, ListSortDirection.Ascending);
                    break;
                default:
                    this.OnSortingPropertyChanged(TasksUserData.TASK_TRACKER_SORT_NAME, ListSortDirection.Ascending);
                    break;
            }
        }

        /// <summary>
        /// Adds a new task to the collection of tasks
        /// </summary>
        private void AddNewTask()
        {
            logger.Info("Displaying add new task dialog");
            AddNewTaskDialog dialog = new AddNewTaskDialog();
            this.container.ComposeParts(dialog);
            dialog.Show();
        }

        /// <summary>
        /// Deletes all tasks from the collection of tasks
        /// </summary>
        private void DeleteAllTasks()
        {
            logger.Info("Deleting all tasks");
            var tasksToDelete = new List<PlayerTaskViewModel>(this.controller.PlayerTasks);
            foreach (var pt in tasksToDelete)
            {
                this.controller.DeleteTask(pt.Task);
            }
        }

        /// <summary>
        /// Imports a file containing tasks
        /// </summary>
        private void LoadTasks()
        {
            logger.Info("Loading tasks");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckPathExists = true;
            openFileDialog.Filter = "Player Task Files (*.xml)|*.xml";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                this.controller.LoadTasksFile(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Imports a file containing tasks
        /// </summary>
        private void ImportTasks()
        {
            logger.Info("Importing tasks");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckPathExists = true;
            openFileDialog.Filter = "Player Task Files (*.xml)|*.xml";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                this.controller.ImportTasks(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Exports a file containing tasks
        /// </summary>
        private void ExportTasks()
        {
            logger.Info("Exporting tasks");

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "Player Task Files (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                this.controller.ExportTasks(saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Handles updating the sorting descriptions of the Objectives collection
        /// and raising INotifyPropertyChanged for all sort properties
        /// </summary>
        private void OnSortingPropertyChanged(string property, ListSortDirection direction)
        {
            this.PlayerTasks.SortDescriptions.Clear();
            this.PlayerTasks.SortDescriptions.Add(new SortDescription(property, direction));
            this.PlayerTasks.View.Refresh();

            this.UserData.TaskTrackerSortProperty = property;
            this.OnPropertyChanged(() => this.SortByName);
            this.OnPropertyChanged(() => this.SortByDistance);
        }
    }
}
